using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PandaDotNet.ChainProcessing;
using PandaDotNet.ChainProcessing.Abstraction;

namespace PandaDotNet.Tests.ChainProcessing
{
    public class MakeUpperCaseTask : ITask<DemoChainPayload>
    {
        public bool CanRun(DemoChainPayload payload) => !string.IsNullOrWhiteSpace(payload.Text);

        public bool Run(DemoChainPayload payload)
        {
            payload.Text = payload.Text.ToUpperInvariant();
            return true;
        }
    }

    public class ParseNumberTask : ITask<DemoChainPayload>
    {
        public bool CanRun(DemoChainPayload payload) => !string.IsNullOrWhiteSpace(payload.Text);

        public bool Run(DemoChainPayload payload)
        {
            if (int.TryParse(payload.Text, out int number))
            {
                payload.Number = number;
                return true;
            }
            return false;
        }
    }

    public class WaitAsyncTask : IAsyncTask<DemoChainPayload>
    {
        public bool CanRun(DemoChainPayload payload)
        {
            return true;
        }

        public async Task<bool> Run(DemoChainPayload payload)
        {
            await Task.Delay(1250);
            return true;
        }
    }

    public class DemoChainPayload
    {
        public string Text { get; set; }
        public int? Number { get; set; }
    }

    public class DemoChain : ITaskChain<DemoChainPayload>
    {
        public IEnumerable<ITaskBase<DemoChainPayload>> GetTasks()
        {
            yield return new MakeUpperCaseTask();
            yield return new ParseNumberTask();
        }
    }

    public class AsyncDemoChain : ITaskChain<DemoChainPayload>
    {
        public IEnumerable<ITaskBase<DemoChainPayload>> GetTasks()
        {
            yield return new MakeUpperCaseTask();
            yield return new WaitAsyncTask();
            yield return new ParseNumberTask();
        }
    }

    [TestClass]
    public class ChainProcessingTests
    {
        private DefaultTaskChainProcessor<DemoChainPayload> _chainProcessor;

        public ChainProcessingTests()
        {
        }

        private void CreateProcessor(bool asyncProcessor, bool asyncChain)
        {
            ITaskChain<DemoChainPayload> chain = asyncChain ? new AsyncDemoChain() : new DemoChain();
            _chainProcessor = asyncProcessor
                ? new DefaultAsyncTaskChainProcessor<DemoChainPayload>(chain)
                : new DefaultTaskChainProcessor<DemoChainPayload>(chain);
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _chainProcessor.OnChainAborted += (_, args) =>
                Debug.WriteLine($"TRANS={args.TransactionId} Chain aborted!");
            _chainProcessor.OnChainCompleted += (_, args) =>
                Debug.WriteLine($"TRANS={args.TransactionId} Chain completed!");
            _chainProcessor.OnTaskSkipped += (_, args) =>
                Debug.WriteLine($"TRANS={args.TransactionId} Task {args.CurrentTask} skipped");
            _chainProcessor.OnBeforeTaskStarted += (_, args) =>
                Debug.WriteLine($"TRANS={args.TransactionId} Starting Task {args.CurrentTask} ...");
            _chainProcessor.OnTaskCompleted += (_, args) =>
                Debug.WriteLine($"TRANS={args.TransactionId} Task {args.CurrentTask} completed");
        }

        [TestMethod]
        [DataRow("Hello World", false, "HELLO WORLD", null, false)]
        [DataRow("69420", true, "69420", 69420, false)]
        [DataRow(null, true, null, null, false)]
        [DataRow("Hello World", false, "HELLO WORLD", null, true)]
        [DataRow("69420", true, "69420", 69420, true)]
        [DataRow(null, true, null, null, true)]
        public void DefaultHandlerRunsAsExpected(string input, bool expectToComplete, string expectedOutput, int? expectedNumber,
            bool async)
        {
            CreateProcessor(async, async);
            
            var payload = new DemoChainPayload
            {
                Text = input
            };

            bool completed = _chainProcessor.Process(payload);
            Assert.AreEqual(expectToComplete, completed);
            Assert.AreEqual(expectedOutput, payload.Text);
            Assert.AreEqual(expectedNumber, payload.Number);
        }
    }
}