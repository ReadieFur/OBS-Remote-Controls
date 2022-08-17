using System.Reflection;
using CSharpTools.ConsoleExtensions;
using GlobalInputHook.Tools;
using GlobalInputHook.Objects;
using OBSRemoteControlsCustom;
using OBSRemoteControlsCustom.Configuration;
using OBSWebsocketDotNet;
using Newtonsoft.Json;

#region Program
#if DEBUG
Logger.logLevel = ELogLevel.Trace;
BuildSampleData();
#endif

#region Variables
bool exiting = false;
SConfiguration configuration;
OBSWebsocket obsWebsocket;
HookClientHelper hookClientHelper;
Dictionary<SOBSMacro, List<Action<OBSWebsocket>>> macros = new();
#endregion

#region Initialization
#if RELEASE
if (Environment.GetCommandLineArgs().Contains("--trace")) Logger.logLevel = ELogLevel.Trace;
#endif

if (!ConfigurationHelper.LoadConfiguration(out configuration))
{
    await Logger.Error("Failed to load configuration file.");
    Environment.Exit(-1);
}
await Logger.Trace($"Loaded configuration:\n" + JsonConvert.SerializeObject(configuration, Formatting.Indented));
if (configuration.macros.Count == 0)
{
    await Logger.Warning("No macros are defined, exiting...");
    Environment.Exit(-1);
}

obsWebsocket = new OBSWebsocket();
obsWebsocket.Connected += ObsWebsocket_Connected;
obsWebsocket.Disconnected += ObsWebsocket_Disconnected;
/*await Logger.Info("Connecting to OBS websocket...");
//Don't await this this first time around, prevents hanging on load.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
Task.Run(async () =>
{
    while (!await ConnectOBSWebsocket())
    {
        //Find out why this is blocking here.
        await Logger.Info("Reconnecting to OBS websocket...");
        await Task.Delay(1000);
    }
});*/
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

hookClientHelper = HookClientHelper.GetOrCreateInstance("obs_remote_controls_custom", configuration.hookUpdateRateMS);
hookClientHelper.onData += HookClientHelper_onData;

AppDomain.CurrentDomain.ProcessExit += (_, _) => Unload();
#endregion

#region Load configuration options
foreach (SOBSMacro macro in configuration.macros)
{
    if (macros.ContainsKey(macro))
    {
        await Logger.Warning($"Duplicate macro detected '{macro}'.");
        continue;
    }

    List<Action<OBSWebsocket>> actions = new();

    foreach (SMethodData action in macro.actions)
    {
        if (OBSAction.BuildAction(action.method, action.parameters, out Action<OBSWebsocket> method)) actions.Add(method);
        else await Logger.Warning($"Failed to load action '{action.method}'.");
    }

    macros.Add(macro, actions);
}
#endregion

await ConnectOBSWebsocket();

#region UI (CLI) loop
Dictionary<string, Action> options = new Dictionary<string, Action>()
{
    {
        "exit",
        () => {} //Handle unload after the UI loop.
    }
};
while (true)
{
    string userInput = string.Empty;
    try { userInput = Input.GetString(true, options.Keys.ToArray()); }
    catch (IOException) { break; } //The terminal has closed.
    catch { continue; }
    options[userInput]();
    if (userInput == "exit") break;
}
#endregion

Unload();
#endregion

//====

#region Methods
void Unload()
{
    exiting = true;
    hookClientHelper?.Dispose();
    obsWebsocket?.Disconnect();
}

