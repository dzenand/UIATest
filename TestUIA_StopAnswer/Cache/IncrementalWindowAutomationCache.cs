using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using TestUIA.Automation;
using TestUIA.Common;
using TestUIA.Process;

namespace TestUIA.Cache
{
    public class IncrementalWindowAutomationCache : DisposableBase
    {
        private static readonly IList<string> ProcessNamesWithDuplicatedProcessId = new List<string>
        {
            "iexplore"
        };

        private readonly IProcessInfo _processInfo;
        private readonly IntPtr _rootWindowHandle;

        private readonly IAutomationElementFactory _automationElementFactory;
        private readonly IAutomationElementDataFactory _automationElementDataFactory;
        private readonly CacheRequest _cacheRequest;
        private readonly object _syncLock;

        private Dictionary<ScreenElementId, IAutomationElementData> _dataCache;
        private ConcurrentDictionary<Point, IAutomationElementData> _requestCache;

        private ICacheInvalidationExecutant _invalidator;

        public IncrementalWindowAutomationCache(
            IProcessInfo processInfo, IntPtr rootWindowHandle, object syncLock)
            : this(processInfo, rootWindowHandle, new AutomationElementFactory(), new AutomationElementDataFactory(), syncLock)
        {
        }

        internal IncrementalWindowAutomationCache(
            IProcessInfo processInfo,
            IntPtr rootWindowHandle,
            IAutomationElementFactory automationElementFactory,
            IAutomationElementDataFactory automationElementDataFactory,
            object syncLock)
        {
            _processInfo = processInfo;
            _rootWindowHandle = rootWindowHandle;

            _cacheRequest = CreateCacheRequest();
            _dataCache = new Dictionary<ScreenElementId, IAutomationElementData>();
            _requestCache = new ConcurrentDictionary<Point, IAutomationElementData>();
            _automationElementFactory = automationElementFactory;
            _automationElementDataFactory = automationElementDataFactory;
            _syncLock = syncLock;
            InitInvalidator();
        }

        public event EventHandler Disposed = delegate { };

        public IntPtr RootWindowHandle
        {
            get { return _rootWindowHandle; }
        }

        public IAutomationElementData FindFromPoint(Point point)
        {
            CheckDisposed();

            // Truncating floating part
            point.X = (int)point.X;
            point.Y = (int)point.Y;

            lock (_dataCache)
            {
                IAutomationElementData data;
                if (_requestCache.TryGetValue(point, out data))
                {
                    return data;
                }

                data = FindAutomationElementFromPoint(point);

                return data;
            }
        }

        public IEnumerable<IAutomationElementData> GetElementChildren(IAutomationElementData automationElement)
        {
            lock (_dataCache)
            {
                // weve already clean this element
                if (!_dataCache.ContainsKey(automationElement.Id))
                    return new IAutomationElementData[0];

                // Checking if number of explored children equals real number of children
                if (automationElement.Children.Count() == automationElement.ChildrenCount)
                    return automationElement.Children;

                try
                {
                    // Traversing element's children
                    var child = _automationElementFactory.GetRawViewWalkerFirstChild(automationElement.Element, _cacheRequest);
                    while (child != null)
                    {
                        IAutomationElementData childData = null;
                        try
                        {
                            childData = _automationElementDataFactory.Create(child, automationElement.RootWindowHandle, automationElement, useCache: true);
                        }
                        catch
                        {
                            child.Dispose();
                            throw;
                        }

                        var toDispose = false;
                        if (_dataCache.ContainsKey(childData.Id))
                        {
                            toDispose = true;
                        }
                        else
                        {
                            _dataCache.Add(childData.Id, childData);
                            CreateAncestorPath(childData, automationElement);
                            automationElement.AddChild(childData);
                        }

                        try
                        {
                            child = _automationElementFactory.GetRawViewWalkerNextSibling(child, _cacheRequest);
                        }
                        finally
                        {
                            if (toDispose)
                                childData.Dispose();
                        }
                    }
                }
                catch
                {
                    return new IAutomationElementData[0];
                }

                return automationElement.Children;
            }
        }

