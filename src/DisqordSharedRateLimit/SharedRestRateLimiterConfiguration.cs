using StackExchange.Redis;

namespace DisqordSharedRatelimit
{
    public sealed class SharedRestRateLimiterConfiguration
    {
        public ConfigurationOptions RedisConfiguration { get; set; } = new();
    }
}
