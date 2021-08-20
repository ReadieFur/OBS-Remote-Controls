using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OBSWebsocketDotNet;
using Forms = System.Windows.Forms;
using HotkeyController = OBS_Remote_Controls.Hotkeys;
using System.Text.RegularExpressions;

namespace OBS_Remote_Controls.WPF.Pages
{
    public partial class Hotkeys : Page
    {
        private static readonly Random random = new Random();
        private static readonly Regex capitalRegex = new Regex("[A-Z]");
        private static readonly IEnumerable<Forms.Keys> keys = Enum.GetValues(typeof(Forms.Keys)).Cast<Forms.Keys>();
        private static readonly IEnumerable<HotkeyController.KeyModifiers> keyModifiers = Enum.GetValues(typeof(HotkeyController.KeyModifiers)).Cast<HotkeyController.KeyModifiers>();
        private static readonly IEnumerable<Actions> actions = Enum.GetValues(typeof(Actions)).Cast<Actions>();
        private static readonly int comboBoxWidth = 195;
        private static readonly Thickness comboBoxMargin = new Thickness(0, 0, 12, 0);

        private readonly OBSWebsocket obsWebsocket;

        private Dictionary<int, ItemSet> itemSets = new Dictionary<int, ItemSet>();

        public Hotkeys(ref OBSWebsocket _obsWebsocket)
        {
            InitializeComponent();

            obsWebsocket = _obsWebsocket;

            Application.Current.Exit += Current_Exit;

            HotkeyController.HotkeyManager.HotKeyPressed += HotkeyManager_HotKeyPressed;

            if (Program.savedData.data.hotkeys.Count > 0)
            {
                List<AppData.Objects.Hotkey> duplicateEntries = new List<AppData.Objects.Hotkey>();

                foreach (AppData.Objects.Hotkey hotkey in Program.savedData.data.hotkeys)
                {
                    //Is saved value a duplicate?
                    if (!itemSets.FirstOrDefault(_kv =>
                        _kv.Value.key == hotkey.key &&
                        _kv.Value.combination == hotkey.combination &&
                        _kv.Value.action == hotkey.action
                    ).Equals(default(KeyValuePair<int, ItemSet>)))
                    {
                        duplicateEntries.Add(hotkey);
                    }
                    else
                    {
                        KeyValuePair<int, ItemSet> kv = CreateRow(hotkey.key, hotkey.combination, hotkey.action);
                        kv.Value.key = hotkey.key;
                        kv.Value.combination = hotkey.combination;
                        kv.Value.action = hotkey.action;
                        kv.Value.id = HotkeyController.HotkeyManager.RegisterHotKey(hotkey.key, hotkey.combination);
                    }
                }

                foreach (AppData.Objects.Hotkey hotkey in duplicateEntries)
                {
                    Program.savedData.data.hotkeys.Remove(hotkey);
                }
            }
            else
            {
                CreateRow();
            }
        }

        private void HotkeyManager_HotKeyPressed(object sender, HotkeyController.HotkeyArgs e)
        {
            List<AppData.Objects.Hotkey> hotkeys = Program.savedData.data.hotkeys.FindAll(hotkey => hotkey.key == e.Key && hotkey.combination == e.Modifiers);
            
            foreach (AppData.Objects.Hotkey hotkey in hotkeys)
            {
                switch (hotkey.action)
                {
                    case Actions.StartRecording:
                        obsWebsocket.StartRecording();
                        break;
                    case Actions.StopRecording:
                        obsWebsocket.StopRecording();
                        break;
                    case Actions.StartStreaming:
                        obsWebsocket.StartStreaming();
                        break;
                    case Actions.StopStreaming:
                        obsWebsocket.StopStreaming();
                        break;
                    case Actions.StarReplayBuffer:
                        obsWebsocket.StartReplayBuffer();
                        break;
                    case Actions.StopReplayBuffer:
                        obsWebsocket.StopReplayBuffer();
                        break;
                    case Actions.SaveReplayBuffer:
                        obsWebsocket.SaveReplayBuffer();
                        break;
                    default: //Actions.None
                        break;
                }
            }
        }

