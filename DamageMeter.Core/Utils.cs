using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace DamageMeter
{
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
}