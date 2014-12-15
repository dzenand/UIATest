using System;
using System.Windows.Automation;
using TestUIA.Common;

namespace TestUIA.Automation
{
    public class AutomationElementWrapper : DisposableBase, IAutomationElementWrapper
    {
        public AutomationElementWrapper(AutomationElement automationElement)
        {
            AutomationElement = automationElement;
            TimestampUtc = DateTime.UtcNow;
        }

        public AutomationElement AutomationElement { get; private set; }

        public DateTime TimestampUtc { get; private set; }

        public AutomationElement.AutomationElementInformation Cached
        {
            get
            {
                CheckDisposed();

                return AutomationElement.Cached;
            }
        }

        public AutomationElement.AutomationElementInformation Current
        {
            get
            {
                CheckDisposed();

                return AutomationElement.Current;
            }
        }

        public int[] GetRuntimeId()
        {
            CheckDisposed();

            return AutomationElement.GetRuntimeId();
        }

        public bool TryGetPropertyValue<T>(AutomationProperty property, bool isCached, out T value)
        {
            CheckDisposed();

            return AutomationElement.TryGetPropertyValue(property, isCached, out value);
        }

        public bool TryGetPattern<T>(AutomationPattern property, bool isCached, out T value)
        {
            CheckDisposed();

            return AutomationElement.TryGetPattern(property, isCached, out value);
        }

        public object GetCachedPropertyValue(AutomationProperty property, bool ignoreDefaultValue)
        {
            CheckDisposed();

            return AutomationElement.GetCachedPropertyValue(property, ignoreDefaultValue);
        }

        protected override void DisposeManagedResources()
        {
            AutomationElement.Dispose();
            base.DisposeManagedResources();
        }

        private void CheckDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}