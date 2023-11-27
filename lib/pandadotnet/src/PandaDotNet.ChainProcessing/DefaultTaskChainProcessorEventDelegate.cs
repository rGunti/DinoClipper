namespace PandaDotNet.ChainProcessing
{
    /// <summary>
    /// The delegate used when raising events from a <see cref="DefaultTaskChainProcessor{TPayload}"/>.
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public delegate void DefaultTaskChainProcessorEventDelegate<TPayload>(
        DefaultTaskChainProcessor<TPayload> sender,
        DefaultTaskChainProcessorEventArgs<TPayload> eventArgs);
}