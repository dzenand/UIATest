using System;

namespace TestUIA.Automation
{
    [Flags]
    public enum AvailableScrollDirections
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        HorizontalAndVertical = Horizontal | Vertical
    }
}