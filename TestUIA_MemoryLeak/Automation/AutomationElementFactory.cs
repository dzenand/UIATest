using System.Windows;
using System.Windows.Automation;

namespace TestUIA.Automation
{
    internal class AutomationElementFactory : IAutomationElementFactory
    {
        public IAutomationElementWrapper FromPoint(Point pt)
        {
            var automationElement = AutomationElement.FromPoint(pt);

            return CreateAutomationElement(automationElement);
        }

        public IAutomationElementWrapper GetRawViewWalkerParent(IAutomationElementWrapper element,
            CacheRequest cacheRequest = null)
        {
            var automationElement = cacheRequest == null
                ? TreeWalker.RawViewWalker.GetParent(element.AutomationElement)
                : TreeWalker.RawViewWalker.GetParent(element.AutomationElement, cacheRequest);

            return CreateAutomationElement(automationElement);
        }

        public IAutomationElementWrapper GetRawViewWalkerFirstChild(IAutomationElementWrapper element,
            CacheRequest cacheRequest = null)
        {

            var automationElement = cacheRequest == null
                ? TreeWalker.RawViewWalker.GetFirstChild(element.AutomationElement)
                : TreeWalker.RawViewWalker.GetFirstChild(element.AutomationElement, cacheRequest);

            return CreateAutomationElement(automationElement);
        }

        public IAutomationElementWrapper GetRawViewWalkerNextSibling(IAutomationElementWrapper element,
            CacheRequest cacheRequest = null)
        {
            var automationElement = cacheRequest == null
                ? TreeWalker.RawViewWalker.GetNextSibling(element.AutomationElement)
                : TreeWalker.RawViewWalker.GetNextSibling(element.AutomationElement, cacheRequest);

            return CreateAutomationElement(automationElement);
        }

        private IAutomationElementWrapper CreateAutomationElement(AutomationElement element)
        {
            if (element == null)
                return null;

            return new AutomationElementWrapper(element);
        }
    }
}