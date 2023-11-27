using System.Collections.Generic;

namespace PandaDotNet.ChainProcessing.Abstraction
{
    /// <summary>
    /// This interface describes a chain of tasks.
    /// </summary>
    /// <typeparam name="TPayload">The payload to be processed</typeparam>
    public interface ITaskChain<in TPayload>
    {
        /// <summary>
        /// Returns a new list of tasks to be run in order.
        /// <para>
        /// It is recommended to write the implementation by chaining "yield return" statements together.
        /// The chain should always re-instantiate new tasks when called to ensure no state is carried over.  
        /// </para>
        /// </summary>
        /// <returns></returns>
        IEnumerable<ITaskBase<TPayload>> GetTasks();
    }
}