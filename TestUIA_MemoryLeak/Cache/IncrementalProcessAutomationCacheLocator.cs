using System;
using System.Collections.Generic;
using System.ComponentModel;
using TestUIA.Process;

namespace TestUIA.Cache
{
    public class IncrementalProcessAutomationCacheLocator
    {
        private readonly IProcessManager _processManager;
        private readonly IDictionary<int, IncrementalProcessAutomationCache> _caches;
        private readonly object _cachesLock;

        public IncrementalProcessAutomationCacheLocator()
        {
            _processManager = new ProcessManager();
            _caches = new Dictionary<int, IncrementalProcessAutomationCache>();
            _cachesLock = new object();
        }

        public IncrementalProcessAutomationCache GetForProcess(int processId)
        {
            IncrementalProcessAutomationCache foundCache;
            if (TryGetCache(processId, out foundCache))
                return foundCache;

            return null;
        }

        private bool TryGetCache(int processId, out IncrementalProcessAutomationCache foundCache)
        {
            lock (_cachesLock)
            {
                if (_caches.TryGetValue(processId, out foundCache))
                    return true;

                foundCache = CreateCacheFrom(processId);
                if (foundCache != null)
                {
                    _caches.Add(processId, foundCache);
                    return true;
                }

                return false;
            }
        }

        private IncrementalProcessAutomationCache CreateCacheFrom(int processId)
        {
            IProcess process;
            if (_processManager.TryGetProcess(processId, out process))
            {
                try
                {
                    process.EnableRaisingEvents = true;
                    process.Exited += ProcessOnExited;
                    return new IncrementalProcessAutomationCache(process);
                }
                catch (Win32Exception)
                {
                    /*Catch exceptions from elevated processes */
                }
            }

            return null;
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            var process = sender as IProcess;

            if (process != null)
            {
                lock (_cachesLock)
                {
                    _caches.Remove(process.Id);
                }

                process.Exited -= ProcessOnExited;
            }
        }
    }
}