        protected override void DisposeManagedResources()
        {
            OnDisposed();

            if (_invalidator != null)
            {
                _invalidator.Invalidate -= InvalidatorOnInvalidate;
                _invalidator.Dispose();
            }

            Clear();
            AutomationElementExtensions.ForceCleanAutomationElements();

            base.DisposeManagedResources();
        }

        private static void AddElementAndItsChildren(
            ISet<IAutomationElementData> elements,
            IAutomationElementData rootElement,
            ISet<IAutomationElementData> visited)
        {
            if (visited.Contains(rootElement))
                return;

            elements.Add(rootElement);
            visited.Add(rootElement);
            foreach (var element in rootElement.Children)
            {
                AddElementAndItsChildren(elements, element, visited);
            }
        }

        private static CacheRequest CreateCacheRequest()
        {
            var cacheRequest = new CacheRequest();
            cacheRequest.AutomationElementMode = AutomationElementMode.Full;
            cacheRequest.TreeScope = TreeScope.Element;
            AutomationElementData.UsedProperties.ToList().ForEach(cacheRequest.Add);
            AutomationElementData.UsedPatterns.ToList().ForEach(cacheRequest.Add);

            return cacheRequest;
        }

        private void DisposeElementsAsync(IEnumerable<IAutomationElementData> elements)
        {
            Task.Run(() =>
            {
                foreach (var element in elements)
                {
                    lock (_syncLock)
                    {
                        try
                        {
                            element.Dispose();
                        }
                        catch
                        {
                        }
                    }
                }
            });
        }

        private void Clear()
        {
            var elements = new List<IAutomationElementData>(_dataCache.Values);
            _dataCache.Clear();
            _requestCache.Clear();
            DisposeElementsAsync(elements);
        }

        private void CheckDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        private void OnDisposed()
        {
            Disposed(this, EventArgs.Empty);
        }

        private void InitInvalidator()
        {
            var invalidatorFactory = new InvalidatorFactory();
            _invalidator = invalidatorFactory.CreateExecutant();

            // For tests
            if (_invalidator == null)
                return;

            if (!_invalidator.Init(_processInfo.Id, _rootWindowHandle))
                throw new InvalidOperationException("Window invalidator cannot be created");

            _invalidator.Invalidate += InvalidatorOnInvalidate;
        }

        private void InvalidatorOnInvalidate(object sender, InvalidateEventArgs invalidateEventArgs)
        {
            if (IsDisposed)
                return;

            if (!(invalidateEventArgs is PartialInvalidateEventArgs))
            {
                lock (_syncLock)
                {
                    Dispose();
                    return;
                }
            }

            var visualEventArgs = invalidateEventArgs as VisualInvalidateEventArgs;
            if (visualEventArgs != null)
            {
                ProcessChangesInRectangles(visualEventArgs.ChangedRectangles);
                return;
            }

            throw new NotImplementedException("Unknown invalidate event type");
        }

        private void ProcessChangesInRectangles(IEnumerable<Rectangle> changedRectangles)
        {
            CheckDisposed();

            var rects = changedRectangles.ToList();
            lock (_dataCache)
            {
                try
                {
                    const double ToInvalidateAreaCoveragePercent = 0.8;
                    var toRemove = new HashSet<IAutomationElementData>();
                    foreach (var element in _dataCache.Values)
                    {
                        if (toRemove.Contains(element))
                            continue;

                        var visibleBounds = element.VisibleBounds;
                        if (visibleBounds.IsEmpty)
                        {
                            toRemove.Add(element);
                            continue;
                        }

                        double coveredArea = 0;
                        foreach (var changedRectangle in rects)
                        {
                            changedRectangle.Intersection(element.VisibleBounds);
                            coveredArea += changedRectangle.Area;
                        }

                        if (coveredArea > visibleBounds.Area * ToInvalidateAreaCoveragePercent)
                            AddElementAndItsChildren(toRemove, element, new HashSet<IAutomationElementData>());
                    }

                    RemoveElements(toRemove.ToList());
                    RemovePointsInRectangles(rects);
                }
                catch
                {
                    Clear();
                }
            }
        }

