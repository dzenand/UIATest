using System;
using System.Collections.Generic;
using System.Windows;
using TestUIA.Automation;
using TestUIA.Common;
using TestUIA.Process;

namespace TestUIA.Cache
{
    public class IncrementalProcessAutomationCache : DisposableBase
    {
        private readonly IProcessInfo _processInfo;
        private readonly IDictionary<IntPtr, IncrementalWindowAutomationCache> _cache;
        private readonly object _cacheLock;

        public IncrementalProcessAutomationCache(
            IProcessInfo processInfo)
        {
            _processInfo = processInfo;
            _cacheLock = new object();
            _cache = new Dictionary<IntPtr, IncrementalWindowAutomationCache>();
            SyncLock = new object();
        }

        public object SyncLock { get; private set; }

        public IAutomationElementData FindFromPoint(Point point, IntPtr rootWindowHandle)
        {
            var incrementalWindowAutomationCache = GetIncrementalWindowAutomationCache(rootWindowHandle);
            if (incrementalWindowAutomationCache != null)
                return incrementalWindowAutomationCache.FindFromPoint(point);

            return null;
        }

        public IEnumerable<IAutomationElementData> GetElementChildren(IAutomationElementData automationElement)
        {
            var cache = GetIncrementalWindowAutomationCache(automationElement.RootWindowHandle);
            if (cache != null)
                return cache.GetElementChildren(automationElement);

            return new AutomationElementData[0];
        }

        protected override void DisposeManagedResources()
        {
            lock (_cacheLock)
            {
                foreach (var incrementalWindowAutomationCache in _cache.Values)
                {
                    incrementalWindowAutomationCache.Disposed -= IncrementalWindowAutomationCacheOnDisposed;
                    incrementalWindowAutomationCache.Dispose();
                }
            }

            base.DisposeManagedResources();
        }

        private IncrementalWindowAutomationCache GetIncrementalWindowAutomationCache(IntPtr windowHandle)
        {
            lock (_cacheLock)
            {
                IncrementalWindowAutomationCache incrementalWindowAutomationCache;
                var exists = _cache.TryGetValue(windowHandle, out incrementalWindowAutomationCache);
                if (exists)
                {
                    if (incrementalWindowAutomationCache.IsDisposed)
                    {
                        _cache.Remove(windowHandle);
                    }
                    else
                    {
                        return incrementalWindowAutomationCache;
                    }
                }

                try
                {
                    incrementalWindowAutomationCache = new IncrementalWindowAutomationCache(_processInfo, windowHandle, SyncLock);
                    incrementalWindowAutomationCache.Disposed += IncrementalWindowAutomationCacheOnDisposed;
                    _cache[windowHandle] = incrementalWindowAutomationCache;
                    return incrementalWindowAutomationCache;
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
        }

        private void IncrementalWindowAutomationCacheOnDisposed(object sender, EventArgs eventArgs)
        {
            if (IsDisposed)
                return;

            var windowAutomationCache = sender as IncrementalWindowAutomationCache;
            if (windowAutomationCache != null)
            {
                lock (_cacheLock)
                {
                    IncrementalWindowAutomationCache cachedWindowAutomationCache;
                    if (_cache.TryGetValue(windowAutomationCache.RootWindowHandle, out cachedWindowAutomationCache) &&
                        ReferenceEquals(cachedWindowAutomationCache, windowAutomationCache))
                    {
                        windowAutomationCache.Disposed -= IncrementalWindowAutomationCacheOnDisposed;
                        _cache.Remove(windowAutomationCache.RootWindowHandle);
                    }
                }
            }
        }
    }
}