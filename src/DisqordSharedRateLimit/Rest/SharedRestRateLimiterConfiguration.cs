using StackExchange.Redis;

namespace DisqordSharedRatelimit.Rest
{
    public sealed class SharedRestRateLimiterConfiguration
    {
        public ConfigurationOptions RedisConfiguration { get; set; } = new();
    }
}
