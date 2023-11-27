using System;
using PandaDotNet.ChainProcessing.Abstraction;

namespace PandaDotNet.ChainProcessing
{
    /// <summary>
    /// An object that contains event data from a <see cref="DefaultTaskChainProcessor{TPayload}"/>.
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public class DefaultTaskChainProcessorEventArgs<TPayload> : EventArgs
    {
        /// <summary>
        /// Creates a new <see cref="DefaultTaskChainProcessorEventArgs{TPayload}"/> object.
        /// </summary>
        /// <param name="chain"></param>
        /// <param name="currentTask"></param>
        /// <param name="payload"></param>
        /// <param name="transactionId"></param>
        public DefaultTaskChainProcessorEventArgs(
            ITaskChain<TPayload> chain,
            ITaskBase<TPayload> currentTask,
            TPayload payload,
            string transactionId)
        {
            Chain = chain;
            CurrentTask = currentTask;
            Payload = payload;
            TransactionId = transactionId;
        }

        /// <summary>
        /// Creates a new <see cref="DefaultTaskChainProcessorEventArgs{TPayload}"/> object
        /// with a log message
        /// </summary>
        /// <param name="chain"></param>
        /// <param name="currentTask"></param>
        /// <param name="payload"></param>
        /// <param name="transactionId"></param>
        /// <param name="message"></param>
        public DefaultTaskChainProcessorEventArgs(
            ITaskChain<TPayload> chain,
            ITaskBase<TPayload> currentTask,
            TPayload payload,
            string transactionId,
            string message)
            : this(chain, currentTask, payload, transactionId)
        {
            Message = message;
        }

        /// <summary>
        /// The chain which is currently used to process data
        /// </summary>
        public ITaskChain<TPayload> Chain { get; }
        /// <summary>
        /// The current task to be run. This may return null depending on the context.
        /// </summary>
        public ITaskBase<TPayload> CurrentTask { get; }
        /// <summary>
        /// The payload in its current state being processed
        /// </summary>
        public TPayload Payload { get; }
        /// <summary>
        /// An identifier used to correlate multiple events
        /// </summary>
        public string TransactionId { get; }
        /// <summary>
        /// An optional log message to report errors or warnings
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Creates a new object with a custom log message.
        /// </summary>
        /// <param name="logMessage"></param>
        /// <returns></returns>
        public DefaultTaskChainProcessorEventArgs<TPayload> WithLogMessage(
            string logMessage)
        {
            return new DefaultTaskChainProcessorEventArgs<TPayload>(
                Chain, CurrentTask, Payload, TransactionId, logMessage);
        }
    }
}