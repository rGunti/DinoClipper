namespace PandaDotNet.ChainProcessing.Abstraction
{
    /// <summary>
    /// This interface describes a single task in a chain.
    /// <para>
    /// Tasks are meant to be stateless. <see cref="TPayload"/> should be stateful since
    /// this object will be passed through the task chain.
    /// </para>
    /// </summary>
    /// <typeparam name="TPayload">The payload which is being processed</typeparam>
    /// <seealso cref="ITaskBase{TPayload}"/>
    public interface ITask<in TPayload> : ITaskBase<TPayload>
    {
        /// <summary>
        /// Runs the task and returns a boolean indicating if processing was successful.
        /// The task may return false to stop the chain from processing further.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        bool Run(TPayload payload);
    }
}