using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Data;
using Lang;

namespace DamageMeter
{
    public sealed class KeyboardHook : IDisposable
    {
        private static KeyboardHook _instance;

        public delegate void TopmostSwitch();
        public event TopmostSwitch SwitchTopMost;
        private readonly Window _window = new Window();
        private int _currentId;

        private bool _isRegistered;

        private KeyboardHook()
        {
            // register the event of the inner native window.
            _window.KeyPressed += delegate(object sender, KeyPressedEventArgs args) { KeyPressed?.Invoke(this, args); };
        }


        public static KeyboardHook Instance => _instance ?? (_instance = new KeyboardHook());

        public bool SetHotkeys(bool value)
        {
          
            if (value && !_isRegistered)
            {
                Register();
                return true;
            }
            if (!value && _isRegistered)
            {
                ClearHotkeys();
            }
            return false;
            
        }

        private static void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {


            if (e.Key == BasicTeraData.Instance.HotkeysData.Topmost.Key &&
                e.Modifier == BasicTeraData.Instance.HotkeysData.Topmost.Value)
            {
                Instance.SwitchTopMost?.Invoke();
            }
            else if (e.Key == BasicTeraData.Instance.HotkeysData.Paste.Key &&
                     e.Modifier == BasicTeraData.Instance.HotkeysData.Paste.Value)
            {
                var text = Clipboard.GetText();
                var pasteThread = new Thread(() => CopyPaste.Paste(text));
                pasteThread.Start();
            }
            else if (e.Key == BasicTeraData.Instance.HotkeysData.Reset.Key &&
                     e.Modifier == BasicTeraData.Instance.HotkeysData.Reset.Value)
            {
                //Can't call directly NetworkController.Instance.Reset() => threading problem
                NetworkController.Instance.NeedToReset = true;
            }
            else if (e.Key == BasicTeraData.Instance.HotkeysData.ResetCurrent.Key &&
                     e.Modifier == BasicTeraData.Instance.HotkeysData.ResetCurrent.Value)
            {
                //Can't call directly NetworkController.Instance.ResetCurrent() => threading problem
                NetworkController.Instance.NeedToResetCurrent = true;
            }
            else if (e.Key == BasicTeraData.Instance.HotkeysData.ExcelSave.Key &&
                     e.Modifier == BasicTeraData.Instance.HotkeysData.ExcelSave.Value)
            {
                //Can't call directly Export => threading problem
                NetworkController.Instance.NeedToExport = DataExporter.Dest.Excel | DataExporter.Dest.Manual;
            }
            else if (e.Key == BasicTeraData.Instance.HotkeysData.ClickThrou.Key &&
                     e.Modifier == BasicTeraData.Instance.HotkeysData.ClickThrou.Value)
            {
                NetworkController.Instance.SwitchClickThrou();
            }
            foreach (
                var copy in
                    BasicTeraData.Instance.HotkeysData.Copy.Where(
                        copy => e.Key == copy.Key && e.Modifier == copy.Modifier))
            {
                //Can't copy directly, => threading problem
                NetworkController.Instance.NeedToCopy = copy;
            }
        }


        public void Update()
        {
            ClearHotkeys();
            Register();       
        }

        public void RegisterKeyboardHook()
        {
          
                // register the event that is fired after the key press.
                Instance.KeyPressed += hook_KeyPressed;
          
                if (!_isRegistered)
                {
                    Register();
                }
            
        }

        private void Register()
        {
            //MessageBox.Show("PASSE" + Environment.StackTrace, "ERROR: "+ Environment.StackTrace, MessageBoxButtons.OKCancel);

            RegisterHotKey(BasicTeraData.Instance.HotkeysData.Topmost.Value,
                BasicTeraData.Instance.HotkeysData.Topmost.Key);
            RegisterHotKey(BasicTeraData.Instance.HotkeysData.Paste.Value,
                BasicTeraData.Instance.HotkeysData.Paste.Key);
            RegisterHotKey(BasicTeraData.Instance.HotkeysData.Reset.Value,
                BasicTeraData.Instance.HotkeysData.Reset.Key);
            RegisterHotKey(BasicTeraData.Instance.HotkeysData.ResetCurrent.Value,
                BasicTeraData.Instance.HotkeysData.ResetCurrent.Key);
            RegisterHotKey(BasicTeraData.Instance.HotkeysData.ExcelSave.Value,
                BasicTeraData.Instance.HotkeysData.ExcelSave.Key);
            RegisterHotKey(BasicTeraData.Instance.HotkeysData.ClickThrou.Value,
                BasicTeraData.Instance.HotkeysData.ClickThrou.Key);
            if (BasicTeraData.Instance.WindowData.RemoveTeraAltEnterHotkey)
            {
                RegisterHotKey(HotkeysData.ModifierKeys.Alt, Keys.Enter);
                RegisterHotKey(HotkeysData.ModifierKeys.Alt | HotkeysData.ModifierKeys.Control, Keys.Enter);

            }
            foreach (var copy in BasicTeraData.Instance.HotkeysData.Copy)
            {
                RegisterHotKey(copy.Modifier, copy.Key);
            }
            _isRegistered = true;
        }

        // Registers a hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        ///     Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        public void RegisterHotKey(HotkeysData.ModifierKeys modifier, Keys key)
        {
            if (key == Keys.None) return; //allow disable hotkeys using "None" key
            // increment the counter.
            _currentId++;

            // register the hot key.
            if (!RegisterHotKey(_window.Handle, _currentId, (uint) modifier, (uint) key))
                MessageBox.Show(LP.RegisterHotKeyError + " " + key + LP.AlreadyInUse);
        }

        /// <summary>
        ///     A hot key has been pressed.
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        /// <summary>
        ///     Represents the window that is used internally to get the messages.
        /// </summary>
        private sealed class Window : NativeWindow, IDisposable
        {
            private static readonly int WmHotkey = 0x0312;

            public Window()
            {
                // create the handle for the window.
                CreateHandle(new CreateParams());
            }

            #region IDisposable Members

            public void Dispose()
            {
                DestroyHandle();
            }

            #endregion

            /// <summary>
            ///     Overridden to get the notifications.
            /// </summary>
            /// <param name="m"></param>
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                // check if we got a hot key pressed.
                if (m.Msg == WmHotkey)
                {
                    // get the keys.
                    var key = (Keys) (((int) m.LParam >> 16) & 0xFFFF);
                    var modifier = (HotkeysData.ModifierKeys) ((int) m.LParam & 0xFFFF);

                    // invoke the event to notify the parent.
                    KeyPressed?.Invoke(this, new KeyPressedEventArgs(modifier, key));
                }
            }

            public event EventHandler<KeyPressedEventArgs> KeyPressed;
        }

        #region IDisposable Members

        public void Dispose()
        {
            // unregister all the registered hot keys.
            ClearHotkeys();

            // dispose the inner native window.
            _window.Dispose();
        }

        private void ClearHotkeys()
        {
            for (var i = _currentId; i > 0; i--)
            {
                UnregisterHotKey(_window.Handle, i);
            }
            _currentId = 0;
            _isRegistered = false;
        }

        #endregion
    }

    /// <summary>
    ///     Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        internal KeyPressedEventArgs(HotkeysData.ModifierKeys modifier, Keys key)
        {
            Modifier = modifier;
            Key = key;
        }

        public HotkeysData.ModifierKeys Modifier { get; }

        public Keys Key { get; }
    }
}