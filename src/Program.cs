using System.Reflection;
using CSharpTools.ConsoleExtensions;
using GlobalInputHook.Tools;
using GlobalInputHook.Objects;
using OBSRemoteControlsCustom;
using OBSRemoteControlsCustom.Configuration;
using OBSWebsocketDotNet;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;

#region Program
#if DEBUG
Logger.logLevel = ELogLevel.Trace;
BuildSampleData();
#endif

#region Variables
IntPtr? windowHandle = null;
bool isWindowVisible = true;
NotifyIcon trayIcon;
bool exiting = false;
SConfiguration configuration;
OBSWebsocket obsWebsocket;
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
if (Environment.GetCommandLineArgs().Contains("-m")) ToggleWindowVisibility();

trayIcon = new NotifyIcon();
trayIcon.Icon = Resources.Icon;
trayIcon.Visible = true;
trayIcon.Click += (_, _) => ToggleWindowVisibility();

obsWebsocket = new OBSWebsocket();
obsWebsocket.Connected += ObsWebsocket_Connected;
obsWebsocket.Disconnected += ObsWebsocket_Disconnected;
DLLInstanceHelper.OnUpdate += GlobalInputHook_OnUpdate;
DLLInstanceHelper.StartHookOnNewSTAMessageThread(configuration.hookUpdateRateMS > 0 ? configuration.hookUpdateRateMS : -1);

AppDomain.CurrentDomain.ProcessExit += (_, _) => Unload();
#endregion

#region Load configuration options
foreach (SOBSMacro macro in configuration.macros)
{
    if (macros.ContainsKey(macro))
    {
        await Logger.Warning($"Duplicate macro detected '{macro}'.", false);
        continue;
    }

    List<Action<OBSWebsocket>> actions = new();

    foreach (SMethodData action in macro.actions)
    {
        if (OBSAction.BuildAction(action.method, action.parameters, out Action<OBSWebsocket> method)) actions.Add(method);
        else await Logger.Warning($"Failed to load action '{action.method}'.", false);
    }

    macros.Add(macro, actions);
}
#endregion

_ = ConnectOBSWebsocket();

#region UI (CLI) loop
_ = Task.Run(() =>
{
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
        if (userInput == "exit") Unload();
    }
});
#endregion

Application.Run();
Unload();
#endregion

//====

#region Methods
//Shouldn't return.
void Unload(int exitCode = 0)
{
    if (exiting) return;
    exiting = true;
    DLLInstanceHelper.Unhook();
    obsWebsocket?.Disconnect();
    Environment.Exit(exitCode);
}

[DllImport("user32.dll", SetLastError = true)]
static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

[DllImport("user32.dll")]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

void ToggleWindowVisibility()
{
    if (windowHandle == null) windowHandle = FindWindow(null, Console.Title);
    ShowWindow(windowHandle.Value, isWindowVisible ? 0 : 1);
    isWindowVisible = !isWindowVisible;
}

async void GlobalInputHook_OnUpdate(SHookData data)
{
    if (exiting || !obsWebsocket.IsConnected) return;

#if DEBUG && false
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
            await Logger.Info($"Running macro '{macro}'.", false);
            foreach (Action<OBSWebsocket> action in keyValuePair.Value)
            {
                try { action(obsWebsocket); }
                catch (Exception ex) { await Logger.Error($"Failed to execute action: {ex.InnerException!.Message}", false); }
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
    await Logger.Trace($"Avaliable methods:\n{avaliableMethodsString}", false);
    try { File.WriteAllText(Environment.CurrentDirectory + "\\methods.json", avaliableMethodsString); }
    catch { await Logger.Warning("Failed to write avaliable methods to file.", false); }

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
    catch { await Logger.Warning("Failed to write sample configuration to file.", false); }
}

Task<bool> ConnectOBSWebsocket()
{
    return Task.Run(async () =>
    {
        string exceptionString = string.Empty;
        try { obsWebsocket.Connect($"ws://{configuration.ipAddress}:{configuration.port}", configuration.password); }
        catch (Exception ex) { exceptionString = $" {ex.Message}"; }

        if (obsWebsocket.IsConnected) return true;
        
        await Logger.Warning($"Failed to connect to OBS websocket.{exceptionString}", false);
        return false;
    });
}

async void ObsWebsocket_Connected(object? sender, EventArgs e)
{
    await Logger.Info("Connected to OBS websocket server.", false);
    obsWebsocket.WSConnection.OnError += async (_, e) => await Logger.Error(e.Message);
}

async void ObsWebsocket_Disconnected(object? sender, EventArgs e)
{
    if (!exiting)
    {
        await Logger.Info("Reconnecting to OBS websocket...", false);
        await Task.Delay(1000);
        await ConnectOBSWebsocket();
    }
    else await Logger.Info("Disconnected from OBS websocket server.", false);
}
#endregion
