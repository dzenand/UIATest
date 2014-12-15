using System;

namespace TestUIA.Automation
{
    public interface IAutomationElementDataFactory
    {
        IAutomationElementData Create(
            IAutomationElementWrapper element,
            IntPtr rootWindowHandle,
            IAutomationElementData parentData,
            bool useCache = false);

        IAutomationElementData CreateEmptyRoot(int processId, IntPtr rootWindowHandle);

        ScreenElementId TryGetElementId(IAutomationElementWrapper element, IAutomationElementData parentData = null, bool useCache = false);
    }
}