async void HookClientHelper_onData(SHookData data)
{
    if (exiting || !obsWebsocket.IsConnected) return;

#if DEBUG && true
    ; ; //await Logger.Trace(JsonConvert.SerializeObject(data, Formatting.Indented));
#endif

    foreach (KeyValuePair<SOBSMacro, List<Action<OBSWebsocket>>> keyValuePair in macros)
    {
        //Check if the macro conditions are satisfied.
        SOBSMacro macro = keyValuePair.Key;
        bool cursorPositionValid;
        if (macro.mouseBoundsLeft != null
            && macro.mouseBoundsTop != null
            && macro.mouseBoundsRight != null
            && macro.mouseBoundsBottom != null)
            cursorPositionValid =
                data.mousePosition.x >= macro.mouseBoundsLeft
                && data.mousePosition.x <= macro.mouseBoundsRight
                && data.mousePosition.y >= macro.mouseBoundsTop
                && data.mousePosition.y <= macro.mouseBoundsBottom;
        else cursorPositionValid = true;
        bool keyboardKeysValid = macro.keyboardButtons.All(k => data.pressedKeyboardKeys.Contains(k));
        bool mouseButtonsValid = macro.mouseButtons.All(b => data.pressedMouseButtons.Contains(b));

        //Run the actions on the macro.
        if (cursorPositionValid && mouseButtonsValid && keyboardKeysValid)
        {
            await Logger.Info($"Running macro '{macro}'.");
            foreach (Action<OBSWebsocket> action in keyValuePair.Value)
            {
                try { action(obsWebsocket); }
                catch (Exception ex) { await Logger.Error($"Failed to execute action: {ex.InnerException!.Message}"); }
            }
        }
    }
}

async void BuildSampleData()
{
    //Get the avaliable methods, useful for seeing what methods the user can call.
    Dictionary<string, Dictionary<string, string>> simplifiedActions = new();
    foreach (KeyValuePair<string, ParameterInfo[]> actionKVP in OBSAction.actions)
    {
        Dictionary<string, string> simpleArgs = new();
        foreach (ParameterInfo? actionArgs in actionKVP.Value)
            simpleArgs.Add(actionArgs.Name!, actionArgs.ParameterType.Name);
        simplifiedActions.Add(actionKVP.Key, simpleArgs);
    }
    string avaliableMethodsString = JsonConvert.SerializeObject(simplifiedActions, Formatting.Indented);
    await Logger.Trace($"Avaliable methods:\n{avaliableMethodsString}");
    try { File.WriteAllText(Environment.CurrentDirectory + "\\methods.json", avaliableMethodsString); }
    catch { await Logger.Warning("Failed to write avaliable methods to file."); }

    //Build a sample config.json file.
    SConfiguration sampleConfiguration = new SConfiguration()
    {
        //I only need to set values for the following as the rest can use their default values.
        macros = new()
        {
            {
                new()
                {
                    keyboardButtons = new() { EKeyboardKeys.LMenu, EKeyboardKeys.F9 }, //ALT + F9
                    actions = new()
                    {
                        {
                            new()
                            {
                                method = "SaveReplayBuffer"
                            }
                        }
                    }
                }
            },
            {
                new()
                {
                    mouseBoundsLeft = 0,
                    mouseBoundsTop = 0,
                    mouseBoundsRight = 1920,
                    mouseBoundsBottom = 1080,
                    actions = new()
                    {
                        {
                            new()
                            {
                                method = "SetCurrentSceneCollection",
                                parameters = new()
                                {
                                    { "scName", "Middle Display" }
                                }
                            }
                        }
                    }
                }
            }
        }
    };
    try { File.WriteAllText(Environment.CurrentDirectory + "\\sampleConfiguration.json",
        JsonConvert.SerializeObject(sampleConfiguration, ConfigurationHelper.JSON_SERIALIZER_SETTINGS)); }
    catch { await Logger.Warning("Failed to write sample configuration to file."); }
}

Task<bool> ConnectOBSWebsocket()
{
    return Task.Run(async () =>
    {
        try { obsWebsocket.Connect($"ws://{configuration.ipAddress}:{configuration.port}", configuration.password); }
        catch {}

        if (obsWebsocket.IsConnected) return true;
        
        await Logger.Warning("Failed to connect to OBS websocket.");
        return false;
    });
}

async void ObsWebsocket_Connected(object? sender, EventArgs e)
{
    await Logger.Info("Connected to OBS websocket server.");
    obsWebsocket.WSConnection.OnError += async (_, e) => await Logger.Error(e.Message);
}

async void ObsWebsocket_Disconnected(object? sender, EventArgs e)
{
    if (!exiting)
    {
        await Logger.Info("Reconnecting to OBS websocket...");
        await Task.Delay(1000);
        await ConnectOBSWebsocket();
    }
    else await Logger.Info("Disconnected from OBS websocket server.");
}
#endregion
