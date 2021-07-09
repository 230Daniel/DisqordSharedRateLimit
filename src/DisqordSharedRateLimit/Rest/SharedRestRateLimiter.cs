using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord.Rest.Api;
using Disqord.Utilities.Binding;
using DisqordSharedRateLimit.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DisqordSharedRateLimit.Rest
{
    public sealed class SharedRestRateLimiter : IRestRateLimiter
    {
        public ILogger Logger { get; }
        public IRestApiClient ApiClient => _binder.Value;
        public IDatabase Database { get; }
        
        private readonly Binder<IRestApiClient> _binder;
        private readonly Dictionary<string, RequestQueue> _requestQueues;

        public SharedRestRateLimiter(
            ILogger<SharedRestRateLimiter> logger,
            IOptions<SharedRestRateLimiterConfiguration> config)
        {
            Logger = logger;
            _binder = new Binder<IRestApiClient>(this);
            _requestQueues = new();
            
            var redis = ConnectionMultiplexer.Connect(config.Value.RedisConfiguration);
            Database = redis.GetDatabase(0);
        }
        
        public void Bind(IRestApiClient value)
        {
            _binder.Bind(value);
        }
        
        public bool IsRateLimited(FormattedRoute route = null)
        {
            return false;
        }

        public ValueTask EnqueueRequestAsync(IRestRequest request)
        {
            if (ApiClient.Requester.Version < 8)
                request.Options.Headers["X-Ratelimit-Precision"] = "millisecond";

            var bucketId = GetBucketId(request.Route);
            Bucket bucket;
            
            lock (this)
            {
                bucket = Database.GetRestBucket(bucketId);
                if (bucket is null)
                {
                    bucket = new Bucket(bucketId)
                    {
                        FirstRequest = true
                    };
                    Database.SetRestBucket(bucket);
                }
            }
            
            if (!_requestQueues.TryGetValue(bucketId, out var requestQueue))
            {
                requestQueue = new(this, bucket);
                _requestQueues.Add(bucketId, requestQueue);
            }
            
            requestQueue.Enqueue(request);
            return default;
        }

        private string GetBucketId(FormattedRoute route)
        {
            var parameters = $"{route.Parameters.GuildId}:{route.Parameters.ChannelId}:{route.Parameters.WebhookId}";
            var bucketId = $"{route}:{parameters}";
            return bucketId;
        }
    }
}
