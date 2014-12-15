using System;
using System.Collections.Generic;
using System.Windows;

namespace TestUIA.Automation
{
    internal class AutomationElementDataFactory : IAutomationElementDataFactory
    {
        // Either supply an element with a valid runtime id, or supply an AutomationElementData created from parent element
        public IAutomationElementData Create(
            IAutomationElementWrapper element,
            IntPtr rootWindowHandle,
            IAutomationElementData parentData,
            bool useCache = false)
        {
            var id = TryGetElementId(element, parentData, useCache);
            if (id == null)
                throw new ArgumentException("Cannot obtain element id");

            return new AutomationElementData(
                id,
                element,
                parentData,
                rootWindowHandle,
                useCache);
        }

        public IAutomationElementData CreateEmptyRoot(int processId, IntPtr rootWindowHandle)
        {
            return new AutomationElementData(new ScreenElementId(new[] { processId }), null, null, rootWindowHandle, false);
        }

        public ScreenElementId TryGetElementId(IAutomationElementWrapper element, IAutomationElementData parentData = null, bool useCache = false)
        {
            int[] currentId = element.GetRuntimeId();
            if (currentId == null || currentId.Length == 0)
            {
                if (parentData != null)
                    return GenerateElementId(element, parentData, useCache);

                return null;
            }

            var elementInfo = useCache ? element.Cached : element.Current;
            var boundingRect = elementInfo.BoundingRectangle;

            // generate new id based on original id and bounds to avoid duplicates
            var tempIdData = new List<int>(currentId);
            tempIdData.AddRange(new[]
            {
                (int)boundingRect.Top,
                (int)boundingRect.Left,
                (int)boundingRect.Width,
                (int)boundingRect.Height
            });

            return new ScreenElementId(tempIdData.ToArray());
        }

        // generate new id based on parent id + some hopefully unique enough values from current element
        private ScreenElementId GenerateElementId(IAutomationElementWrapper element, IAutomationElementData parentData, bool useCache)
        {
            if (parentData == null)
                throw new ArgumentNullException("parentData", "Element does not have a valid RuntimeId, and no valid parent AutomationElementData object was supplied");

            var elementInfo = useCache ? element.Cached : element.Current;
            Rect boundingRect = elementInfo.BoundingRectangle;
            System.Windows.Automation.ControlType controlType = elementInfo.ControlType;
            string name = elementInfo.Name;

            // generate new id based on parent id + some hopefully unique enough values from current element
            var tempIdData = new List<int>(parentData.Id);
            tempIdData.AddRange(new[]
            {
                name.GetHashCode(),
                controlType.Id,
                (int)boundingRect.Top,
                (int)boundingRect.Left,
                (int)boundingRect.Width,
                (int)boundingRect.Height
            });

            return new ScreenElementId(tempIdData.ToArray());
        }
    }
}