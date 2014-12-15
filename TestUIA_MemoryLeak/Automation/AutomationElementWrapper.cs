using System;
using System.Windows.Automation;

namespace TestUIA.Automation
{
    public class AutomationElementWrapper : IAutomationElementWrapper
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
                return AutomationElement.Cached;
            }
        }

        public AutomationElement.AutomationElementInformation Current
        {
            get
            {
                return AutomationElement.Current;
            }
        }

        public int[] GetRuntimeId()
        {
            return AutomationElement.GetRuntimeId();
        }

        public bool TryGetPropertyValue<T>(AutomationProperty property, bool isCached, out T value)
        {
            return AutomationElement.TryGetPropertyValue(property, isCached, out value);
        }

        public bool TryGetPattern<T>(AutomationPattern property, bool isCached, out T value)
        {
            return AutomationElement.TryGetPattern(property, isCached, out value);
        }

        public object GetCachedPropertyValue(AutomationProperty property, bool ignoreDefaultValue)
        {
            return AutomationElement.GetCachedPropertyValue(property, ignoreDefaultValue);
        }
    }
}