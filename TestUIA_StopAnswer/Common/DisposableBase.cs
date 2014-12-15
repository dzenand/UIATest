using System;
using System.Diagnostics;
using System.Threading;

namespace TestUIA.Common
{
    public abstract class DisposableBase
    {
        private int _isDisposed; // Interlocked.CompareExchange does not support bools.

        ~DisposableBase()
        {
            Dispose(false);
        }

        public bool IsDisposed
        {
            get { return _isDisposed == 1; }
        }

        [DebuggerStepThrough]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposingManagedResources)
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
                return;

            if (isDisposingManagedResources)
                DisposeManagedResources();

            DisposeNativeResources();
        }

        protected virtual void DisposeManagedResources()
        { }

        protected virtual void DisposeNativeResources()
        { }
    }
}