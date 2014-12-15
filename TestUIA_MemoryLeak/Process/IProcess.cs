using System;

namespace TestUIA.Process
{
    public interface IProcess : IProcessInfo
    {
        event EventHandler Exited;

        bool EnableRaisingEvents { get; set; }
    }
}