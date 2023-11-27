using System.Threading.Tasks;

namespace PandaDotNet.ChainProcessing.Abstraction
{
    /// <summary>
    /// This interface describes a service that processes a given <see cref="ITaskChain{TPayload}"/>
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public interface ITaskChainProcessor<in TPayload>
    {
        /// <summary>
        /// Executes the assigned <see cref="ITaskChain{TPayload}"/> with the given <see cref="TPayload"/> object
        /// to be processed.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        bool Process(TPayload payload);
    }

    /// <summary>
    /// An asynchronous extension of <see cref="ITaskChainProcessor{TPayload}"/>.
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public interface IAsyncTaskChainProcessor<in TPayload> : ITaskChainProcessor<TPayload>
    {
        /// <summary>
        /// Executes the assigned <see cref="ITaskChain{TPayload}"/> asynchronously
        /// with the given <see cref="TPayload"/> object to be processed.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        new Task<bool> Process(TPayload payload);
    }
}