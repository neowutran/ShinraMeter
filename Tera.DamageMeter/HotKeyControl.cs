// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tera.DamageMeter
{
    public class HotKeyControl : TextBox
    {
        const int WM_KEYUP = 0x101;
        const int WM_KEYDOWN = 0x100;
        const int WM_SYSKEYDOWN = 0x104;
        const int WM_SYSKEYUP = 0x105;

        private Keys _key;

        public Keys Key
        {
            get { return _key; }
            set
            {
                if (_key == value)
                    return;

                _key = value;
                Text = HotKeyHelpers.ToString(Key);
            }
        }

        public Keys CancelKey { get; set; }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private static Keys CleanKeys(Keys keys)
        {
            var baseKey = keys & Keys.KeyCode;
            if (baseKey == Keys.ControlKey || baseKey == Keys.LControlKey || baseKey == Keys.RControlKey)
                keys &= ~Keys.Control;
            if (baseKey == Keys.ShiftKey || baseKey == Keys.LShiftKey || baseKey == Keys.RShiftKey)
                keys &= ~Keys.Shift;
            if (baseKey == Keys.Menu)
                keys &= ~Keys.Alt;
            return keys;
        }

        private void KeyPressed(Keys key)
        {
            key = CleanKeys(key);
            if (key == CancelKey)
                key = Keys.None;
            Key = key;
        }

        protected override bool ProcessKeyMessage(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == WM_KEYDOWN || m.Msg == WM_SYSKEYDOWN)
            {
                KeyPressed((Keys)m.WParam | ModifierKeys);
                return true;
            }
            return base.ProcessKeyMessage(ref m);
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message m, Keys keyData)
        {
            if (m.Msg == WM_KEYDOWN || m.Msg == WM_SYSKEYDOWN)
            {
                KeyPressed((Keys)m.WParam | ModifierKeys);
                return true;
            }
            return base.ProcessCmdKey(ref m, keyData);
        }

        public HotKeyControl()
        {
            ShortcutsEnabled = false;
        }
    }

}
