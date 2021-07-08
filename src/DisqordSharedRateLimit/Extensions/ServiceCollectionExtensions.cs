using System;
using Disqord.Rest.Api;
using Microsoft.Extensions.DependencyInjection;

namespace DisqordSharedRatelimit.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedRestRateLimiter(this IServiceCollection services, Action<SharedRestRateLimiterConfiguration> configure)
        {
            services.AddSingleton<IRestRateLimiter, SharedRestRateLimiter>();
            services.Configure(configure);

            return services;
        }
    }
}
