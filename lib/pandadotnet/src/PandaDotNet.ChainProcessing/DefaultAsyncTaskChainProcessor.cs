using System.Threading.Tasks;
using PandaDotNet.ChainProcessing.Abstraction;

namespace PandaDotNet.ChainProcessing
{
    /// <summary>
    /// An asynchronous implementation of <see cref="DefaultTaskChainProcessor{TPayload}"/>
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public class DefaultAsyncTaskChainProcessor<TPayload> :
        DefaultTaskChainProcessor<TPayload>,
        IAsyncTaskChainProcessor<TPayload>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chain"></param>
        public DefaultAsyncTaskChainProcessor(ITaskChain<TPayload> chain)
            : base(chain)
        {
        }

        /// <inheritdoc />
        public new async virtual Task<bool> Process(TPayload payload)
        {
            string transactionId = GenerateTransactionId(payload);

            foreach (ITaskBase<TPayload> task in _chain.GetTasks())
            {
                var eventArgs = new DefaultTaskChainProcessorEventArgs<TPayload>(_chain, task, payload, transactionId);
                if (task.CanRun(payload))
                {
                    InvokeOnBeforeTaskStarted(eventArgs);

                    bool canContinue = false;
                    if (task is IAsyncTask<TPayload> asyncTask)
                    {
                        canContinue = await asyncTask.Run(payload);
                    } else if (task is ITask<TPayload> regularTask)
                    {
                        canContinue = regularTask.Run(payload);
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
    }
}