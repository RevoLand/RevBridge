using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace RevBridge.Framework
{
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

        private void ExecuteOnSyncContext(Action action)
        {
            try
            {
                if (SynchronizationContext.Current == _synchronizationContext)
                {
                    action();
                }
                else
                {
                    _synchronizationContext.Send(_ => action(), null);
                }
            }
            catch (Exception)
            {
                //logger.Fatal(ex);
            }
        }

        protected override void InsertItem(int index, T item)
        {
            try
            {
                ExecuteOnSyncContext(() => base.InsertItem(index, item));
            }
            catch (Exception)
            {
                //logger.Fatal(ex);
            }
        }

        protected override void ClearItems() => ExecuteOnSyncContext(() => base.ClearItems());
    }
}