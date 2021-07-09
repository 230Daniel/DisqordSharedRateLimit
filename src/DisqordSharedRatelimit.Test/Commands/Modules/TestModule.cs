using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Qmmands;

namespace DisqordSharedRateLimit.Test.Commands.Modules
{
    public class TestModule : DiscordGuildModuleBase
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
        public DiscordCommandResult SpamAsync(
            [Range(0, 20)] 
            int count = 10)
        {
            for (var i = 1; i <= count; i++)
            {
                _ = Context.Channel.SendMessageAsync(new LocalMessage().WithContent($"Spamming! {i}/{count}"));
            }
            return Response("Done");
        }
    }
}