        private KeyValuePair<int, ItemSet> CreateRow() { return _CreateRow(null, null, null); }
        private KeyValuePair<int, ItemSet> CreateRow(Forms.Keys _key, HotkeyController.KeyModifiers _combination, Actions _action) { return _CreateRow(_key, _combination, _action); }
        private KeyValuePair<int, ItemSet> _CreateRow(Forms.Keys? _key = null, HotkeyController.KeyModifiers? _combination = null, Actions? _action = null)
        {
            //Generate a random ID.
            int id;
            do
            {
                id = random.Next();
            }
            while (itemSets.ContainsKey(id));

            //ListBoxItem listBoxItem = new ListBoxItem();
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Tag = id;

            ComboBox keyComboBox = new ComboBox();
            keyComboBox.Width = comboBoxWidth;
            keyComboBox.Margin = comboBoxMargin;
            keyComboBox.SelectionChanged += comboBox_SelectionChanged;
            foreach (Forms.Keys key in keys)
            {
                if (key == Forms.Keys.None) { continue; }

                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Tag = key;
                comboBoxItem.Content = key.ToString();
                if (_key == key && _combination != null && _action != null) { comboBoxItem.IsSelected = true; }
                keyComboBox.Items.Add(comboBoxItem);
            }

            ComboBox combinationComboBox = new ComboBox();
            combinationComboBox.Width = comboBoxWidth;
            combinationComboBox.Margin = comboBoxMargin;
            combinationComboBox.SelectionChanged += comboBox_SelectionChanged;
            foreach (HotkeyController.KeyModifiers keyModifier in keyModifiers)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Tag = keyModifier;
                comboBoxItem.Content = keyModifier == HotkeyController.KeyModifiers.NoRepeat ? "None" : keyModifier.ToString();
                if (_key != null && _combination == keyModifier && _action != null) { comboBoxItem.IsSelected = true; }
                combinationComboBox.Items.Add(comboBoxItem);
            }

            ComboBox actionComboBox = new ComboBox();
            actionComboBox.Width = comboBoxWidth - 5;
            actionComboBox.Margin = comboBoxMargin;
            actionComboBox.SelectionChanged += comboBox_SelectionChanged;
            foreach (Actions action in actions)
            {
                if (action == Actions.None) { continue; }

                string key = action.ToString();
                string content = key[0].ToString();
                for (int i = 1; i < key.Length; i++)
                {
                    if (capitalRegex.IsMatch(key[i].ToString()))
                    {
                        content += $" {key[i]}";
                    }
                    else
                    {
                        content += key[i];
                    }
                }

                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Tag = action;
                comboBoxItem.Content = content;
                if (_key != null && _combination != null && _action == action) { comboBoxItem.IsSelected = true; }
                actionComboBox.Items.Add(comboBoxItem);
            }

            stackPanel.Children.Add(keyComboBox);
            stackPanel.Children.Add(combinationComboBox);
            stackPanel.Children.Add(actionComboBox);
            listBox.Items.Add(stackPanel);

            ItemSet itemSet = new ItemSet(stackPanel, keyComboBox, combinationComboBox, actionComboBox);
            itemSets.Add(id, itemSet);
            return new KeyValuePair<int, ItemSet>(id, itemSet);
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((StackPanel)((ComboBox)sender).Parent == null) { return; }
            int tag = (int)((StackPanel)((ComboBox)sender).Parent).Tag;
            itemSets.TryGetValue(tag, out ItemSet itemSet);

            if (itemSet.id != null)
            {
                HotkeyController.HotkeyManager.UnregisterHotKey((int)itemSet.id);
            }

