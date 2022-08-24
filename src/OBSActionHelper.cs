using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using OBSWebsocketDotNet;

namespace OBSRemoteControls
{
    public static class OBSActionHelper
    {
        private static readonly BindingFlags METHOD_BINDING_FLAGS_MASK =
            BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.DeclaredOnly;
        private static readonly ParameterAttributes PARAMETER_FLAGS_MASK =
            ParameterAttributes.In
            | ParameterAttributes.Out
            | ParameterAttributes.Lcid
            | ParameterAttributes.Retval
            | ParameterAttributes.HasFieldMarshal
            | ParameterAttributes.Reserved3
            | ParameterAttributes.Reserved4;

        public static readonly IReadOnlyDictionary<string, ParameterInfo[]> actions = GetMethods();

        private static IReadOnlyDictionary<string, ParameterInfo[]> GetMethods()
        {
            Dictionary<string, ParameterInfo[]> filteredMethods = new();

            MethodInfo[] methods = typeof(OBSWebsocket).GetMethods(METHOD_BINDING_FLAGS_MASK);
            foreach (MethodInfo methodInfo in methods)
            {
                //Make sure we only get void methods.
                if (methodInfo.ReturnType != typeof(void)) continue;

                //Make sure we don't add any duplicate keys.
                int i = 0;
                string key = methodInfo.Name;
                while (filteredMethods.ContainsKey(key)) key = ++i + methodInfo.Name;

                //Make sure that we only add methods with primitive types.
                ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                if (parameterInfos.Any(p =>
                    p.Name == null
                    || (
                        !p.ParameterType.IsPrimitive
                        && p.ParameterType != typeof(string)
                       )
                    || p.Attributes == PARAMETER_FLAGS_MASK
                )) continue;

                //Add the method to the filtered methods dictionary.
                filteredMethods.Add(key, parameterInfos);
            }

            //Return the filtered methods dictionary.
            return filteredMethods;
        }

        public static bool BuildAction(string methodName, Dictionary<string, object> args, out Action<OBSWebsocket> action)
        {
            action = (_) => { };

            //Make sure that the method name is valid.
            if (!actions.ContainsKey(methodName)) return false;

            ParameterInfo[] methodParameters = actions[methodName];

            //Make sure the given argument names are valid.
            if (!args.All(arg => methodParameters.Any(p => p.Name == arg.Key))) return false;

            //Get the method.
            MethodInfo? method = typeof(OBSWebsocket).GetMethod(methodName, METHOD_BINDING_FLAGS_MASK,
                methodParameters.Select(p => p.ParameterType).ToArray());
            if (method == null) return false;

            //Format the args.
            List<object?> formattedArgs = new();
            foreach (ParameterInfo parameter in methodParameters)
            {
                object? convertedValue = null;

                if (args.TryGetValue(parameter.Name!, out object? value))
                {
                    //Get the converter for the passed user defined argument.
                    TypeConverter converter = TypeDescriptor.GetConverter(value.GetType());

                    //Check if the user defined argument can be converted to the method parameter type.
                    //I am assuming that this works for null conversion too?
                    if (converter.CanConvertTo(parameter.ParameterType)) convertedValue = converter.ConvertTo(value, parameter.ParameterType);
                    else return false;
                }
                //If we couldn't find a user defined value then see if we can set a default.
                else if (parameter.HasDefaultValue) convertedValue = parameter.DefaultValue;
                //Otherwise check if this parameter is allowed to be optional, if it isn't then return false.
                else if (!parameter.IsOptional) return false;

                //Add the converted value to the formatted arguments list.
                formattedArgs.Add(convertedValue);
            }

            //Build the action.
            action = (obsWebsocket) => method.Invoke(obsWebsocket, formattedArgs.ToArray());

            return true;
        }
    }
}
