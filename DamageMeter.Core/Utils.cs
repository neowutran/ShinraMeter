using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Tera.Game;

namespace DamageMeter
{
    public enum PlayerRole
    {
        Dps,
        Tank,
        Healer,
        Self,
        None
    }

    public static class MiscUtils
    {
        public static PlayerRole RoleFromClass(PlayerClass c)
        {
            // //todo: more accurate tank detection

            return c switch
            {
                PlayerClass.Warrior => PlayerRole.Dps,
                PlayerClass.Slayer => PlayerRole.Dps,
                PlayerClass.Berserker => PlayerRole.Dps,
                PlayerClass.Sorcerer => PlayerRole.Dps,
                PlayerClass.Archer => PlayerRole.Dps,
                PlayerClass.Reaper => PlayerRole.Dps,
                PlayerClass.Gunner => PlayerRole.Dps,
                PlayerClass.Ninja => PlayerRole.Dps,
                PlayerClass.Valkyrie => PlayerRole.Dps,
                PlayerClass.Priest => PlayerRole.Healer,
                PlayerClass.Mystic => PlayerRole.Healer,
                PlayerClass.Brawler => PlayerRole.Tank,
                PlayerClass.Lancer => PlayerRole.Tank,
                _ => PlayerRole.None
            };

        }

        public static bool IsToolboxRunning()
        {
            //kinda ewww, but ok
            var expectedPath = Path.Combine(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))!, "node_modules\\electron\\dist\\electron.exe");

            return Process.GetProcessesByName("Electron").Any(x => x.GetFilePath() == expectedPath);
        }
    }
    public static class Extensions
    {
        public static void InvokeIfRequired(this Dispatcher disp, Action dotIt, DispatcherPriority priority)
        {
            if (disp.Thread != Thread.CurrentThread) { disp.Invoke(priority, dotIt); }
            else { dotIt(); }
        }

        public static string LimitUtf8ByteCount(this string s, int n)
        {
            // quick test (we probably won't be trimming most of the time)
            if (Encoding.UTF8.GetByteCount(s) <= n)
                return s;
            // get the bytes
            var a = Encoding.UTF8.GetBytes(s);
            // if we are in the middle of a character (highest two bits are 10)
            if (n > 0 && (a[n] & 0xC0) == 0x80)
            {
                // remove all bytes whose two highest bits are 10
                // and one more (start of multi-byte sequence - highest bits should be 11)
                while (--n > 0 && (a[n] & 0xC0) == 0x80)
                    ;
            }
            // convert back to string (with the limit adjusted)
            return Encoding.UTF8.GetString(a, 0, n);
        }

        public static string GetFilePath(this Process p)
        {
            var capacity = 2000;
            var builder = new StringBuilder(capacity);
            var ptr = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, p.Id);
            return !QueryFullProcessImageName(ptr, 0, builder, ref capacity) ? string.Empty : builder.ToString();
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryFullProcessImageName(
            [In] IntPtr hProcess,
            [In] int dwFlags,
            [Out] StringBuilder lpExeName,
            ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            ProcessAccessFlags processAccess,
            bool bInheritHandle,
            int processId);

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            CreateProcess = 0x0080,
            CreateThread = 0x0002,
            DupHandle = 0x0040,
            QueryInformation = 0x0400,
            QueryLimitedInformation = 0x1000,
            SetInformation = 0x0200,
            SetQuota = 0x0100,
            SuspendResume = 0x0800,
            Terminate = 0x0001,
            Operation = 0x0008,
            Read = 0x0010,
            Write = 0x0020,
            Synchronize = 0x00100000,
        }

    }

    public class TSPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string v = null)
        {
            Application.Current.Dispatcher.InvokeIfRequired(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v)), DispatcherPriority.DataBind);
        }

        public void NotifyPropertyChangedEx(string v)
        {
            NotifyPropertyChanged(v);
        }
    }

    public class SynchronizedObservableCollection<T> : ObservableCollection<T>
    {
        private readonly Dispatcher _dispatcher;
        private readonly ReaderWriterLockSlim _lock;

        public SynchronizedObservableCollection()
        {
            _dispatcher = Application.Current.Dispatcher;
            _lock = new ReaderWriterLockSlim();
        }

        protected override void ClearItems()
        {
            _dispatcher.InvokeIfRequired(() =>
            {
                _lock.EnterWriteLock();
                try { base.ClearItems(); }
                finally { _lock.ExitWriteLock(); }
            }, DispatcherPriority.DataBind);
        }

        protected override void InsertItem(int index, T item)
        {
            _dispatcher.InvokeIfRequired(() =>
            {
                if (index > Count) { return; }
                _lock.EnterWriteLock();
                try { base.InsertItem(index, item); }
                finally { _lock.ExitWriteLock(); }
            }, DispatcherPriority.DataBind);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            _dispatcher.InvokeIfRequired(() =>
            {
                _lock.EnterReadLock();
                var count = Count;
                _lock.ExitReadLock();
                if ((oldIndex >= count) | (newIndex >= count) | (oldIndex == newIndex)) { return; }
                _lock.EnterWriteLock();
                try { base.MoveItem(oldIndex, newIndex); }
                finally { _lock.ExitWriteLock(); }
            }, DispatcherPriority.DataBind);
        }

        protected override void RemoveItem(int index)
        {
            _dispatcher.InvokeIfRequired(() =>
            {
                if (index >= Count) { return; }
                _lock.EnterWriteLock();
                try { base.RemoveItem(index); }
                finally { _lock.ExitWriteLock(); }
            }, DispatcherPriority.DataBind);
        }

        protected override void SetItem(int index, T item)
        {
            _dispatcher.InvokeIfRequired(() =>
            {
                _lock.EnterWriteLock();
                try { base.SetItem(index, item); }
                finally { _lock.ExitWriteLock(); }
            }, DispatcherPriority.DataBind);
        }

        public void DisposeAll() //No checks, use only if all items are disposable
        {
            _dispatcher.InvokeIfRequired(() =>
            {
                _lock.EnterWriteLock();
                try
                {
                    foreach (IDisposable x1 in base.Items) { x1.Dispose(); }
                    base.ClearItems();
                }
                finally { _lock.ExitWriteLock(); }
            }, DispatcherPriority.DataBind);
        }

        public T[] ToSyncArray()
        {
            _lock.EnterReadLock();
            try
            {
                var array = new T[Count];
                CopyTo(array, 0);
                return array;
            }
            finally { _lock.ExitReadLock(); }
        }
    }

    public static class EnumUtils
    {
        public static List<T> ListFromEnum<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }
    }

}