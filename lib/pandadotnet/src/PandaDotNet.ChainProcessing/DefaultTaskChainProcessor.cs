using System;
using PandaDotNet.ChainProcessing.Abstraction;
using PandaDotNet.Utils;

namespace PandaDotNet.ChainProcessing
{
    /// <summary>
    /// A default implementation of <see cref="ITaskChainProcessor{TPayload}"/>.
    /// You will have to supply it with your own task chain to use it.
    /// <para>
    /// This chain processor also provides you with events to subscribe to for logging purposes.
    /// </para>
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public class DefaultTaskChainProcessor<TPayload>: ITaskChainProcessor<TPayload>
    {
        /// <summary>
        /// The chain to be run
        /// </summary>
        protected readonly ITaskChain<TPayload> _chain;

        /// <summary>
        /// Instantiates a new <see cref="DefaultTaskChainProcessor{TPayload}"/> with the given
        /// chain to process.
        /// </summary>
        /// <param name="chain"></param>
        public DefaultTaskChainProcessor(ITaskChain<TPayload> chain)
        {
            _chain = chain;
        }
        
        /// <summary>
        /// This event is raised when a task is being skipped because <see cref="ITask{TPayload}.CanRun"/>
        /// has returned false.
        /// </summary>
        /// <seealso cref="ITask{TPayload}.CanRun"/>
        public event DefaultTaskChainProcessorEventDelegate<TPayload> OnTaskSkipped;

        /// <summary>
        /// This event is raised before a task is run but after it is determined that it can run.
        /// </summary>
        public event DefaultTaskChainProcessorEventDelegate<TPayload> OnBeforeTaskStarted; 

        /// <summary>
        /// This event is raised when a task has completed successfully and does not abort the chain.
        /// </summary>
        public event DefaultTaskChainProcessorEventDelegate<TPayload> OnTaskCompleted;

        /// <summary>
        /// This event is raised when a task has completed processing and returns false to abort the chain.
        /// </summary>
        public event DefaultTaskChainProcessorEventDelegate<TPayload> OnChainAborted;

        /// <summary>
        /// This event is raised when all tasks have been completed or skipped without the chain being aborted.
        /// <para>
        /// Note that no task will be provided in the <see cref="DefaultTaskChainProcessorEventArgs{TPayload}"/>
        /// object.
        /// </para>
        /// </summary>
        public event DefaultTaskChainProcessorEventDelegate<TPayload> OnChainCompleted;

        /// <summary>
        /// This event is raised when the task chain processor wants to log a message
        /// </summary>
        public event DefaultTaskChainProcessorEventDelegate<TPayload> OnLogMessageReported; 

        /// <summary>
        /// Generates a new transaction ID to be provided to events.
        /// In its default implementation, it will use <see cref="Guid.NewGuid"/> to generate such an ID
        /// but this may be overwritten to suit your needs. The <see cref="TPayload"/> instance to be processed
        /// is provided.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected virtual string GenerateTransactionId(TPayload payload)
        {
            return Guid.NewGuid().ToString();
        }

        /// <inheritdoc />
        public virtual bool Process(TPayload payload)
        {
            string transactionId = GenerateTransactionId(payload);

            foreach (ITaskBase<TPayload> taskBase in _chain.GetTasks())
            {
                var eventArgs = new DefaultTaskChainProcessorEventArgs<TPayload>(_chain, taskBase, payload, transactionId);
                if (taskBase.CanRun(payload))
                {
                    InvokeOnBeforeTaskStarted(eventArgs);
                    bool canContinue = false;
                    if (taskBase is ITask<TPayload> task)
                    {
                        canContinue = task.Run(payload);
                    } else if (taskBase is IAsyncTask<TPayload> asyncTask)
                    {
                        canContinue = UtilityExtensions.RunSync(() => asyncTask.Run(payload));
                    }
                    else
                    {
                        InvokeOnLogMessageSent(eventArgs,
                            "Task could not be executed because it did not implement a compatible type.");
                    }

                    if (!canContinue)
                    {
                        InvokeOnChainAborted(eventArgs);
                        return false;
                    }
                    InvokeOnTaskCompleted(eventArgs);
                }
                else
                {
                    InvokeOnTaskSkipped(eventArgs);
                }
            }

            InvokeOnChainCompleted(new DefaultTaskChainProcessorEventArgs<TPayload>(
                _chain, null, payload, transactionId));
            return true;
        }

        /// <summary>
        /// Invokes <see cref="OnBeforeTaskStarted"/>
        /// </summary>
        /// <param name="eventArgs"></param>
        protected void InvokeOnBeforeTaskStarted(DefaultTaskChainProcessorEventArgs<TPayload> eventArgs)
            => OnBeforeTaskStarted?.Invoke(this, eventArgs);
        /// <summary>
        /// Invokes <see cref="OnChainAborted"/>
        /// </summary>
        /// <param name="eventArgs"></param>
        protected void InvokeOnChainAborted(DefaultTaskChainProcessorEventArgs<TPayload> eventArgs)
            => OnChainAborted?.Invoke(this, eventArgs);
        /// <summary>
        /// Invokes <see cref="OnTaskCompleted"/>
        /// </summary>
        /// <param name="eventArgs"></param>
        protected void InvokeOnTaskCompleted(DefaultTaskChainProcessorEventArgs<TPayload> eventArgs)
            => OnTaskCompleted?.Invoke(this, eventArgs);
        /// <summary>
        /// Invokes <see cref="OnChainCompleted"/>
        /// </summary>
        /// <param name="eventArgs"></param>
        protected void InvokeOnChainCompleted(DefaultTaskChainProcessorEventArgs<TPayload> eventArgs)
            => OnChainCompleted?.Invoke(this, eventArgs);
        /// <summary>
        /// Invokes <see cref="OnTaskSkipped"/>
        /// </summary>
        /// <param name="eventArgs"></param>
        protected void InvokeOnTaskSkipped(DefaultTaskChainProcessorEventArgs<TPayload> eventArgs)
            => OnTaskSkipped?.Invoke(this, eventArgs);

        /// <summary>
        /// Invokes <see cref="OnLogMessageReported"/>
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <param name="logMessage">Log message</param>
        protected void InvokeOnLogMessageSent(
            DefaultTaskChainProcessorEventArgs<TPayload> eventArgs,
            string logMessage)
            => OnLogMessageReported?.Invoke(this, eventArgs.WithLogMessage(logMessage));
    }
}