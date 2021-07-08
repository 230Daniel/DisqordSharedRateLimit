using StackExchange.Redis;

namespace DisqordSharedRateLimit.Rest
{
    public sealed class SharedRestRateLimiterConfiguration
    {
        public ConfigurationOptions RedisConfiguration { get; set; } = new();
    }
}
