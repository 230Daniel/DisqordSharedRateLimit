using System.Threading;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Hosting;
using Microsoft.Extensions.Logging;

namespace DisqordSharedRateLimit.Test
{
    public class TestService : DiscordClientService
    {
        private readonly ILogger<TestService> _logger;
        private readonly DiscordBotBase _bot;
        
        public TestService(ILogger<TestService> logger, DiscordBotBase bot)
        {
            _logger = logger;
            _bot = bot;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _bot.WaitUntilReadyAsync(stoppingToken);

            _logger.LogInformation("Ready!!!");
        }
    }
}
