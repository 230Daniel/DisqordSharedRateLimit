using System;
using System.Threading.Tasks;
using Disqord.Bot.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DisqordSharedRateLimit.Extensions;
using Microsoft.Extensions.Configuration;

namespace DisqordSharedRateLimit.Test
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .ConfigureDiscordBotSharder((context, bot) =>
                {
                    bot.Token = context.Configuration["Discord:Token"];
                    //bot.ShardCount = 3;
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
            services.AddSharedRateLimiters(config =>
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
