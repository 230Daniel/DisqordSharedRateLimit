using StackExchange.Redis;

namespace DisqordSharedRateLimit
{
    public class SharedRateLimiterConfiguration
    {
        public ConfigurationOptions RedisConfiguration { get; set; } = new();
    }
}
