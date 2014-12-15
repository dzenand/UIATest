using System;

namespace TestUIA.Automation
{
    public static class ExpandCollapseStateUtils
    {
        public static ExpandCollapseState ConvertFromAutomation(System.Windows.Automation.ExpandCollapseState expandCollapseState)
        {
            switch (expandCollapseState)
            {
                case System.Windows.Automation.ExpandCollapseState.Collapsed:
                    return ExpandCollapseState.Collapsed;
                case System.Windows.Automation.ExpandCollapseState.Expanded:
                    return ExpandCollapseState.Expanded;
                case System.Windows.Automation.ExpandCollapseState.LeafNode:
                    return ExpandCollapseState.LeafNode;
                case System.Windows.Automation.ExpandCollapseState.PartiallyExpanded:
                    return ExpandCollapseState.PartiallyExpanded;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}