﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Shapes;
using TestUIA.Common;
using Rectangle = TestUIA.Common.Rectangle;

namespace TestUIA.Automation
{
    public class AutomationElementData : DisposableBase, IAutomationElementData
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "It's ok.")]
        public static readonly IEnumerable<AutomationProperty> UsedProperties = new[]
        {
            AutomationElement.NameProperty,
            AutomationElement.ClassNameProperty,
            AutomationElement.BoundingRectangleProperty,
            AutomationElement.ControlTypeProperty,
            AutomationElement.AutomationIdProperty,
            AutomationElement.FrameworkIdProperty,
            AutomationElement.ProcessIdProperty,
            AutomationElement.NativeWindowHandleProperty,
            AutomationElement.IsScrollPatternAvailableProperty,

            ScrollPattern.HorizontalScrollPercentProperty,
            ScrollPattern.HorizontalViewSizeProperty,
            ScrollPattern.HorizontallyScrollableProperty,
            ScrollPattern.VerticalScrollPercentProperty,
            ScrollPattern.VerticalViewSizeProperty,
            ScrollPattern.VerticallyScrollableProperty,

            AutomationElement.IsInvokePatternAvailableProperty,

            AutomationElement.IsTogglePatternAvailableProperty,
            AutomationElement.IsSelectionPatternAvailableProperty,
            AutomationElement.IsSelectionItemPatternAvailableProperty,
            AutomationElement.IsExpandCollapsePatternAvailableProperty,

            ExpandCollapsePattern.ExpandCollapseStateProperty,
            AutomationElement.IsKeyboardFocusableProperty
        };

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "It's ok.")]
        public static readonly IEnumerable<AutomationPattern> UsedPatterns = new[]
        {
            ScrollPattern.Pattern,
            InvokePattern.Pattern,
            ExpandCollapsePattern.Pattern
        };

        private readonly bool _useCache;
        private readonly ISet<IAutomationElementData> _children;

        // NOTE: null means that they haven't been asked yet
        private string _name;
        private string _className;
        private string _rootWindowClassName;
        private string _ariaRole;
        private Rectangle? _bounds;
        private Rectangle? _visibleBounds;
        private ControlTypeId? _controlType;
        private bool? _isScrollPatternAvailable;
        private bool? _isInvokePatternAvailable;
        private bool? _isTogglePatternAvailable;
        private bool? _isSelectionPatternAvailable;
        private bool? _isSelectionItemPatternAvailable;
        private bool? _isExpandCollapseItemPatternAvailable;
        private bool? _isKeyboardFocusable;
        private bool? _hasKeyboardFocus;
        private bool? _isOffscreen;
        private int? _childrenCount;
        private bool? _hasChildren;

        private AvailableScrollDirections _availableScrollDirection;
        private ExpandCollapseState _expandCollapseState;
        private string _automationId;
        private string _frameworkId;
        private double _scrollPercentageHorizontal;
        private double _scrollPercentageVertical;
        private ScreenElementId _id;
        private IAutomationElementData _parent;
        private IntPtr _rootWindowHandle;
        private IAutomationElementWrapper _element;
        private HashSet<ScreenElementId> _ancestorPath;

        internal AutomationElementData(
            ScreenElementId id,
            IAutomationElementWrapper element,
            IAutomationElementData parentData,
            IntPtr rootWindowHandle,
            bool useCache)
        {
            Id = id;
            _element = element;
            Parent = parentData;
            RootWindowHandle = rootWindowHandle;
            _useCache = useCache;

            _children = new HashSet<IAutomationElementData>();
            AncestorPath = new HashSet<ScreenElementId>();
            _availableScrollDirection = AvailableScrollDirections.None;
            _expandCollapseState = ExpandCollapseState.None;
            _scrollPercentageHorizontal = -1d;
            _scrollPercentageVertical = -1d;

            TimestampUtc = _element != null ? element.TimestampUtc : DateTime.UtcNow;
        }

        public ScreenElementId Id
        {
            get
            {
                return _id;
            }

            private set
            {
                _id = value;
            }
        }

        public IAutomationElementData Parent
        {
            get
            {
                return _parent;
            }

            private set
            {
                _parent = value;
            }
        }

        public bool IsRoot
        {
            get
            {
                return _parent == null;
            }
        }

        public IntPtr RootWindowHandle
        {
            get
            {
                CheckDisposed();

                return _rootWindowHandle;
            }

            private set
            {
                _rootWindowHandle = value;
            }
        }

        public HashSet<ScreenElementId> AncestorPath
        {
            get
            {
                CheckDisposed();

                return _ancestorPath;
            }

            private set
            {
                _ancestorPath = value;
            }
        }

        public IEnumerable<IAutomationElementData> Children
        {
            get
            {
                CheckDisposed();

                return _children;
            }
        }

        public string Name
        {
            get
            {
                CheckDisposed();

                LazySetName();
                return _name;
            }
        }

        public string ClassName
        {
            get
            {
                CheckDisposed();

                LazySetClassName();
                return _className;
            }
        }

        public string RootWindowClassName
        {
            get
            {
                CheckDisposed();

                LazySetRootWindowClassName();
                return _rootWindowClassName;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                CheckDisposed();

                LazySetBounds();
                return _bounds ?? Rectangle.Empty;
            }
        }

        public Rectangle VisibleBounds
        {
            get
            {
                LazySetVisibleBounds();
                return _visibleBounds ?? Rectangle.Empty;
            }
        }

        public ControlTypeId ControlType
        {
            get
            {
                CheckDisposed();

                LazySetControlType();
                return _controlType ?? ControlTypeId.None;
            }
        }

        public string AutomationId
        {
            get
            {
                CheckDisposed();

                LazySetAutomationId();
                return _automationId;
            }
        }

        public string FrameworkId
        {
            get
            {
                CheckDisposed();

                LazySetFrameworkId();
                return _frameworkId;
            }
        }

        public AvailableScrollDirections AvailableScrollDirection
        {
            get
            {
                CheckDisposed();

                LazySetIsScrollPatternAvailable();
                return _availableScrollDirection;
            }
        }

        public Point ScrollPercent
        {
            get
            {
                CheckDisposed();

                LazySetIsScrollPatternAvailable();
                return new Point(_scrollPercentageHorizontal, _scrollPercentageVertical);
            }
        }

        public bool IsScrollPatternAvailable
        {
            get
            {
                CheckDisposed();

                LazySetIsScrollPatternAvailable();
                return _isScrollPatternAvailable != null && _isScrollPatternAvailable.Value;
            }
        }

        public bool IsInvokePatternAvailable
        {
            get
            {
                CheckDisposed();

                LazySetIsInvokePatternAvailable();
                return _isInvokePatternAvailable != null && _isInvokePatternAvailable.Value;
            }
        }

        public bool IsTogglePatternAvailable
        {
            get
            {
                CheckDisposed();

                LazySetIsTogglePatternAvailable();
                return _isTogglePatternAvailable.GetValueOrDefault(false);
            }
        }

        public bool IsSelectionPatternAvailable
        {
            get
            {
                CheckDisposed();

                LazySetIsSelectionPatternAvailable();
                return _isSelectionPatternAvailable.GetValueOrDefault(false);
            }
        }

        public bool IsSelectionItemPatternAvailable
        {
            get
            {
                CheckDisposed();

                LazySetIsSelectionItemPatternAvailable();
                return _isSelectionItemPatternAvailable.GetValueOrDefault(false);
            }
        }

        public bool IsExpandCollapsePatternAvailable
        {
            get
            {
                CheckDisposed();

                LazySetIsExpandCollapsePatternAvailable();
                return _isExpandCollapseItemPatternAvailable.GetValueOrDefault(false);
            }
        }

        public ExpandCollapseState ExpandCollapseState
        {
            get
            {
                CheckDisposed();

                LazySetIsExpandCollapsePatternAvailable();
                return _expandCollapseState;
            }
        }

        public bool IsKeyboardFocusable
        {
            get
            {
                CheckDisposed();

                LazySetIsKeyboardFocusable();
                return _isKeyboardFocusable.GetValueOrDefault(false);
            }
        }

        public bool IsOffscreen
        {
            get
            {
                CheckDisposed();

                LazySetIsOffscreen();
                return _isOffscreen.GetValueOrDefault(false);
            }
        }

        public bool HasKeyboardFocus
        {
            get
            {
                CheckDisposed();

                LazySetHasKeyboardFocus();
                return _hasKeyboardFocus.GetValueOrDefault(false);
            }
        }

        public string AriaRole
        {
            get
            {
                CheckDisposed();

                LazySetAriaRole();
                return _ariaRole;
            }
        }

        public int ChildrenCount
        {
            get
            {
                LazySetChildrenCount();
                return _childrenCount.GetValueOrDefault(0);
            }
        }

        public bool HasChildren
        {
            get
            {
                LazySetHasChildren();
                return _hasChildren.GetValueOrDefault(false);
            }
        }

        public IAutomationElementWrapper Element
        {
            get
            {
                return _element;
            }
        }

        public DateTime TimestampUtc { get; private set; }

        public override string ToString()
        {
            CheckDisposed();

            if (IsRoot)
                return "Root";

            return string.Format(
                "Name = {0}, AutomationId = {1}, ClassName = {2}, ControlType = {3}, Bounds = {4}",
                Name,
                AutomationId,
                ClassName,
                ControlType,
                Bounds);
        }

        public void AddChild(IAutomationElementData child)
        {
            CheckDisposed();

            if (!_children.Contains(child))
            {
                _children.Add(child);

                if (_isScrollPatternAvailable.HasValue
                    && _isScrollPatternAvailable.Value
                    && _availableScrollDirection == AvailableScrollDirections.None)
                {
                    _isScrollPatternAvailable = null;
                }
            }
        }

        public void RemoveChild(IAutomationElementData child)
        {
            CheckDisposed();

            _children.Remove(child);
        }

        public override int GetHashCode()
        {
            CheckDisposed();

            return
                unchecked(
                    Id.GetHashCode() + AutomationId.GetHashCode() + RootWindowHandle.GetHashCode() +
                    ClassName.GetHashCode() + Bounds.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            CheckDisposed();

            var other = obj as AutomationElementData;
            if (other == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return other.Id.Equals(Id) &&
                other.AutomationId.Equals(AutomationId) &&
                other.RootWindowHandle.Equals(RootWindowHandle) &&
                other.Bounds.Equals(Bounds);
        }

        protected override void DisposeManagedResources()
        {
            if (_element != null)
            {
                _element.Dispose();
                _element = null;
            }

            base.DisposeManagedResources();
        }

        private void CheckDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        private void LazySetName()
        {
            if (_name != null)
                return;

            string name;
            _element.TryGetPropertyValue(AutomationElement.NameProperty, _useCache, out name);
            _name = name ?? string.Empty;
        }

        private void LazySetClassName()
        {
            if (_className != null)
                return;

            string className;
            _element.TryGetPropertyValue(AutomationElement.ClassNameProperty, _useCache, out className);
            _className = className ?? string.Empty;
        }

        private void LazySetRootWindowClassName()
        {
            if (_rootWindowClassName != null)
                return;

            _rootWindowClassName = WindowInformationUtils.GetClassName(RootWindowHandle);
        }

        private void LazySetAriaRole()
        {
            if (_ariaRole != null)
                return;

            string ariaRole;
            _element.TryGetPropertyValue(AutomationElement.AriaRoleProperty, false, out ariaRole);
            _ariaRole = ariaRole ?? string.Empty;
        }

        private void LazySetBounds()
        {
            if (_bounds.HasValue)
                return;

            Rect bounds;
            if (_element.TryGetPropertyValue(AutomationElement.BoundingRectangleProperty, _useCache, out bounds))
                _bounds = new Rectangle(bounds);
            else
                _bounds = Rectangle.Empty;
        }

        private void LazySetVisibleBounds()
        {
            if (_visibleBounds.HasValue)
                return;

            var visibleBounds = Bounds;
            if (!visibleBounds.IsEmpty && !Parent.IsRoot)
                visibleBounds.Intersection(Parent.VisibleBounds);

            _visibleBounds = visibleBounds;
        }

        private void LazySetAutomationId()
        {
            if (_automationId != null)
                return;

            string automationId;
            _element.TryGetPropertyValue(AutomationElement.AutomationIdProperty, _useCache, out automationId);
            _automationId = automationId ?? string.Empty;
        }

        private void LazySetFrameworkId()
        {
            if (_frameworkId != null)
                return;

            string frameworkId;
            _element.TryGetPropertyValue(AutomationElement.FrameworkIdProperty, _useCache, out frameworkId);
            _frameworkId = frameworkId ?? string.Empty;
        }

        private void LazySetControlType()
        {
            if (_controlType.HasValue)
                return;

            ControlType type;
            if (_element.TryGetPropertyValue(AutomationElement.ControlTypeProperty, _useCache, out type))
                _controlType = (ControlTypeId)type.Id;
            else
                _controlType = ControlTypeId.None;
        }

        private void LazySetIsScrollPatternAvailable()
        {
            if (_isScrollPatternAvailable.HasValue)
                return;

            ScrollPattern pattern;
            if (!_element.TryGetPattern(ScrollPattern.Pattern, false, out pattern))
            {
                _isScrollPatternAvailable = false;
                return;
            }

            if (pattern == null)
            {
                _isScrollPatternAvailable = false;
                return;
            }

            using (pattern)
            {
                _isScrollPatternAvailable = true;
                bool horizontal;
                bool vertical;
                ScrollPattern.ScrollPatternInformation patternInfo = pattern.Current;

                try
                {
                    horizontal = patternInfo.HorizontallyScrollable;
                    vertical = patternInfo.VerticallyScrollable;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("LazySetIsScrollPatternAvailable exception: " + e);
                    horizontal = false;
                    vertical = false;
                }

                try
                {
                    _scrollPercentageHorizontal = Math.Round(patternInfo.HorizontalScrollPercent, 1, MidpointRounding.AwayFromZero);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("LazySetIsScrollPatternAvailable exception: " + e);
                    _scrollPercentageHorizontal = -1d;
                }

                try
                {
                    _scrollPercentageVertical = Math.Round(patternInfo.VerticalScrollPercent, 1, MidpointRounding.AwayFromZero);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("LazySetIsScrollPatternAvailable exception: " + e);
                    _scrollPercentageVertical = -1d;
                }

                if (horizontal && vertical)
                    _availableScrollDirection = AvailableScrollDirections.HorizontalAndVertical;
                else if (horizontal)
                    _availableScrollDirection = AvailableScrollDirections.Horizontal;
                else if (vertical)
                    _availableScrollDirection = AvailableScrollDirections.Vertical;
                else
                    _availableScrollDirection = AvailableScrollDirections.None;
            }
        }

        private void LazySetIsInvokePatternAvailable()
        {
            if (_isInvokePatternAvailable.HasValue)
                return;

            InvokePattern pattern;
            _isInvokePatternAvailable = _element.TryGetPattern(InvokePattern.Pattern, _useCache, out pattern) && pattern != null;

            if (pattern != null)
            {
                pattern.Dispose();
            }
        }

        private void LazySetIsTogglePatternAvailable()
        {
            if (_isTogglePatternAvailable.HasValue)
                return;

            TogglePattern pattern;
            _isTogglePatternAvailable = _element.TryGetPattern(TogglePattern.Pattern, _useCache, out pattern) && pattern != null;

            if (pattern != null)
            {
                pattern.Dispose();
            }
        }

        private void LazySetIsSelectionPatternAvailable()
        {
            if (_isSelectionPatternAvailable.HasValue)
                return;

            SelectionPattern pattern;
            _isSelectionPatternAvailable = _element.TryGetPattern(SelectionPattern.Pattern, _useCache, out pattern) && pattern != null;

            if (pattern != null)
            {
                pattern.Dispose();
            }
        }

        private void LazySetIsSelectionItemPatternAvailable()
        {
            if (_isSelectionItemPatternAvailable.HasValue)
                return;

            SelectionItemPattern pattern;
            _isSelectionItemPatternAvailable = _element.TryGetPattern(SelectionItemPattern.Pattern, _useCache, out pattern) && pattern != null;

            if (pattern != null)
            {
                pattern.Dispose();
            }
        }

        private void LazySetIsExpandCollapsePatternAvailable()
        {
            if (_isExpandCollapseItemPatternAvailable.HasValue)
                return;

            ExpandCollapsePattern pattern;
            _isExpandCollapseItemPatternAvailable = _element.TryGetPattern(ExpandCollapsePattern.Pattern, _useCache, out pattern) && pattern != null;
            if (pattern != null)
            {
                using (pattern)
                {
                    var expandCollapseState = _useCache
                        ? pattern.Cached.ExpandCollapseState
                        : pattern.Current.ExpandCollapseState;
                    _expandCollapseState = ExpandCollapseStateUtils.ConvertFromAutomation(expandCollapseState);
                }
            }
        }

        private void LazySetIsKeyboardFocusable()
        {
            if (_isKeyboardFocusable.HasValue)
                return;

            bool isKeyboardFocusable;
            if (_element.TryGetPropertyValue(AutomationElement.IsKeyboardFocusableProperty, _useCache, out isKeyboardFocusable))
                _isKeyboardFocusable = isKeyboardFocusable;
            else
                _isKeyboardFocusable = false;
        }

        private void LazySetIsOffscreen()
        {
            if (_isOffscreen.HasValue)
                return;

            // not using cache, because of strange exceptions thrown for this property
            bool isOffscreen;
            if (_element.TryGetPropertyValue(AutomationElement.IsOffscreenProperty, false, out isOffscreen))
                _isOffscreen = isOffscreen;
            else
                _isOffscreen = false;
        }

        private void LazySetHasKeyboardFocus()
        {
            if (_hasKeyboardFocus.HasValue)
                return;

            // not using cache, because of strange exceptions thrown for this property
            bool hasKeyboardFocus;
            if (_element.TryGetPropertyValue(AutomationElement.HasKeyboardFocusProperty, false, out hasKeyboardFocus))
                _hasKeyboardFocus = hasKeyboardFocus;
            else
                _hasKeyboardFocus = false;
        }

        private void LazySetChildrenCount()
        {
            if (_childrenCount.HasValue)
                return;

            var children = _element.AutomationElement.FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition);
            if (children != null)
            {
                using (children)
                {
                    _childrenCount = children.Count;
                }
            }
            else
            {
                _childrenCount = 0;
            }
        }

        private void LazySetHasChildren()
        {
            if (_hasChildren.HasValue)
                return;

            if (_children.Count != 0)
            {
                _hasChildren = true;
                return;
            }

            _hasChildren = ChildrenCount != 0;
        }
    }
}