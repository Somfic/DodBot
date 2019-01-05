using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DodBot.Modules
{
    public class ServerModule : ModuleBase<SocketCommandContext>
    {
        [Command("announce")]
        [Alias("broadcast", "bc")]
        public Task AnnounceAsync([Remainder]string s)
        {
            var c = Context.Client.GetChannel(528695295698141201) as ITextChannel;
            SocketRole role;
            try { role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Staff"); } catch { return Task.CompletedTask; }
            var user = Context.User as SocketGuildUser;
            if (!user.Roles.Contains(role)) { return Task.CompletedTask; }

            return c.SendMessageAsync(s);
        }
    }
}
