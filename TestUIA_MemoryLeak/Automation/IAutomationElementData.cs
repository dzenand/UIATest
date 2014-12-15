using System;
using System.Collections.Generic;
using System.Windows;
using TestUIA.Common;

namespace TestUIA.Automation
{
    public interface IAutomationElementData
    {
        ScreenElementId Id { get; }
        IAutomationElementData Parent { get; }
        bool IsRoot { get; }
        IntPtr RootWindowHandle { get; }
        HashSet<ScreenElementId> AncestorPath { get; }
        IEnumerable<IAutomationElementData> Children { get; }
        string Name { get; }
        string ClassName { get; }
        string RootWindowClassName { get; }
        Rectangle Bounds { get; }
        Rectangle VisibleBounds { get; }
        ControlTypeId ControlType { get; }
        string AutomationId { get; }
        string FrameworkId { get; }
        AvailableScrollDirections AvailableScrollDirection { get; }
        Point ScrollPercent { get; }
        bool IsScrollPatternAvailable { get; }
        bool IsInvokePatternAvailable { get; }
        bool IsTogglePatternAvailable { get; }
        bool IsSelectionPatternAvailable { get; }
        bool IsSelectionItemPatternAvailable { get; }
        bool IsExpandCollapsePatternAvailable { get; }
        ExpandCollapseState ExpandCollapseState { get; }
        bool IsKeyboardFocusable { get; }
        int ChildrenCount { get; }
        bool HasChildren { get; }
        IAutomationElementWrapper Element { get; }
        DateTime TimestampUtc { get; }

        void AddChild(IAutomationElementData child);
        void RemoveChild(IAutomationElementData child);
    }
}