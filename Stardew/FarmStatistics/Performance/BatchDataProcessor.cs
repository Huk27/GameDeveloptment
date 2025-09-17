using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace FarmStatistics.Performance
{
    /// <summary>
    /// Automate 패턴을 적용한 배치 처리 시스템 (Phase 3.1)
    /// 대량의 데이터 수집 작업을 효율적으로 처리합니다.
    /// </summary>
    public class BatchDataProcessor : IDisposable
    {
        #region Fields

        private readonly IMonitor _monitor;
        private readonly ConcurrentQueue<DataCollectionTask> _pendingTasks = new();
        private readonly Timer _batchTimer;
        private readonly object _processingLock = new();
        
        // 성능 설정
        private const int MaxBatchSize = 50;
        private const int BatchIntervalMs = 1000; // 1초마다 배치 처리
        private const int MaxProcessingTimeMs = 100; // 최대 100ms 처리 시간
        
        // 통계
        private int _totalTasksProcessed = 0;
        private int _totalBatchesProcessed = 0;
        private DateTime _lastProcessingTime = DateTime.Now;
        
        #endregion

        #region Constructor

        public BatchDataProcessor(IMonitor monitor)
        {
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            
            // 배치 처리 타이머 시작
            _batchTimer = new Timer(ProcessBatchCallback, null, 0, BatchIntervalMs);
            
            _monitor.Log("배치 데이터 처리기 초기화 완료", LogLevel.Debug);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 데이터 수집 작업을 큐에 추가합니다.
        /// </summary>
        public void EnqueueTask(DataCollectionTask task)
        {
            try
            {
                if (task == null)
                {
                    _monitor?.Log("null 작업은 큐에 추가할 수 없습니다", LogLevel.Debug);
                    return;
                }

                _pendingTasks.Enqueue(task);
                _monitor?.Log($"작업 큐에 추가: {task.TaskType} (대기 중: {_pendingTasks.Count})", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"작업 큐 추가 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 특정 타입의 작업들을 일괄 추가합니다.
        /// </summary>
        public void EnqueueTasks(IEnumerable<DataCollectionTask> tasks)
        {
            try
            {
                if (tasks == null) return;

                var taskList = tasks.ToList();
                foreach (var task in taskList)
                {
                    if (task != null)
                        _pendingTasks.Enqueue(task);
                }

                _monitor?.Log($"배치 작업 추가: {taskList.Count}개 (총 대기: {_pendingTasks.Count})", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"배치 작업 추가 중 오류: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 현재 대기 중인 작업 수를 반환합니다.
        /// </summary>
        public int GetPendingTaskCount() => _pendingTasks.Count;

        /// <summary>
        /// 처리 통계를 반환합니다.
        /// </summary>
        public BatchProcessingStats GetStats()
        {
            return new BatchProcessingStats
            {
                TotalTasksProcessed = _totalTasksProcessed,
                TotalBatchesProcessed = _totalBatchesProcessed,
                PendingTasks = _pendingTasks.Count,
                LastProcessingTime = _lastProcessingTime,
                AverageTasksPerBatch = _totalBatchesProcessed > 0 ? 
                    (double)_totalTasksProcessed / _totalBatchesProcessed : 0
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 타이머 콜백 - 배치 처리 실행
        /// </summary>
        private void ProcessBatchCallback(object state)
        {
            try
            {
                ProcessBatch();
            }
            catch (Exception ex)
            {
                _monitor?.Log($"배치 처리 콜백 오류: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 배치 처리 메인 로직 (Automate 패턴)
        /// </summary>
        private void ProcessBatch()
        {
            if (!Monitor.TryEnter(_processingLock, TimeSpan.FromMilliseconds(10)))
            {
                _monitor?.Log("이전 배치 처리가 진행 중이므로 건너뜀", LogLevel.Trace);
                return;
            }

            try
            {
                if (_pendingTasks.IsEmpty)
                {
                    return; // 처리할 작업이 없음
                }

                var startTime = DateTime.Now;
                var processed = 0;
                var results = new List<DataCollectionResult>();

                // 최대 배치 크기와 처리 시간 제한
                while (_pendingTasks.TryDequeue(out var task) && 
                       processed < MaxBatchSize && 
                       (DateTime.Now - startTime).TotalMilliseconds < MaxProcessingTimeMs)
                {
                    try
                    {
                        var result = ProcessSingleTask(task);
                        if (result != null)
                        {
                            results.Add(result);
                        }
                        processed++;
                    }
                    catch (Exception ex)
                    {
                        _monitor?.Log($"단일 작업 처리 중 오류 ({task.TaskType}): {ex.Message}", LogLevel.Debug);
                    }
                }

                // 통계 업데이트
                _totalTasksProcessed += processed;
                _totalBatchesProcessed++;
                _lastProcessingTime = DateTime.Now;

                if (processed > 0)
                {
                    var processingTime = (DateTime.Now - startTime).TotalMilliseconds;
                    _monitor?.Log($"배치 처리 완료: {processed}개 작업, {processingTime:F1}ms, 남은 작업: {_pendingTasks.Count}", LogLevel.Trace);

                    // 결과 처리 이벤트 발생
                    OnBatchProcessed?.Invoke(new BatchProcessedEventArgs
                    {
                        ProcessedTasks = processed,
                        Results = results,
                        ProcessingTimeMs = processingTime,
                        RemainingTasks = _pendingTasks.Count
                    });
                }
            }
            finally
            {
                Monitor.Exit(_processingLock);
            }
        }

        /// <summary>
        /// 단일 작업 처리 (작업 타입별 분기)
        /// </summary>
        private DataCollectionResult ProcessSingleTask(DataCollectionTask task)
        {
            try
            {
                var startTime = DateTime.Now;
                DataCollectionResult result = null;

                switch (task.TaskType)
                {
                    case DataCollectionTaskType.CropData:
                        result = ProcessCropDataTask(task);
                        break;
                        
                    case DataCollectionTaskType.AnimalData:
                        result = ProcessAnimalDataTask(task);
                        break;
                        
                    case DataCollectionTaskType.OverviewData:
                        result = ProcessOverviewDataTask(task);
                        break;
                        
                    case DataCollectionTaskType.TimeData:
                        result = ProcessTimeDataTask(task);
                        break;
                        
                    default:
                        _monitor?.Log($"알 수 없는 작업 타입: {task.TaskType}", LogLevel.Debug);
                        break;
                }

                if (result != null)
                {
                    result.ProcessingTimeMs = (DateTime.Now - startTime).TotalMilliseconds;
                    result.Timestamp = DateTime.Now;
                }

                return result;
            }
            catch (Exception ex)
            {
                _monitor?.Log($"작업 처리 중 오류 ({task.TaskType}): {ex.Message}", LogLevel.Debug);
                return new DataCollectionResult
                {
                    TaskType = task.TaskType,
                    Success = false,
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.Now
                };
            }
        }

        /// <summary>
        /// 작물 데이터 수집 작업 처리
        /// </summary>
        private DataCollectionResult ProcessCropDataTask(DataCollectionTask task)
        {
            // 실제 작물 데이터 수집 로직은 GameDataCollector에서 처리
            // 여기서는 배치 처리 최적화만 담당
            
            return new DataCollectionResult
            {
                TaskType = task.TaskType,
                Success = true,
                Data = task.Execute(), // 작업 실행
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 동물 데이터 수집 작업 처리
        /// </summary>
        private DataCollectionResult ProcessAnimalDataTask(DataCollectionTask task)
        {
            return new DataCollectionResult
            {
                TaskType = task.TaskType,
                Success = true,
                Data = task.Execute(),
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 개요 데이터 수집 작업 처리
        /// </summary>
        private DataCollectionResult ProcessOverviewDataTask(DataCollectionTask task)
        {
            return new DataCollectionResult
            {
                TaskType = task.TaskType,
                Success = true,
                Data = task.Execute(),
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 시간 데이터 수집 작업 처리
        /// </summary>
        private DataCollectionResult ProcessTimeDataTask(DataCollectionTask task)
        {
            return new DataCollectionResult
            {
                TaskType = task.TaskType,
                Success = true,
                Data = task.Execute(),
                Timestamp = DateTime.Now
            };
        }

        #endregion

        #region Events

        /// <summary>
        /// 배치 처리 완료 시 발생하는 이벤트
        /// </summary>
        public event Action<BatchProcessedEventArgs> OnBatchProcessed;

        #endregion

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            if (!_disposed)
            {
                _batchTimer?.Dispose();
                
                // 남은 작업들 정리
                while (_pendingTasks.TryDequeue(out _)) { }
                
                _monitor?.Log($"배치 데이터 처리기 종료 - 총 처리: {_totalTasksProcessed}개 작업, {_totalBatchesProcessed}개 배치", LogLevel.Info);
                
                _disposed = true;
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// 데이터 수집 작업 정의
    /// </summary>
    public class DataCollectionTask
    {
        public DataCollectionTaskType TaskType { get; set; }
        public string TaskId { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Func<object> Execute { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    /// <summary>
    /// 데이터 수집 작업 타입
    /// </summary>
    public enum DataCollectionTaskType
    {
        CropData,
        AnimalData,
        OverviewData,
        TimeData,
        GoalData
    }

    /// <summary>
    /// 데이터 수집 결과
    /// </summary>
    public class DataCollectionResult
    {
        public DataCollectionTaskType TaskType { get; set; }
        public bool Success { get; set; }
        public object Data { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
        public double ProcessingTimeMs { get; set; }
    }

    /// <summary>
    /// 배치 처리 완료 이벤트 인자
    /// </summary>
    public class BatchProcessedEventArgs
    {
        public int ProcessedTasks { get; set; }
        public List<DataCollectionResult> Results { get; set; } = new();
        public double ProcessingTimeMs { get; set; }
        public int RemainingTasks { get; set; }
    }

    /// <summary>
    /// 배치 처리 통계
    /// </summary>
    public class BatchProcessingStats
    {
        public int TotalTasksProcessed { get; set; }
        public int TotalBatchesProcessed { get; set; }
        public int PendingTasks { get; set; }
        public DateTime LastProcessingTime { get; set; }
        public double AverageTasksPerBatch { get; set; }
    }

    #endregion
}
