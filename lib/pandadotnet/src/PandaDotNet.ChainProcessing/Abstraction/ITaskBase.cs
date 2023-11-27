namespace PandaDotNet.ChainProcessing.Abstraction
{
    /// <summary>
    /// This interface is a common base for all variants of tasks.
    /// </summary>
    /// <typeparam name="TPayload">The payload which is being processed</typeparam>
    public interface ITaskBase<in TPayload>
    {
        /// <summary>
        /// Returns true, if this task can run.
        /// If this method returns false, the task may be skipped.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        bool CanRun(TPayload payload);
    }
}