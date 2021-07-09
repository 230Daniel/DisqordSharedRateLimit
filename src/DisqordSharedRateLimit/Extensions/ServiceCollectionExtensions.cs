﻿using System;
using Disqord.Gateway.Api;
using Disqord.Rest.Api;
using DisqordSharedRateLimit.Gateway;
using DisqordSharedRateLimit.Rest;
using Microsoft.Extensions.DependencyInjection;

namespace DisqordSharedRateLimit.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedRestRateLimiter(this IServiceCollection services, Action<SharedRestRateLimiterConfiguration> configure)
        {
            services.AddSingleton<IRestRateLimiter, SharedRestRateLimiter>();
            services.Configure(configure);

            return services;
        }
        
        public static IServiceCollection AddSharedGatewayRateLimiter(this IServiceCollection services, Action<SharedGatewayRateLimiterConfiguration> configure)
        {
            services.AddScoped<IGatewayRateLimiter, SharedGatewayRateLimiter>();
            services.Configure(configure);

            return services;
        }
    }
}
