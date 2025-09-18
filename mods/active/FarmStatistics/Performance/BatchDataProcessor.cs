using StardewModdingAPI;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace FarmStatistics.Performance
{
    public class BatchDataProcessor
    {
        private readonly IMonitor _monitor;
        private readonly ConcurrentQueue<Action> _taskQueue = new();
        private readonly Timer _batchTimer;

        public event Action? OnBatchProcessed;

        public BatchDataProcessor(IMonitor monitor)
        {
            _monitor = monitor;
            _batchTimer = new Timer(ProcessBatch, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        public void EnqueueTask(Action task)
        {
            _taskQueue.Enqueue(task);
        }

        private void ProcessBatch(object? state)
        {
            if (_taskQueue.IsEmpty)
                return;
                
            _monitor.Log($"Processing batch of {_taskQueue.Count} tasks.", LogLevel.Trace);

            while (_taskQueue.TryDequeue(out var task))
            {
                try
                {
                    task.Invoke();
                }
                catch (Exception ex)
                {
                    _monitor.Log($"Error processing a task in the batch: {ex.Message}", LogLevel.Error);
                }
            }

            OnBatchProcessed?.Invoke();
        }
    }
}
