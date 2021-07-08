using System;
using System.Threading.Tasks;
using Disqord.Bot.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DisqordSharedRatelimit.Extensions;
using Microsoft.Extensions.Configuration;

namespace DisqordSharedRatelimit.Test
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .ConfigureDiscordBot((context, bot) =>
                {
                    bot.Token = context.Configuration["Discord:Token"];
                })
                .Build();

            try
            {
                using (host)
                {
                    await host.RunAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }
        
        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSharedRestRateLimiter(config =>
            {
                config.RedisConfiguration = new()
                {
                    EndPoints =
                    {
                        { context.Configuration["Redis:Host"], context.Configuration.GetValue<int>("Redis:Port") }
                    },
                    Password = context.Configuration["Redis:Password"]
                };
            });
        }
    }
}
