using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace TestUIA.Process
{
    public sealed class ProcessManager : IProcessManager
    {
        private static readonly TimeSpan FailedAttemptTimeout = TimeSpan.FromSeconds(5);

        private readonly ConcurrentDictionary<int, IProcess> _cache;
        private readonly ConcurrentDictionary<int, DateTime> _failedAttemptCache;

        public ProcessManager()
        {
            _cache = new ConcurrentDictionary<int, IProcess>();
            _failedAttemptCache = new ConcurrentDictionary<int, DateTime>();
        }

        public bool TryGetProcess(string processId, out IProcess process)
        {
            return TryGetProcess(int.Parse(processId), out process);
        }

        public bool TryGetProcess(int processId, out IProcess process)
        {
            if (_cache.TryGetValue(processId, out process))
                return true;

            DateTime failedAttempt;
            if (_failedAttemptCache.TryGetValue(processId, out failedAttempt) &&
                (DateTime.UtcNow - failedAttempt) < FailedAttemptTimeout)
                return false;

            process = GetProcessById(processId);

            if (process != null)
            {
                try
                {
                    process.EnableRaisingEvents = true;
                    _cache.TryAdd(processId, process);
                    process.Exited += ProcessOnExited;
                    return true;
                }
                catch (Win32Exception)
                {
                    // When trying to enable rising events for some protected processes
                }
            }

            _failedAttemptCache.AddOrUpdate(processId, DateTime.UtcNow, (key, av) => DateTime.UtcNow);
            return false;
        }

        public void Clear()
        {
            var processes = new List<IProcess>(_cache.Values);
            _cache.Clear();
            foreach (var process in processes)
            {
                process.Exited -= ProcessOnExited;
            }

            _failedAttemptCache.Clear();
        }

        private void ProcessOnExited(object sender, EventArgs eventArgs)
        {
            var process = sender as IProcess;
            if (process != null)
            {
                _cache.TryRemove(process.Id, out process);
                process.Exited -= ProcessOnExited;
            }
        }

        private IProcess GetProcessById(int processId)
        {
            try
            {
                var process = System.Diagnostics.Process.GetProcessById(processId);
                return new Process(process);
            }
            catch (ArgumentException e)
            {
                // The process specified by the processId parameter is not running. The identifier might be expired.
                Debug.WriteLine(
                    string.Format(
                        "Could not create get System.Diagnostics.Process (process not running) from id ({0}): {1}",
                        processId,
                        e));
                return null;
            }
            catch (InvalidOperationException e)
            {
                // The process was not started by this object.
                Debug.WriteLine(
                    string.Format(
                        "Could not create get System.Diagnostics.Process (process was not started.) from id ({0}): {1}",
                        processId,
                        e));
                return null;
            }
            catch (Win32Exception e)
            {
                Debug.WriteLine(
                    string.Format(
                        "Could not create create Tobii.EyeX.Acceleration.SystemFacade.Process from id ({0}): {1}",
                        processId,
                        e));
                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format("Exception when creating System.Diagnostics.Process id: {0}", processId), e);
            }

            return null;
        }
    }
}