        private void RemoveElements(List<IAutomationElementData> elements)
        {
            lock (_dataCache)
            {
                elements.Where(t => t.Parent != null).ToList().ForEach(t => t.Parent.RemoveChild(t));
                elements.ForEach(t => _dataCache.Remove(t.Id));

                var keysToRemove = _requestCache.Where(rc => elements.Any(dc => rc.Value.Equals(dc))).Select(t => t.Key).ToList();

                IAutomationElementData dummy;
                keysToRemove.ForEach(t => _requestCache.TryRemove(t, out dummy));

                var emptyParents = elements.Where(t => t.Parent != null && !t.Parent.Children.Any()).Select(t => t.Parent).ToList();
                if (emptyParents.Any())
                    RemoveElements(emptyParents);

                DisposeElementsAsync(elements);
            }
        }

        private void RemovePointsInRectangles(IEnumerable<Rectangle> rectangles)
        {
            var keysToRemove = _requestCache
                .Where(rc => rectangles.Any(rect => rect.Contains(rc.Key.X, rc.Key.Y)))
                .Select(rc => rc.Key).ToList();

            IAutomationElementData dummy;
            keysToRemove.ForEach(t => _requestCache.TryRemove(t, out dummy));
        }

        private IAutomationElementData FindAutomationElementFromPoint(Point point)
        {
            IAutomationElementData data = null;
            try
            {
                var element = GetElementWrapperFromPoint(point);
                if (element == null)
                    return null;

                data = CacheElementAndAncestors(element);
                if (data != null && IsCheckableElement(data))
                    data = _requestCache.GetOrAdd(point, data);
            }
            catch (Exception)
            {
            }

            return data;
        }

