using OBSWebsocketDotNet;
using System;

namespace OBS_Remote_Controls
{
    public class CustomOBSWebsocket : OBSWebsocket
    {
        public void SendOBSAction(OBSActions action)
        {
            try
            {
                switch (action)
                {
                    case OBSActions.StartRecording:
                        Logger.Info("Start recording.");
                        StartRecording();
                        break;
                    case OBSActions.StopRecording:
                        Logger.Info("Stop recording.");
                        StopRecording();
                        break;
                    case OBSActions.StartStreaming:
                        Logger.Info("Start streaming.");
                        StartStreaming();
                        break;
                    case OBSActions.StopStreaming:
                        Logger.Info("Stop streaming.");
                        StopStreaming();
                        break;
                    case OBSActions.StartReplayBuffer:
                        Logger.Info("Start replay buffer.");
                        StartReplayBuffer();
                        break;
                    case OBSActions.StopReplayBuffer:
                        Logger.Info("Stop replay buffer.");
                        StopReplayBuffer();
                        break;
                    case OBSActions.SaveReplayBuffer:
                        Logger.Info("Save replay buffer.");
                        SaveReplayBuffer();
                        break;
                    case OBSActions.ToggleRecording:
                        Logger.Info("Toggle recording.");
                        ToggleRecording();
                        break;
                    case OBSActions.ToggleStreaming:
                        Logger.Info("Toggle streaming.");
                        ToggleStreaming();
                        break;
                    default: //Actions.None
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public enum OBSActions
        {
            None = 0,
            StartRecording = 1,
            StopRecording = 2,
            StartStreaming = 3,
            StopStreaming = 4,
            StartReplayBuffer = 5,
            StopReplayBuffer = 6,
            SaveReplayBuffer = 7,
            ToggleRecording = 8,
            ToggleStreaming = 9
        }
    }
}
