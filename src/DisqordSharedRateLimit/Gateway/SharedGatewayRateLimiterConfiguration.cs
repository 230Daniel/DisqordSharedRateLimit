using StackExchange.Redis;

namespace DisqordSharedRateLimit.Gateway
{
    public class SharedGatewayRateLimiterConfiguration
    {
        public ConfigurationOptions RedisConfiguration { get; set; } = new();
    }
}
