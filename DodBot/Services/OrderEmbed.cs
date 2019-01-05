using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;

namespace DodBot.Services
{
    public static class OrderEmbed
    {
        public static Embed GenOrder(Order o, SocketCommandContext Context = null)
        {
            EmbedBuilder embed = new EmbedBuilder();
            try { embed.WithAuthor("Confirmed by " + Context.User); } catch { }
            try { embed.Author.IconUrl = Context.User.GetAvatarUrl(); } catch { }
            embed.WithColor(Color.LighterGrey);
            embed.WithTitle($"Order #{o.ID}");
            embed.WithDescription($"Sender: <@{o.Sender.DiscordID}> \nRecipient: {GetRec(o.Recipient.DiscordID, o.Recipient.Username)}\nPickup: {o.PickUpLocation}\nDrop-off: {o.DropOffLocation}");
            embed.Description += $"\nStatus: {o.State}";
            foreach (var item in o.Items)
            {
                embed.AddField(item.Content, $"x{item.Amount}", true);
            }

            return embed.Build();
        }

        public static string GetRec(ulong id, string username)
        {
            if (id == 0) { return username; } else { return $"<@{id}>"; }
        }
    }
}
