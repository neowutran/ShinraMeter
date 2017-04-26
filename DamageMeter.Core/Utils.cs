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
                disp.Invoke(priority, (Delegate)dotIt);
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
            this._dispatcher = Application.Current.Dispatcher;
            this._lock = new ReaderWriterLockSlim();
        }

        protected override void ClearItems()
        {
            this._dispatcher.InvokeIfRequired((Action) (() =>
            {
                this._lock.EnterWriteLock();
                try
                {
                    base.ClearItems();
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }), DispatcherPriority.DataBind);
        }

        protected override void InsertItem(int index, T item)
        {
            this._dispatcher.InvokeIfRequired((Action) (() =>
            {
                if (index > this.Count)
                    return;
                this._lock.EnterWriteLock();
                try
                {
                    base.InsertItem(index, item);
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }), DispatcherPriority.DataBind);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            this._dispatcher.InvokeIfRequired((Action) (() =>
            {
                this._lock.EnterReadLock();
                int count = this.Count;
                this._lock.ExitReadLock();
                if (oldIndex >= count | newIndex >= count | oldIndex == newIndex)
                    return;
                this._lock.EnterWriteLock();
                try
                {
                    base.MoveItem(oldIndex, newIndex);
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }), DispatcherPriority.DataBind);
        }

        protected override void RemoveItem(int index)
        {
            this._dispatcher.InvokeIfRequired((Action) (() =>
            {
                if (index >= this.Count)
                    return;
                this._lock.EnterWriteLock();
                try
                {
                    base.RemoveItem(index);
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }), DispatcherPriority.DataBind);
        }

        protected override void SetItem(int index, T item)
        {
            this._dispatcher.InvokeIfRequired((Action) (() =>
            {
                this._lock.EnterWriteLock();
                try
                {
                    base.SetItem(index, item);
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }), DispatcherPriority.DataBind);
        }

        public T[] ToSyncArray()
        {
            this._lock.EnterReadLock();
            try
            {
                T[] array = new T[this.Count];
                this.CopyTo(array, 0);
                return array;
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }
    }
}
