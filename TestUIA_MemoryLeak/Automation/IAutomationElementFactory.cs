using System.Windows;
using System.Windows.Automation;

namespace TestUIA.Automation
{
    public interface IAutomationElementFactory
    {
        IAutomationElementWrapper FromPoint(Point pt);

        IAutomationElementWrapper GetRawViewWalkerParent(IAutomationElementWrapper element, CacheRequest cacheRequest = null);

        IAutomationElementWrapper GetRawViewWalkerFirstChild(IAutomationElementWrapper element, CacheRequest cacheRequest = null);

        IAutomationElementWrapper GetRawViewWalkerNextSibling(IAutomationElementWrapper element, CacheRequest cacheRequest = null);
    }
}