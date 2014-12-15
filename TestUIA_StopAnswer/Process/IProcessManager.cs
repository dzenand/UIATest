namespace TestUIA.Process
{
    public interface IProcessManager
    {
        bool TryGetProcess(string processId, out IProcess process);

        bool TryGetProcess(int processId, out IProcess process);

        void Clear();
    }
}