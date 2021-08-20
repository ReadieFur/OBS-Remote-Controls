using System;
using System.Windows.Forms;

namespace OBS_Remote_Controls.Hotkeys
{
    internal class HotkeyArgs
    {
        public readonly Keys Key;
        public readonly KeyModifiers Modifiers;

        public HotkeyArgs(Keys key, KeyModifiers modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }

        public HotkeyArgs(IntPtr hotKeyParam)
        {
            uint param = (uint)hotKeyParam.ToInt64();
            Key = (Keys)((param & 0xffff0000) >> 16);
            Modifiers = (KeyModifiers)(param & 0x0000ffff);
        }
    }

    [Flags]
    public enum KeyModifiers
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8,
        NoRepeat = 0x4000
    }
}
