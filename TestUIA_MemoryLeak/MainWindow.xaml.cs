using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TestUIA.Automation;
using TestUIA.Cache;
using TestUIA.Common;

namespace TestUIA
{
    public partial class MainWindow : Window
    {
        private const int PollFrequency = 30;

        private readonly IncrementalProcessAutomationCacheLocator _incrementalProcessAutomationCacheLocator;
        private readonly ShotgunPatternGenerator _shotgunPatternGenerator;
        private Timer _timer;

        private bool _isProcessingQuery;
        private Point? _storedPoint;
        private readonly object _queryLock = new object();

        public MainWindow()
        {
            InitializeComponent();
            ConsoleHelper.OpenConsole();

            DataContext = this;

            _shotgunPatternGenerator = new ShotgunPatternGenerator(4);
            _incrementalProcessAutomationCacheLocator = new IncrementalProcessAutomationCacheLocator();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (_timer == null)
                _timer = new Timer(TimerPoll, null, TimeSpan.FromSeconds(1.0 / (double)PollFrequency), TimeSpan.FromSeconds(1.0 / (double)PollFrequency));
        }

        private void TimerPoll(object state)
        {
            try
            {
                var mousePos = MouseHelper.GetCurrentMousePosition();

                HandlePoint(mousePos);
            }
            catch
            {
            }
        }

        private async Task HandlePoint(Point point)
        {
            lock (_queryLock)
            {
                if (_isProcessingQuery)
                {
                    _storedPoint = point;
                    return;
                }

                _isProcessingQuery = true;
            }

            await Task.Run(() => ProcessPoint(point));

            lock (_queryLock)
            {
                _isProcessingQuery = false;

                if (_storedPoint != null)
                {
                    var storedPoint = _storedPoint.Value;
                    _storedPoint = null;
                    HandlePoint(storedPoint);
                }
            }
        }

        private void ProcessPoint(Point point)
        {
            var sw = new Stopwatch();

            sw.Start();

            try
            {
                var windowHandle = NativeWindowUtils.DesktopChildWindowFromPoint(point);
                uint processId = 0;
                User32.GetWindowThreadProcessId(windowHandle, out processId);

                Console.WriteLine("Mouse point = {0}, processId = {1}", point, processId);

                var cache = _incrementalProcessAutomationCacheLocator.GetForProcess((int)processId);
                ProcessPoint(cache, point, windowHandle);
            }
            catch (Exception)
            {
            }

            sw.Stop();

            Console.WriteLine("Stopwatch ProcessPoint = {0}\r\n", sw.Elapsed);
        }

        private void ProcessPoint(IncrementalProcessAutomationCache incrementalProcessAutomationCache, Point point, IntPtr windowHandle)
        {
            var rectangle = new Rectangle(point.X - 100, point.Y - 100, 200, 200);
            var points = _shotgunPatternGenerator.NextPattern(rectangle);

            foreach (var shotgunPoint in points)
            {
                Console.WriteLine("Shotgun point = {0}", shotgunPoint);
                var automationElementData = incrementalProcessAutomationCache.FindFromPoint(shotgunPoint, windowHandle);

                if ((automationElementData != null) && automationElementData.ControlType == ControlTypeId.Window)
                {
                    Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAA");
                }
                else
                {
                    ProcessAutomationElementData(automationElementData);
                }
            }
        }

        private void ProcessAutomationElementData(IAutomationElementData automationElementData)
        {
            if (automationElementData == null || automationElementData.IsRoot)
                return;

            var automationId = automationElementData.AutomationId;
            var className = automationElementData.ClassName;
            var controlType = automationElementData.ControlType;
            var expandCollapseState = automationElementData.ExpandCollapseState;
            var scrollPercent = automationElementData.ScrollPercent;
            var isTogglePatternAvailable = automationElementData.IsTogglePatternAvailable;
            var isScrollPatternAvailable = automationElementData.IsScrollPatternAvailable;
            var name = automationElementData.Name;
            var availableScrollDirection = automationElementData.AvailableScrollDirection;
            var bounds = automationElementData.Bounds;
            var visibleBounds = automationElementData.VisibleBounds;

            var sb = new StringBuilder();
            sb.Append("automationElementData = ");
            sb.AppendLine(automationElementData.ToString());
            sb.Append("automationId = ");
            sb.AppendLine(automationId);
            sb.Append("className = ");
            sb.AppendLine(className);
            sb.Append("controlType = ");
            sb.AppendLine(controlType.ToString());
            sb.Append("expandCollapseState = ");
            sb.AppendLine(expandCollapseState.ToString());
            sb.Append("scrollPercent = ");
            sb.AppendLine(scrollPercent.ToString());
            sb.Append("isTogglePatternAvailable = ");
            sb.AppendLine(isTogglePatternAvailable.ToString());
            sb.Append("isScrollPatternAvailable = ");
            sb.AppendLine(isScrollPatternAvailable.ToString());
            sb.Append("name = ");
            sb.AppendLine(name);
            sb.Append("availableScrollDirection = ");
            sb.AppendLine(availableScrollDirection.ToString());
            sb.Append("bounds = ");
            sb.AppendLine(bounds.ToString());
            sb.Append("visibleBounds = ");
            sb.AppendLine(visibleBounds.ToString());
            sb.AppendLine("==");

            Console.Write(sb);

            ProcessAutomationElementData(automationElementData.Parent);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
                _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }
}
