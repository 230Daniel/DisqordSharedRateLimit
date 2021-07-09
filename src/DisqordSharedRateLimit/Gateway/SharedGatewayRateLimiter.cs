using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway.Api;
using Disqord.Gateway.Api.Models;
using Disqord.Utilities.Binding;
using DisqordSharedRateLimit.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DisqordSharedRateLimit.Gateway
{
    public class SharedGatewayRateLimiter : IGatewayRateLimiter
    {
        private const int Heartbeats = 2;
        
        public ILogger Logger => ApiClient.Logger;
        public IGatewayApiClient ApiClient => _binder.Value;
        public ulong ClientId => (ApiClient.Token as BotToken).Id;
        public IDatabase Database { get; }

        private readonly ILoggerFactory _loggerFactory;
        private readonly Binder<IGatewayApiClient> _binder;
        private readonly SimpleBucket _masterBucket;
        private readonly Dictionary<GatewayPayloadOperation, SimpleBucket> _buckets;

        public SharedGatewayRateLimiter(
            IOptions<SharedRateLimiterConfiguration> config,
            ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;

            _binder = new Binder<IGatewayApiClient>(this, x =>
            {
                if (x.Token is not BotToken)
                    throw new ArgumentException("The shared gateway rate-limiter supports only bot tokens.");
            });

            _masterBucket = new SimpleBucket(null, 120 - Heartbeats, TimeSpan.FromSeconds(60));
            _buckets = new Dictionary<GatewayPayloadOperation, SimpleBucket>(2);
            
            var redis = ConnectionMultiplexer.Connect(config.Value.RedisConfiguration);
            Database = redis.GetDatabase(0);
        }

        public void Bind(IGatewayApiClient apiClient)
        {
            _binder.Bind(apiClient);
            _buckets[GatewayPayloadOperation.UpdatePresence] = new SimpleBucket(_loggerFactory.CreateLogger("Presence Bucket"), 5, TimeSpan.FromSeconds(60));
        }
        
        public bool IsRateLimited(GatewayPayloadOperation? operation = null)
        {
            if (operation != null)
            {
                var bucket = _buckets.GetValueOrDefault(operation.Value);
                if (bucket?.CurrentCount == 0)
                    return true;
            }

            return _masterBucket.CurrentCount == 0;
        }
        
        public int GetRemainingRequests(GatewayPayloadOperation? operation = null)
        {
            if (operation != null)
            {
                var bucket = _buckets.GetValueOrDefault(operation.Value);
                if (bucket != null)
                    return bucket.CurrentCount;
            }

            return _masterBucket.CurrentCount;
        }
        
        public async Task WaitAsync(GatewayPayloadOperation? operation = null, CancellationToken cancellationToken = default)
        {
            if (operation != null)
            {
                if (operation.Value == GatewayPayloadOperation.Heartbeat)
                    return;

                if (operation.Value == GatewayPayloadOperation.Identify)
                {
                    var bucketId = $"Identify | {ClientId}";
                    await Database.LockGatewayBucketAsync(bucketId);
                    var sharedBucket = Database.GetGatewayBucket(bucketId);
                    
                    sharedBucket ??= new(bucketId)
                    {
                        Limit = 1,
                        Remaining = 1,
                        ResetsAt = DateTimeOffset.MaxValue
                    };

                    if (sharedBucket.ResetsAt <= DateTimeOffset.UtcNow)
                    {
                        sharedBucket.Remaining = sharedBucket.Limit;
                    }
                    else if (sharedBucket.Remaining == 0)
                    {
                        await Task.Delay(sharedBucket.ResetsAt - DateTimeOffset.UtcNow, cancellationToken);
                        sharedBucket.Remaining = sharedBucket.Limit;
                        sharedBucket.ResetsAt = DateTimeOffset.UtcNow.AddSeconds(5.5);
                    }
                    
                    Database.SetGatewayBucket(sharedBucket);
                }

                var bucket = _buckets.GetValueOrDefault(operation.Value);
                if (bucket != null)
                    await bucket.WaitAsync(cancellationToken).ConfigureAwait(false);
            }

            await _masterBucket.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void NotifyCompletion(GatewayPayloadOperation? operation = null)
        {
            if (operation != null)
            {
                if (operation.Value == GatewayPayloadOperation.Heartbeat)
                    return;

                if (operation.Value == GatewayPayloadOperation.Identify)
                {
                    var bucketId = $"Identify | {ClientId}";
                    var sharedBucket = Database.GetGatewayBucket(bucketId);

                    sharedBucket.Remaining--;
                    if (sharedBucket.Remaining == 0)
                    {
                        sharedBucket.ResetsAt = DateTimeOffset.UtcNow.AddSeconds(5.5);
                    }
                    
                    Database.SetGatewayBucket(sharedBucket);
                    Database.UnlockGatewayBucket(bucketId);
                }
                
                var bucket = _buckets.GetValueOrDefault(operation.Value);
                bucket?.NotifyCompletion();
            }

            _masterBucket.NotifyCompletion();
        }

        /// <inheritdoc/>
        public void Release(GatewayPayloadOperation? operation = null)
        {
            if (operation != null)
            {
                if (operation.Value == GatewayPayloadOperation.Heartbeat)
                    return;

                var bucket = _buckets.GetValueOrDefault(operation.Value);
                bucket?.Release();
            }

            _masterBucket.Release();
        }
    }
}
