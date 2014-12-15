using System;
using System.Windows.Automation;

namespace TestUIA.Automation
{
    public interface IAutomationElementWrapper : IDisposable
    {
        AutomationElement AutomationElement { get; }

        AutomationElement.AutomationElementInformation Cached { get; }

        AutomationElement.AutomationElementInformation Current { get; }

        DateTime TimestampUtc { get; }

        int[] GetRuntimeId();

        bool TryGetPropertyValue<T>(AutomationProperty property, bool isCached, out T value);

        bool TryGetPattern<T>(AutomationPattern property, bool isCached, out T value);

        object GetCachedPropertyValue(AutomationProperty property, bool ignoreDefaultValue);
    }
}