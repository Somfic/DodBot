using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DodBot.Services;

namespace DodBot.Modules
{
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("staff update")]
        public Task UpdateAsync(string id, [Remainder]string state)
        {
            id = id.Replace("#", "");

            SocketRole role;
            try { role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Staff"); } catch { return Task.CompletedTask; }
            var user = Context.User as SocketGuildUser;
            if (!user.Roles.Contains(role)) { return Task.CompletedTask; }
            if (Program.ProcessedOrders.Count(x => x.ID.ToString() == id) == 0) { return ReplyAsync("Sorry, I couldn't find that order."); }
            Program.ProcessedOrders.Where(x => x.ID.ToString() == id).First().State = state;
            Order o = Program.ProcessedOrders.Where(x => x.ID.ToString() == id).First();

            Context.Client.GetUser(o.Sender.DiscordID).SendMessageAsync("", false, OrderEmbed.GenOrder(o));
            try { Context.Client.GetUser(o.Recipient.DiscordID).SendMessageAsync("", false, OrderEmbed.GenOrder(o, Context)); } catch { }
            var c = Context.Client.GetChannel(531083554927542284) as ITextChannel;
            c.SendMessageAsync($"Order #{o.ID} updated with state new state: {o.State}.", false, OrderEmbed.GenOrder(o, Context));

            Program.Save();
            return ReplyAsync("State has been updated and the users have been notified.");
        }

        [Command("staff remove")]
        public Task RemoveAsync(string id, [Remainder]string reason = "Not specified")
        {
            SocketRole role;
            try { role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Staff"); } catch { return Task.CompletedTask; }
            var user = Context.User as SocketGuildUser;
            if (!user.Roles.Contains(role)) { return Task.CompletedTask; }
            if (Program.ProcessedOrders.Count(x => x.ID.ToString() == id) == 0) { return ReplyAsync("Sorry, I couldn't find that order."); }
            Order o = Program.ProcessedOrders.Where(x => x.ID.ToString() == id).First();
            o.State = "Removed";
            Program.ProcessedOrders.Remove(o);
            Program.Save();

            Context.Client.GetUser(o.Sender.DiscordID).SendMessageAsync($"Hello, your order `#{o.ID}` has been marked removed by {Context.User.Mention} for '{reason}'.\nIf you believe this is an error, please contact DOD staff.", false, OrderEmbed.GenOrder(o));
            try { Context.Client.GetUser(o.Recipient.DiscordID).SendMessageAsync($"Hello, your order `#{o.ID}` has been marked removed by {Context.User.Mention} for '{reason}'.\nIf you believe this is an error, please contact DOD staff.", false, OrderEmbed.GenOrder(o)); } catch { }
            var c = Context.Client.GetChannel(531083554927542284) as ITextChannel;
            c.SendMessageAsync($"Order `#{o.ID}` has been removed by {Context.User.Mention}.\nReason: {reason}", false, OrderEmbed.GenOrder(o, Context));

            return ReplyAsync("The order has been removed.");
        }
    }
}
