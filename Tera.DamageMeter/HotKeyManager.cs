// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Tera.DamageMeter
{
    [Flags]
    internal enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }

    internal class HotKeyReceiverWindow : Form
    {
        public event Action<int> HotKeyPressed;

        protected virtual void OnHotKeyPressed(int obj)
        {
            Action<int> handler = HotKeyPressed;
            if (handler != null) handler(obj);
        }

        private const int WM_HOTKEY = 0x0312;

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                var id = m.WParam;
                OnHotKeyPressed((int)id);
            }
            base.WndProc(ref m);
        }
    }

    public class HotKeyManager : IDisposable
    {
        internal HashSet<GlobalHotKey> _hotkeys = new HashSet<GlobalHotKey>();
        private readonly Dictionary<int, GlobalHotKeyHandle> _hotkeyHandlesById = new Dictionary<int, GlobalHotKeyHandle>();
        private readonly Dictionary<int, GlobalHotKey> _hotkeysById = new Dictionary<int, GlobalHotKey>();
        private int _currentId;
        private HotKeyReceiverWindow _form;
        private const int IdCount = 0xC000;
        private IntPtr HWnd { get { return _form.Handle; } }

        public IEnumerable<GlobalHotKey> Hotkeys { get { return _hotkeys; } }

        public HotKeyManager()
        {
            _form = new HotKeyReceiverWindow();
            _form.HotKeyPressed += HandleHotKeyPressed;
        }

        void HandleHotKeyPressed(int id)
        {
            _hotkeysById[id].OnPressed();
        }

        private int CreateId()
        {
            if (_hotkeyHandlesById.Count >= IdCount)
                throw new InvalidOperationException("Too many hotkeys");
            do
            {
                _currentId = (_currentId + 1) % IdCount;
            } while (_hotkeyHandlesById.ContainsKey(_currentId));

            return _currentId;
        }

        internal GlobalHotKeyHandle CreateHotKeyHandle(GlobalHotKey hotkey, KeyModifiers fsModifiers, uint vk)
        {
            var id = CreateId();
            var result = new GlobalHotKeyHandle(HWnd, id, fsModifiers, vk);
            _hotkeyHandlesById.Add(id, result);
            _hotkeysById[id] = hotkey;
            return result;
        }

        internal void RemoveHotKeyHandle(GlobalHotKey hotKey, GlobalHotKeyHandle handle)
        {
            _hotkeyHandlesById.Remove(handle.Id);
            _hotkeysById.Remove(handle.Id);
        }

        public void Dispose()
        {
            if (_form == null)
                return;

            foreach (var hotKeyHandle in _hotkeyHandlesById.Values)
            {
                hotKeyHandle.Dispose();
            }
            _hotkeyHandlesById.Clear();

            _form.Dispose();
            _form = null;
        }
    }

    internal class GlobalHotKeyHandle : IDisposable
    {
        private readonly IntPtr _hWnd;
        private int _id;

        public int Id { get { return _id; } }

        public GlobalHotKeyHandle(IntPtr hWnd, int id, KeyModifiers fsModifiers, uint vk)
        {
            _id = -1;

            if (!RegisterHotKey(hWnd, id, fsModifiers, vk))
                throw new Win32Exception();

            _hWnd = hWnd;
            _id = id;
        }

        [DllImport("user32", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, uint vk);

        [DllImport("user32", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public void Dispose()
        {
            if (_id < 0)
                return;

            if (!UnregisterHotKey(_hWnd, _id))
                throw new Win32Exception();
            _id = -1;
        }
    }

    public class GlobalHotKey : IDisposable
    {
        private readonly HotKeyManager _manager;
        private GlobalHotKeyHandle _handle;
        private Keys _key;
        private bool _enabled;

        public GlobalHotKey(HotKeyManager manager)
        {
            _manager = manager;
            _manager._hotkeys.Add(this);
        }

        public Keys Key
        {
            get { return _key; }
            set
            {
                bool originalState = Enabled;
                Enabled = false;
                _key = value;
                Enabled = originalState;
            }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value == Enabled)
                    return;

                if (_manager == null)
                    throw new ObjectDisposedException("GlobalhotKey");

                if (value)
                {
                    if (Key != Keys.None)
                    {
                        var vk = (uint)Key & 0xFFFF;

                        var modifiers = KeyModifiers.None;
                        if ((Key & Keys.Shift) != 0)
                            modifiers |= KeyModifiers.Shift;
                        if ((Key & Keys.Alt) != 0)
                            modifiers |= KeyModifiers.Alt;
                        if ((Key & Keys.Control) != 0)
                            modifiers |= KeyModifiers.Control;

                        _handle = _manager.CreateHotKeyHandle(this, modifiers, vk);
                    }
                }
                else
                {
                    if (_handle != null)
                    {
                        _manager.RemoveHotKeyHandle(this, _handle);
                        _handle.Dispose();
                        _handle = null;
                    }
                }

                _enabled = value;
            }
        }

        public void Dispose()
        {
            Enabled = false;
            _manager._hotkeys.Remove(this);
        }

        public event EventHandler Pressed;

        internal protected virtual void OnPressed()
        {
            EventHandler handler = Pressed;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }

    public static class HotKeyHelpers
    {
        public static string ToString(Keys key)
        {
            return (String)new KeysConverter().ConvertTo(key, typeof(string));
        }

        public static Keys FromString(string s)
        {
            if (s == null)
                return Keys.None;
            return ((Keys?)new KeysConverter().ConvertFrom(s)) ?? Keys.None;
        }
    }
}
