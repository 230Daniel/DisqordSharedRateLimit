using System;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Utilities.Threading;
using Microsoft.Extensions.Logging;

namespace DisqordSharedRateLimit.Gateway
{
    internal class SimpleBucket
    {
        public int CurrentCount
            {
                get
                {
                    lock (_semaphore)
                    {
                        return _semaphore.CurrentCount;
                    }
                }
            }

            private readonly ILogger _logger;
            private readonly BetterSemaphoreSlim _semaphore;
            private readonly TimeSpan _resetDelay;
            private bool _isResetting;

            public SimpleBucket(ILogger logger, int uses, TimeSpan resetDelay)
            {
                _logger = logger;
                _semaphore = new BetterSemaphoreSlim(uses, uses);
                _resetDelay = resetDelay;
            }

            public Task WaitAsync(CancellationToken cancellationToken)
            {
                lock (_semaphore)
                {
                    return _semaphore.WaitAsync(cancellationToken);
                }
            }

            public void NotifyCompletion()
            {
                lock (_semaphore)
                {
                    if (!_isResetting)
                    {
                        _isResetting = true;
                        _ = ResetAsync();
                    }
                }
            }

            public void Release()
            {
                lock (_semaphore)
                {
                    try
                    {
                        _semaphore.Release();
                    }
                    catch { }
                }
            }

            private async Task ResetAsync()
            {
                await Task.Delay(_resetDelay).ConfigureAwait(false);
                lock (_semaphore)
                {
                    var releaseCount = _semaphore.MaximumCount - _semaphore.CurrentCount;
                    _logger.Log(releaseCount == 0
                        ? LogLevel.Error
                        : LogLevel.Debug, "Releasing the semaphore by {ReleaseCount}", releaseCount);
                    try
                    {
                        _semaphore.Release(releaseCount);
                    }
                    finally
                    {
                        _isResetting = false;
                    }
                }
            }
    }
}
