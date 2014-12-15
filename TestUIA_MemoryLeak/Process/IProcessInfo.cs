namespace TestUIA.Process
{
    public interface IProcessInfo
    {
        /// <summary>
        /// Process Id
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Process Name
        /// </summary>
        string Name { get; }
    }
}