            if (e.AddedItems.Count != 0)
            {
                //Couldnt figure out a way on how to get this to work here 'Enum.GetUnderlyingType(Forms.Keys)';
                switch (((ComboBoxItem)e.AddedItems[0]).Tag.GetType().Name)
                {
                    case "Keys":
                        itemSet.key = (Forms.Keys)((ComboBoxItem)e.AddedItems[0]).Tag;
                        break;
                    case "KeyModifiers":
                        itemSet.combination = (HotkeyController.KeyModifiers)((ComboBoxItem)e.AddedItems[0]).Tag;
                        break;
                    case "Actions":
                        itemSet.action = (Actions)((ComboBoxItem)e.AddedItems[0]).Tag;
                        break;
                    default:
                        return;
                }
            }
            else
            {
                return;
            }

            if (itemSet.key != null && itemSet.combination != null && itemSet.action != null)
            {
                //Check if the hotkey exists.
                if (!itemSets.FirstOrDefault(kv =>
                    kv.Value.key == itemSet.key &&
                    kv.Value.combination == itemSet.combination &&
                    kv.Value.action == itemSet.action &&
                    kv.Key != tag
                ).Equals(default(KeyValuePair<int, ItemSet>)))
                {
                    Logger.Trace("Duplicate Hotkey found.");
                    RemoveRow(tag);
                }
                else
                {
                    itemSet.id = HotkeyController.HotkeyManager.RegisterHotKey((Forms.Keys)itemSet.key, (HotkeyController.KeyModifiers)itemSet.combination);
                    Program.savedData.data.hotkeys.Add(new AppData.Objects.Hotkey
                    {
                        key = (Forms.Keys)itemSet.key,
                        combination = (HotkeyController.KeyModifiers)itemSet.combination,
                        action = (Actions)itemSet.action
                    });
                }
            }
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            foreach (KeyValuePair<int, ItemSet> kv in itemSets)
            {
                if (kv.Value.id != null)
                {
                    HotkeyController.HotkeyManager.UnregisterHotKey((int)kv.Value.id);
                }
            }
        }

        private void subtract_Click(object sender, RoutedEventArgs e)
        {
            StackPanel stackPanel = (StackPanel)listBox.SelectedItem;
            if (stackPanel != null)
            {
                RemoveRow((int)stackPanel.Tag);
            }
        }

        private void RemoveRow(int id)
        {
            if (itemSets.TryGetValue(id, out ItemSet itemSet))
            {
                listBox.Items.Remove(itemSet.stackPanel);
                if (itemSet.id != null)
                {
                    HotkeyController.HotkeyManager.UnregisterHotKey((int)itemSet.id);

                    Program.savedData.data.hotkeys.RemoveAll(hotkey =>
                        hotkey.key == itemSet.key &&
                        hotkey.combination == itemSet.combination &&
                        hotkey.action == itemSet.action
                    );
                }
                itemSets.Remove(id);
            }
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            CreateRow();
        }

        private class ItemSet
        {
            public readonly StackPanel stackPanel;
            public readonly ComboBox keyComboBox;
            public readonly ComboBox combinationComboBox;
            public readonly ComboBox actionComboBox;

            public int? id = null;
            public Forms.Keys? key = null;
            public HotkeyController.KeyModifiers? combination = null;
            public Actions? action = null;

            public ItemSet(StackPanel _stackPanel, ComboBox _keyComboBox, ComboBox _combinationComboBox, ComboBox _actionComboBox)
            {
                stackPanel = _stackPanel;
                keyComboBox = _keyComboBox;
                combinationComboBox = _combinationComboBox;
                actionComboBox = _actionComboBox;
            }
        }

        public enum Actions
        {
            None = 0,
            StartRecording = 1,
            StopRecording = 2,
            StartStreaming = 3,
            StopStreaming = 4,
            StarReplayBuffer = 5,
            StopReplayBuffer = 6,
            SaveReplayBuffer = 7
        }
    }
}
