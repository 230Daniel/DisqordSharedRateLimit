using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Http;
using Disqord.Logging;
using Disqord.Rest.Api;
using Disqord.Rest.Api.Default;
using DisqordSharedRateLimit.Extensions;
using Microsoft.Extensions.Logging;

namespace DisqordSharedRateLimit.Rest
{
    internal sealed class RequestQueue : ILogging
    {
        public ILogger Logger => _rateLimiter.Logger;
        public Bucket Bucket { get; set; }

        private readonly SharedRestRateLimiter _rateLimiter;
        private readonly object _lock;
        private readonly LinkedList<IRestRequest> _requests;
        private Task _task;

        public RequestQueue(SharedRestRateLimiter rateLimiter, Bucket bucket)
        {
            _rateLimiter = rateLimiter;
            _lock = new();
            _requests = new();
            
            Bucket = bucket;
        }
        
        public void Enqueue(IRestRequest request)
        {
            lock (_lock)
            {
                _requests.AddLast(request);
                if (_task is null || _task.IsCompleted)
                    _task = Task.Run(RunAsync);
            }
        }

        private async Task RunAsync()
        {
            while (_requests.First is not null)
            {
                var request = _requests.First.Value;
                _requests.RemoveFirst();

                await _rateLimiter.Database.LockBucketAsync(Bucket.Id);
                Bucket = _rateLimiter.Database.GetBucket(Bucket.Id);
                
                if (!Bucket.FirstRequest && Bucket.Remaining == 0)
                {
                    var delay = Bucket.ResetsAt - DateTimeOffset.UtcNow;
                    if (delay > TimeSpan.Zero)
                    {
                        var level = Equals(request.Route.BaseRoute, Route.Channel.CreateReaction)
                            ? LogLevel.Debug
                            : LogLevel.Information;
                        Logger.Log(level, "Bucket {Id} is pre-emptively rate-limiting, delaying for {Delay}", Bucket.Id, delay);
                        await Task.Delay(delay);
                    }
                }

                await ExecuteAsync(request);

                await _rateLimiter.Database.UnlockBucketAsync(Bucket.Id);
            }
        }

        private async Task ExecuteAsync(IRestRequest request)
        {
            try
            {
                var response = await _rateLimiter.ApiClient.Requester.ExecuteAsync(request).ConfigureAwait(false);
                if (UpdateBucket(response.HttpResponse))
                {
                    Logger.LogInformation("Bucket {Id} is re-enqueuing the last request due to a hit rate-limit", Bucket.Id);
                    _requests.AddFirst(request);
                }
                else
                {
                    request.Complete(response);
                    request.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Bucket {Id} encountered an exception when executing a request", Bucket.Id);
            }
        }

        private bool UpdateBucket(IHttpResponse response)
        {
            var headers = new DefaultRestResponseHeaders(response.Headers);

            if (headers.IsGlobal.GetValueOrDefault())
            {
                Logger.LogError("Bucket {Id} hit the global rate-limit! Retry-After: {RetryAfter}ms", Bucket.Id, headers.RetryAfter.Value.TotalMilliseconds);
            }
            else if (response.Code == HttpResponseStatusCode.TooManyRequests)
            {
                Bucket.FirstRequest = false;
                Bucket.Remaining = 0;
                Bucket.ResetsAt = DateTimeOffset.UtcNow + headers.RetryAfter.Value;
                
                _rateLimiter.Database.SetBucket(Bucket);
                Logger.LogWarning("Bucket {Id} hit a rate-limit! Retry-After: {RetryAfter}ms)", Bucket.Id, headers.RetryAfter.Value.TotalMilliseconds);
                return true;
            }
            
            if (!headers.Bucket.HasValue) return false;

            Bucket.FirstRequest = false;
            Bucket.Limit = Math.Max(1, headers.Limit.Value);
            Bucket.Remaining = headers.Remaining.Value;
            Bucket.ResetsAt = DateTimeOffset.UtcNow + headers.ResetsAfter.Value;

            _rateLimiter.Database.SetBucket(Bucket);
            Logger.LogDebug("Bucket {Id} has updated to ({Remaining}/{Limit}, {ResetAfter})", Bucket.Id, Bucket.Remaining, Bucket.Limit, headers.ResetsAfter.Value);
            
            return false;
        }
    }
}
