using System;
using System.Threading.Tasks;
using Disqord.Rest.Api;
using Disqord.Utilities.Binding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DisqordSharedRatelimit
{
    public sealed class SharedRestRateLimiter : IRestRateLimiter
    {
        public ILogger Logger { get; }
        public IRestApiClient ApiClient => _binder.Value;
        
        private readonly Binder<IRestApiClient> _binder;
        private ConnectionMultiplexer _redis;
        
        public SharedRestRateLimiter(
            ILogger<SharedRestRateLimiter> logger,
            IOptions<SharedRestRateLimiterConfiguration> config)
        {
            Logger = logger;
            _binder = new Binder<IRestApiClient>(this);
            _redis = ConnectionMultiplexer.Connect(config.Value.RedisConfiguration);
        }
        
        public void Bind(IRestApiClient value)
        {
            _binder.Bind(value);
        }
        
        public bool IsRateLimited(FormattedRoute route = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask EnqueueRequestAsync(IRestRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
