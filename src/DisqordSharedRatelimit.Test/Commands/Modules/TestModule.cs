using System.Threading.Tasks;
using Disqord.Bot;
using Qmmands;

namespace DisqordSharedRatelimit.Test.Commands.Modules
{
    public class TestModule : DiscordModuleBase
    {
        [Command("Echo")]
        public DiscordCommandResult EchoAsync(
            [Remainder]
            string message)
        {
            return Response(message);
        }
        
        [Command("Spam")]
        [RequireBotOwner]
        public async Task<DiscordCommandResult> SpamAsync(
            [Range(0, 20)] 
            int count = 10)
        {
            for (var i = 1; i <= count; i++)
            {
                await Response($"Spamming! {i}/{count}");
            }
            return Response("Done");
        }
    }
}
