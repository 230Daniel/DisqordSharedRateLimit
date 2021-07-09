# DisqordSharedRateLimit

A ratelimiter implementation for the [Disqord](https://github.com/Quahu/Disqord/) library.

 - Makes use of [Redis](https://redis.io/) to communicate between seperate bot processes
 - Obeys rest rate limits per-route
 - Obeys the global rest rate limit (WIP)
 - Obeys the global gateway identify rate limit

## Example Usage

```cs
private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddSharedRateLimiters(config =>
    {
        config.RedisConfiguration = new()
        {
            EndPoints =
            {
                { context.Configuration["Redis:Host"], context.Configuration.GetValue<int>("Redis:Port") }
            },
			DefaultDatabase = 0,
            Password = context.Configuration["Redis:Password"]
        };
    });
}
// To add only one ratelimiter you can use the AddSharedRestRateLimiter or AddSharedGatewayRateLimiter extension methods.
```
