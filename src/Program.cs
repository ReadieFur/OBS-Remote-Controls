using System.Reflection;
using System.Windows.Forms;
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
SPoint cursorPosition = new();
List<EMouseButton> pressedMouseButtons = new();
List<Keys> pressedKeyboardKeys = new();
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

hookClientHelper = HookClientHelper.GetOrCreateInstance("obs_remote_controls_custom", configuration.hookUpdateRateMS);
hookClientHelper.mouseEvent += HookClientHelper_mouseEvent;
hookClientHelper.keyboardEvent += HookClientHelper_keyboardEvent;

AppDomain.CurrentDomain.ProcessExit += (_, _) => Unload();
#endregion

#region Load configuration options
await ConnectOBSWebsocket();
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

void HookClientHelper_mouseEvent(SMouseEventData mouseEventData)
{
    switch (mouseEventData.eventType)
    {
        case EMouseEvent.MOUSE_MOVE:
            cursorPosition = mouseEventData.cursorPosition;
            break;
        case EMouseEvent.LBUTTON_DOWN:
            if (!pressedMouseButtons.Contains(EMouseButton.Left)) pressedMouseButtons.Add(EMouseButton.Left);
            break;
        case EMouseEvent.LBUTTON_UP:
            if (pressedMouseButtons.Contains(EMouseButton.Left)) pressedMouseButtons.Remove(EMouseButton.Left);
            break;
        case EMouseEvent.RBUTTON_DOWN:
            if (!pressedMouseButtons.Contains(EMouseButton.Right)) pressedMouseButtons.Add(EMouseButton.Right);
            break;
        case EMouseEvent.RBUTTON_UP:
            if (pressedMouseButtons.Contains(EMouseButton.Right)) pressedMouseButtons.Remove(EMouseButton.Right);
            break;
        case EMouseEvent.MOUSEWHEEL:
            //Skip this for now.
            break;
    }
    HookClientHelper_Update();
}

void HookClientHelper_keyboardEvent(SKeyboardEventData keyboardEventData)
{
    switch (keyboardEventData.eventType)
    {
        case EKeyEvent.SYSKEY_UP:
        case EKeyEvent.KEY_UP:
            if (pressedKeyboardKeys.Contains((Keys)keyboardEventData.keyCode)) pressedKeyboardKeys.Remove((Keys)keyboardEventData.keyCode);
            break;
        case EKeyEvent.SYSKEY_DOWN:
        case EKeyEvent.KEY_DOWN:
            if (!pressedKeyboardKeys.Contains((Keys)keyboardEventData.keyCode)) pressedKeyboardKeys.Add((Keys)keyboardEventData.keyCode);
            break;
    }
    HookClientHelper_Update();
}

async void HookClientHelper_Update()
{
    if (exiting) return;
    else if (!obsWebsocket.IsConnected) return;

#if DEBUG && false
    await Logger.Trace(JsonConvert.SerializeObject(cursorPosition, Formatting.Indented)
        + "\n" + JsonConvert.SerializeObject(pressedMouseButtons, Formatting.Indented)
        + "\n" + JsonConvert.SerializeObject(pressedKeyboardKeys, Formatting.Indented));
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
        {
            cursorPositionValid =
                cursorPosition.x >= macro.mouseBoundsLeft
                && cursorPosition.x <= macro.mouseBoundsRight
                && cursorPosition.y >= macro.mouseBoundsTop
                && cursorPosition.y <= macro.mouseBoundsBottom;
        }
        else cursorPositionValid = true;
        bool mouseButtonsValid = macro.mouseButtons.All(b => pressedMouseButtons.Contains(b));
        bool keyboardKeysValid = macro.keyboardButtons.All(k => pressedKeyboardKeys.Contains(k));

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
                    keyboardButtons = new() { Keys.LMenu, Keys.F9 }, //ALT + F9
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
        catch
        {
            await Logger.Warning("Failed to connect to OBS websocket.");
            return false;
        }
        return true;
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
