using System.Threading.Tasks;

namespace PandaDotNet.ChainProcessing.Abstraction
{
    /// <summary>
    /// This interface is an asynchronous version of <see cref="ITask{TPayload}"/>
    /// </summary>
    /// <typeparam name="TPayload">The payload which is being processed</typeparam>
    /// <seealso cref="ITaskBase{TPayload}"/>
    public interface IAsyncTask<in TPayload> : ITaskBase<TPayload>
    {
        /// <summary>
        /// Runs the task and returns a boolean indicating if processing was successful.
        /// The task may return false to stop the chain from processing further.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        Task<bool> Run(TPayload payload);
    }
}