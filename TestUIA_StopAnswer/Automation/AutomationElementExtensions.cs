using System;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace TestUIA.Automation
{
    public static class AutomationElementExtensions
    {
        private static readonly object GlobalCleanLock = new object();
        private static readonly TimeSpan CleanAutomationElementsThreshold = TimeSpan.FromSeconds(5);

        private static DateTime _lastCleanAutomationElementsTime = default(DateTime);

        public static void ForceCleanAutomationElements()
        {
            Task.Run(() =>
                {
                    lock (GlobalCleanLock)
                    {
                        if ((DateTime.Now.ToUniversalTime() - _lastCleanAutomationElementsTime) > CleanAutomationElementsThreshold)
                        {
                            _lastCleanAutomationElementsTime = DateTime.Now.ToUniversalTime();
                            var transactionTimeout = System.Windows.Automation.Automation.TransactionTimeout;
                            System.Windows.Automation.Automation.ConnectionTimeout = 50;
                            try
                            {
                                GC.Collect();
                                GC.WaitForPendingFinalizers();

                                // clean resurrected object
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                            }
                            finally
                            {
                                System.Windows.Automation.Automation.TransactionTimeout = transactionTimeout;
                            }
                        }
                    }
                });
        }
        
        public static bool TryGetPropertyValue<T>(this AutomationElement element, AutomationProperty property, bool isCached, out T value)
        {
            if (element != null)
            {
                try
                {
                    var obj = isCached
                        ? element.GetCachedPropertyValue(property)
                        : element.GetCurrentPropertyValue(property);

                    if (obj != null)
                    {
                        value = (T)obj;
                        return true;
                    }
                }
                catch (ElementNotAvailableException)
                {
                }
                catch (Exception)
                {
                }
            }

            value = default(T);
            return false;
        }

        public static bool TryGetPattern<T>(this AutomationElement element, AutomationPattern pattern, bool isCached, out T value)
        {
            var success = false;
            object obj = null;
            if (element != null)
            {
                try
                {
                    success = isCached ? element.TryGetCachedPattern(pattern, out obj) : element.TryGetCurrentPattern(pattern, out obj);
                }
                catch (Exception)
                {
                    obj = null;
                    success = false;
                }
            }

            value = (T)obj;
            return success;
        }

        public static bool TryGetScreenElementId(this AutomationElement element, out ScreenElementId screenElementId)
        {
            screenElementId = null;
            if (element == null) return false;

            try
            {
                screenElementId = new ScreenElementId(element.GetRuntimeId());
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}