        private IAutomationElementWrapper GetElementWrapperFromPoint(Point point)
        {
            IAutomationElementWrapper element = null;
            try
            {
                using (_cacheRequest.Activate())
                {
                    element = _automationElementFactory.FromPoint(point);
                    if (element != null)
                    {
                        if (element.Cached.ProcessId != _processInfo.Id
                            && ProcessNamesWithDuplicatedProcessId.All(c => !c.Equals(_processInfo.Name)))
                        {
                            element.Dispose();
                            return null;
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (element != null)
                    element.Dispose();

                element = null;
            }

            return element;
        }
        private IAutomationElementData CacheElementAndAncestors(IAutomationElementWrapper element)
        {
            IAutomationElementData data = null;
            try
            {
                data = RecursivelyFindAndCacheElements(element);
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (ElementNotAvailableException)
            {
            }
            catch (COMException)
            {
                // might be a timeout. we can not deliver a reliable result
            }

            return data;
        }

        // method responsible for dispose current or return IAutomationElementData
        private IAutomationElementData RecursivelyFindAndCacheElements(IAutomationElementWrapper current)
        {
            try
            {
                var currentData = GetByAutomationElement(current);
                if (currentData != null)
                {
                    current.Dispose();
                    return currentData;
                }

                IAutomationElementWrapper parent;
                IAutomationElementData parentData;
                bool hasParent = TryGetParentAutomationElement(current, _cacheRequest, out parent);
                if (hasParent)
                {
                    parentData = RecursivelyFindAndCacheElements(parent);
                }
                else
                {
                    if (!CheckWindowHandle(current))
                    {
                        current.Dispose();
                        return null;
                    }

                    parentData = _automationElementDataFactory.CreateEmptyRoot(_processInfo.Id, _rootWindowHandle);
                }

                if (parentData == null)
                {
                    current.Dispose();
                    return null;
                }

                var currentDataToCheckForId = _automationElementDataFactory.Create(current, _rootWindowHandle, parentData);
                if (_dataCache.ContainsKey(currentDataToCheckForId.Id))
                {
                    var currentId = currentDataToCheckForId.Id;
                    currentDataToCheckForId.Dispose();

#if DEBUG
                    // ToDo: Check next assertions. They should be true, but for some items in Windows Explorer Ribbon they are not
                    var parentContainsChild = parentData.Children.Contains(_dataCache[currentId]);
                    var notEmptyAnsPath = _dataCache[currentId].AncestorPath.Count != 0;
#endif
                    return _dataCache[currentId];
                }

                // Taking as current our element created for check
                currentData = currentDataToCheckForId;
                CreateAncestorPath(currentData, parentData);
                parentData.AddChild(currentData);

                if (IsCheckableElement(currentData))
                    _dataCache.Add(currentData.Id, currentData);

                return currentData;
            }
            catch (Exception)
            {
                current.Dispose();
                throw;
            }
        }

        private void CreateAncestorPath(IAutomationElementData data, IAutomationElementData parentData)
        {
            foreach (var id in parentData.AncestorPath)
                data.AncestorPath.Add(id);

            data.AncestorPath.Add(parentData.Id);
        }

        private IAutomationElementData GetByAutomationElement(IAutomationElementWrapper automationElement)
        {
            var id = _automationElementDataFactory.TryGetElementId(automationElement);
            if (id == null)
                return null;

            IAutomationElementData data;
            if (_dataCache.TryGetValue(id, out data))
                return data;

            return null;
        }

        private bool TryGetParentAutomationElement(IAutomationElementWrapper current, CacheRequest cache,
            out IAutomationElementWrapper parent)
        {
            parent = null;
            if (IsRootByHandle(current))
            {
                return false;
            }

            // Do not hide automation exceptions here. They is handled in outer scope
            parent = _automationElementFactory.GetRawViewWalkerParent(current, cache);

            try
            {
                if (parent == null)
                    return false;

                var processId = cache == null
                    ? parent.Current.ProcessId
                    : parent.Cached.ProcessId;

                if (_processInfo.Id != processId &&
                    ProcessNamesWithDuplicatedProcessId.All(c => !c.Equals(_processInfo.Name)))
                {
                    parent.Dispose();
                    return false;
                }

                // RootElement is created new every get
                using (var rootElement = AutomationElement.RootElement)
                {
                    if (parent.AutomationElement == rootElement)
                    {
                        parent.Dispose();
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                if (parent != null)
                    parent.Dispose();

                throw;
            }
        }

        private bool CheckWindowHandle(IAutomationElementWrapper element)
        {
            var nativeHandle = element.GetCachedPropertyValue(AutomationElement.NativeWindowHandleProperty, true);
            if (nativeHandle == AutomationElement.NotSupported)
                return true;

            var windowHandle = new IntPtr((int)nativeHandle);
            if (windowHandle != _rootWindowHandle)
            {
                return false;
            }

            return true;
        }

        private bool IsRootByHandle(IAutomationElementWrapper element)
        {
            var nativeHandle = element.GetCachedPropertyValue(AutomationElement.NativeWindowHandleProperty, true);
            if (nativeHandle == AutomationElement.NotSupported)
                return false;

            var windowHandle = new IntPtr((int)nativeHandle);
            return windowHandle == _rootWindowHandle;
        }

        // some popup windows do not exist in cache, we skip them, when we find them by AutomationElement.FromPoint
        private bool IsCheckableElement(IAutomationElementData elementData)
        {
            return !(string.Equals(elementData.AutomationId, "Light Dismiss", StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(elementData.ClassName, "PopupRoot", StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(elementData.FrameworkId, "XAML", StringComparison.OrdinalIgnoreCase));
        }
    }
}