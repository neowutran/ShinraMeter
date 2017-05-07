using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace DamageMeter
{
    public static class DispatcherExtensions
    {
        public static void InvokeIfRequired(this Dispatcher disp, Action dotIt, DispatcherPriority priority)
        {
            if (disp.Thread != Thread.CurrentThread)
                disp.Invoke(priority, dotIt);
            else
                dotIt();
        }
    }

    public class TSPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string v)
        {
            Application.Current.Dispatcher.InvokeIfRequired(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v)), DispatcherPriority.DataBind);
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
                try
                {
                    base.ClearItems();
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }, DispatcherPriority.DataBind);
        }

        protected override void InsertItem(int index, T item)
        {
            _dispatcher.InvokeIfRequired(() =>
            {
                if (index > Count)
                    return;
                _lock.EnterWriteLock();
                try
                {
                    base.InsertItem(index, item);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }, DispatcherPriority.DataBind);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            _dispatcher.InvokeIfRequired(() =>
            {
                _lock.EnterReadLock();
                var count = Count;
                _lock.ExitReadLock();
                if ((oldIndex >= count) | (newIndex >= count) | (oldIndex == newIndex))
                    return;
                _lock.EnterWriteLock();
                try
                {
                    base.MoveItem(oldIndex, newIndex);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }, DispatcherPriority.DataBind);
        }

        protected override void RemoveItem(int index)
        {
            _dispatcher.InvokeIfRequired(() =>
            {
                if (index >= Count)
                    return;
                _lock.EnterWriteLock();
                try
                {
                    base.RemoveItem(index);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }, DispatcherPriority.DataBind);
        }

        protected override void SetItem(int index, T item)
        {
            _dispatcher.InvokeIfRequired(() =>
            {
                _lock.EnterWriteLock();
                try
                {
                    base.SetItem(index, item);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
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
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}