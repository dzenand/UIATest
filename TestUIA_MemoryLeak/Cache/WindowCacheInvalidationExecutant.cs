using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TestUIA.Common;

namespace TestUIA.Cache
{
    [SuppressMessage("Microsoft.Design", "CA1063", Justification = "It is correctly implemented")]
    public class WindowCacheInvalidationExecutant : DisposableBase, ICacheInvalidationExecutant
    {
        private readonly IList<ICacheInvalidationExecutant> _invalidators;
        private readonly object _lock;

        public WindowCacheInvalidationExecutant(IEnumerable<ICacheInvalidationExecutant> invalidators)
        {
            _invalidators = new List<ICacheInvalidationExecutant>(invalidators);
            _lock = new object();
        }

        public event EventHandler<InvalidateEventArgs> Invalidate;

        public bool Init(int processId, IntPtr windowHandle)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("WindowCacheInvalidationExecutant");

            lock (_lock)
            {
                foreach (var invalidator in _invalidators)
                {
                    invalidator.Invalidate += InvalidatorOnInvalidate;
                    if (!invalidator.Init(processId, windowHandle))
                        return false;
                }

                return true;
            }
        }

        protected override void DisposeManagedResources()
        {
            lock (_lock)
            {
                foreach (var invalidator in _invalidators)
                {
                    invalidator.Invalidate -= InvalidatorOnInvalidate;
                    invalidator.Dispose();
                }

                _invalidators.Clear();
            }

            base.DisposeManagedResources();
        }

        private void InvalidatorOnInvalidate(object sender, InvalidateEventArgs eventArgs)
        {
            if (IsDisposed)
                return;

            OnInvalidate(eventArgs);
        }

        private void OnInvalidate(InvalidateEventArgs eventArgs)
        {
            var handler = Invalidate;
            if (handler != null)
                handler(this, eventArgs);
        }
    }
}