using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using DamageMeter;
using Data;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ModifierKeys = Data.HotkeysData.ModifierKeys;

namespace TCC.UI.Controls.Settings
{
    public partial class HotkeySetting : INotifyPropertyChanged
    {
        public string ValueString => Value.ToString();

        public HotKey Value
        {
            get => (HotKey)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(HotKey), typeof(HotkeySetting));
        public string SettingName
        {
            get => (string)GetValue(SettingNameProperty);
            set => SetValue(SettingNameProperty, value);
        }
        public static readonly DependencyProperty SettingNameProperty = DependencyProperty.Register("SettingName", typeof(string), typeof(HotkeySetting));
        public Geometry SvgIcon
        {
            get => (Geometry)GetValue(SvgIconProperty);
            set => SetValue(SvgIconProperty, value);
        }
        public static readonly DependencyProperty SvgIconProperty = DependencyProperty.Register("SvgIcon", typeof(Geometry), typeof(HotkeySetting));

        private readonly List<Key> _pressedKeys;
        public HotkeySetting()
        {
            InitializeComponent();
            _pressedKeys = new List<Key>();
            Loaded += OnLoaded;

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            N(nameof(ValueString));
        }


        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {

            KeyboardHook.Instance.SetHotkeys(false);

            e.Handled = true;
            var k = e.Key;
            if (k == Key.System) k = Key.LeftAlt;
            if (_pressedKeys.Contains(k)) return;
            if (k == Key.Tab && _pressedKeys.Contains(Key.LeftAlt)) return;
            if (e.Key == Key.Enter)
            {
                _pressedKeys.Clear();
                Keyboard.ClearFocus();

                return;
            }

            _pressedKeys.Add(k);
            UpdateValue();
        }

        private void UpdateValue()
        {
            var shift = _pressedKeys.Contains(Key.LeftShift);
            var alt = _pressedKeys.Contains(Key.LeftAlt);
            var ctrl = _pressedKeys.Contains(Key.LeftCtrl);
            var key = _pressedKeys.FirstOrDefault(x => x != Key.LeftAlt && x != Key.LeftShift && x != Key.LeftCtrl);
            var mod = (shift ? ModifierKeys.Shift : ModifierKeys.None) | (alt ? ModifierKeys.Alt : ModifierKeys.None) | (ctrl? ModifierKeys.Control: ModifierKeys.None);

            Enum.TryParse(key.ToString(), out Keys wfKey); // Microsoft pls
            if (wfKey == Keys.None) return;
            Value = new HotKey(wfKey, mod);
            N(nameof(ValueString));

        }

        private void UIElement_OnKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            var k = e.Key;
            if (k == Key.System) k = Key.LeftAlt;
            if (e.Key == Key.Enter)
            {
                _pressedKeys.Clear();
                Keyboard.ClearFocus();
                return;

            }
            _pressedKeys.Remove(k);
            UpdateValue();
        }

        public event PropertyChangedEventHandler PropertyChanged = null!;

        protected void N([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UIElement_OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            KeyboardHook.Instance.SetHotkeys(false);
        }

        private void UIElement_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            KeyboardHook.Instance.SetHotkeys(true);
        }

        private void ClearKey(object sender, RoutedEventArgs e)
        {
            Keyboard.ClearFocus();
            Value = new HotKey(Keys.None, ModifierKeys.None);
            N(nameof(ValueString));

        }
    }
}
