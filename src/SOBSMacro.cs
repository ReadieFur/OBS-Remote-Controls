using System.Collections.Generic;
using GlobalInputHook.Objects;

namespace OBSRemoteControls
{
    public struct SOBSMacro
    {
        public int? mouseBoundsLeft;
        public int? mouseBoundsTop;
        public int? mouseBoundsRight;
        public int? mouseBoundsBottom;
        public List<EMouseButtons> mouseButtons;
        public List<EKeyboardKeys> keyboardButtons;
        public List<SMethodData> actions;

        public SOBSMacro()
        {
            mouseBoundsTop = null;
            mouseBoundsLeft = null;
            mouseBoundsBottom = null;
            mouseBoundsRight = null;
            mouseButtons = new();
            keyboardButtons = new();
            actions = new();
        }

        public override string ToString()
        {
            string _string = "";
            if (mouseBoundsTop != null && mouseBoundsLeft != null && mouseBoundsBottom != null && mouseBoundsRight != null)
                _string += $"{mouseBoundsTop}, {mouseBoundsLeft}, {mouseBoundsBottom}, {mouseBoundsRight} ";
            if (mouseButtons.Count > 0) _string += $"mouseButtons: {string.Join(", ", mouseButtons)} ";
            if (keyboardButtons.Count > 0) _string += $"keyboardButtons: {string.Join(", ", keyboardButtons)} ";
            if (actions.Count > 0) _string += "actions: " + Newtonsoft.Json.JsonConvert.SerializeObject(actions);
            return _string;
        }
    }
}
