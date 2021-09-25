using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DinoClipper.Config;
using DinoClipper.Storage;
using Microsoft.Extensions.Logging;
using PandaDotNet.ChainProcessing.Abstraction;
using PandaDotNet.Time;
using PandaDotNet.Utils;

namespace DinoClipper.Downloader
{
    public interface IDownloaderQueue
    {
        void StartQueue(CancellationToken cancellationToken);
        void StopQueue();
        void QueueClip(Clip clip);
    }

    public class DownloaderQueue : IDownloaderQueue
    {
        private static readonly TimeSpan MaxThreadAge = 5.Minutes();

        class WorkerThread
        {
            public Guid Guid { get; } = Guid.NewGuid();
            public Thread Thread { get; set; }
            public DateTime? StartedAt { get; private set; }
            public TimeSpan? Age => StartedAt != null ? DateTime.UtcNow - StartedAt : null;

            public void Start()
            {
                StartedAt = DateTime.UtcNow;
                Thread?.Start();
            }

            public void Reset()
            {
                StartedAt = null;
                Thread = null;
            }
        }
        
        private readonly ILogger<DownloaderQueue> _logger;
        private readonly DinoClipperConfiguration _config;
        private readonly ITaskChainProcessor<DownloaderChainPayload> _clipProcessor;
        private readonly ConcurrentQueue<Clip> _clipQueue = new();

        private readonly ConcurrentQueue<WorkerThread> _workerThreadPool = new();
        private readonly List<WorkerThread> _wipThreads = new();
        private readonly List<WorkerThread> _graveyard = new();

        private CancellationToken _cancellationToken;
        private Thread _workerThread;

        public DownloaderQueue(
            ILogger<DownloaderQueue> logger,
            DinoClipperConfiguration config,
            ITaskChainProcessor<DownloaderChainPayload> clipProcessor)
        {
            _logger = logger;
            _config = config;
            _clipProcessor = clipProcessor;
            
            FillWorkerThreadPool();
        }

        private void FillWorkerThreadPool()
        {
            while (_workerThreadPool.Count < _config.DownloaderFlags.MaxWorkerThreads)
            {
                var thread = new WorkerThread();
                _logger.LogTrace("Adding thread {ThreadId} to pool", thread.Guid);
                _workerThreadPool.Enqueue(thread);
            }
        }

        private void ReturnThreadToQueue(WorkerThread thread)
        {
            _logger.LogTrace("Returning thread {ThreadId} to pool", thread.Guid);
            while (_wipThreads.Remove(thread)) { }
            thread.Reset();
            _workerThreadPool.Enqueue(thread);
        }

        private void SendThreadToGraveyard(WorkerThread zombie)
        {
            _graveyard.Add(zombie);
            _logger.LogWarning(
                "Sent thread {ThreadId} to graveyard, now having {ZombieThread} zombies",
                zombie.Guid, _graveyard.Count);
            while (_wipThreads.Remove(zombie)) { }
            FillWorkerThreadPool();
        }

        public void QueueClip(Clip clip)
        {
            _clipQueue.Enqueue(clip);
            _logger.LogInformation("Queued clip {ClipId}", clip.Id);
        }

        public void StartQueue(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _workerThread = new Thread(WorkOnQueue);
            _workerThread.Start();
        }

        public void StopQueue()
        {
            if (_workerThread == null)
            {
                _logger.LogDebug("Worker Thread is already gone");
                return;
            }
            
            _logger.LogInformation("Requesting queue worker to stop gracefully");
        }

        private void MoveZombiesToGraveyard()
        {
            List<WorkerThread> zombies = _wipThreads
                .Where(t => t.Age > MaxThreadAge)
                .ToList();
            
            foreach (WorkerThread zombie in zombies)
            {
                _logger.LogWarning("Thread {ThreadId} that's taking longer than expected, moving it to graveyard",
                    zombie.Guid);
                SendThreadToGraveyard(zombie);
            }
        }

        private async Task<WorkerThread> GetWorkerThread()
        {
            _logger.LogTrace("Trying to get a new worker thread ...");
            WorkerThread workerThread;
            while (!_workerThreadPool.TryDequeue(out workerThread))
            {
                _logger.LogTrace("No worker thread available, waiting a moment ...");
                await Task.Delay(1.Second(), _cancellationToken);
                if (_cancellationToken.IsCancellationRequested)
                    return null;
                MoveZombiesToGraveyard();
            }

            return workerThread;
        }

        private async void WorkOnQueue()
        {
            using IDisposable scope = _logger.BeginScope("DownloaderQueue");
            _logger.LogInformation("Started worker thread ...");
            while (true)
            {
                _logger.LogTrace("Checking for new clips ...");
                while (_clipQueue.TryDequeue(out Clip clip))
                {
                    _logger.LogTrace("Dequeued clip {ClipId}, queue size is now at {QueueLength}",
                        clip.Id, _clipQueue.Count);

                    WorkerThread workerThread = await GetWorkerThread();
                    if (workerThread == null || _cancellationToken.IsCancellationRequested)
                        break;

                    _logger.LogDebug("Got worker thread {WorkerThread}, will now use it to start working ...",
                        workerThread.Guid);
                    var thread = new Thread(() =>
                    {
                        using IDisposable context = _logger.BeginScope(workerThread.Guid);
                        WorkOnClip(clip, workerThread);
                        ReturnThreadToQueue(workerThread);
                    });
                    thread.Name = workerThread.Guid.ToString();
                    workerThread.Thread = thread;
                    
                    _wipThreads.Add(workerThread);
                    workerThread.Start();
                }

                if (_cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker thread cancellation requested");
                    break;
                }
                
                _logger.LogTrace("Sleeping for a few seconds ...");
                await Task.Delay(5.Second(), _cancellationToken);
            }
            _logger.LogInformation("Worker thread ended");
        }

        private void WorkOnClip(Clip clip, WorkerThread thread)
        {
            using IDisposable clipScope = _logger.BeginScope(clip.Id);
            _logger.LogInformation("Started working on clip {ClipId}", clip.Id);

            var payload = new DownloaderChainPayload
            {
                Clip = clip
            };
            
            _logger.LogDebug("Preparing working directory ...");
            string workingDirectory = Path.Combine(_config.TempStorage, thread.Guid.ToString());
            if (workingDirectory.EnsureDirectoryExists())
            {
                _logger.LogDebug("Created working directory {WorkingDirectory}",
                    workingDirectory);
            }
            else
            {
                _logger.LogDebug("Clearing working directory {WorkingDirectory}",
                    workingDirectory);
                workingDirectory.ClearDirectory(_logger);
            }
            payload.WorkingDirectory = workingDirectory;

            try
            {
                _logger.LogDebug("Starting clip processor ...");
                DateTime start = DateTime.UtcNow;
                bool response = _clipProcessor.Process(payload);
                DateTime end = DateTime.UtcNow;
                _logger.LogTrace("Clip processor for clip {ClipId} completed, worked on it for {ProcessDuration}",
                     clip.Id, end - start);

                if (response)
                {
                    _logger.LogInformation("Clip {ClipId} has completed processing", clip.Id);
                }
                else
                {
                    _logger.LogWarning("Clip processing was aborted for clip {ClipId}, refer to logs for " +
                                       "further information", clip.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process clip {ClipId} due to an exception in processing!",
                    clip.Id);
            }
        }
    }
}