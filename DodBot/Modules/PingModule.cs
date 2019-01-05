using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

namespace DodBot.Modules
{
    public class PingModule : ModuleBase<SocketCommandContext>
    {
        public DiscordSocketClient client { get; set; }

        [Command("ping")]
        [Alias("pong", "delay", "latency")]
        public Task PingAsync() => ReplyAsync($"Pong! ({client.Latency} ms)");
    }
}
