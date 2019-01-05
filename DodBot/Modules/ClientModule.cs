using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DodBot.Services;
using Sheets;

namespace DodBot.Modules
{
    public class ClientModule : ModuleBase<SocketCommandContext>
    {
        [Command("order new")]
        public Task NewOrderAsync(string recipient)
        {
            SocketUser recipientUser = Context.Client.GetUser(recipient, "####");
            if (recipientUser == null) { ReplyAsync("Discord account was not detected, recipient receipt not possible."); return NewOrderAsync(recipient, 0, Context); }
            return NewOrderAsync(recipientUser.Username, recipientUser.Id, Context);
        }

        [Command("order new")]
        public Task NewOrderAsync() => ReplyAsync("Please also include the recipient.");

        [Command("order new")]
        public Task NewOrderAsync(SocketUser user) => NewOrderAsync(user.Username, user.Id, Context);

        private Task NewOrderAsync(string username, ulong id, SocketCommandContext Context)
        {
            if (Program.unProcessedOrders.Count(x => x.Sender.DiscordID == Context.User.Id) != 0)
            {
                return ReplyAsync("You have an order in progress. \nUse `!order confirm` to close your current order.");
            }

            Order o = new Order
            {
                Recipient = new Client(username, id),
                Sender = new Client(Context.User.Username, Context.User.Id),
                State = "Being created",
                ID = DateTime.UtcNow.Ticks,
                Items = new List<Item>(),
                Costs = 0
            };

            Program.unProcessedOrders.Add(o);

            return ReplyAsync($"Your order has been created {o.Sender.Username}. \nTo add items to your order, use `!order add <amount> <item>`.\nTo confirm the order, use `!order confirm`.");
        }

        [Command("order add")]
        public Task OrderAddAsync(uint amount, [Remainder]string content)
        {
            content = content[0].ToString().ToUpper() + content.Substring(1).ToLower();

            if (Program.unProcessedOrders.Count(x => x.Sender.DiscordID == Context.User.Id) == 0) { return ReplyAsync("It seems like you are not in the process of creating an order. \nIf you'd like to create one, use `!order new <recipient>`.\nIf you want a list of your current orders, do `!orders`."); }

            List<Item> items = Program.unProcessedOrders.Where(x => x.Sender.DiscordID == Context.User.Id).First().Items;

            if (items.Count(x => x.Content == content) != 0) { items.Where(x => x.Content == content).First().Amount += amount; }
            else { items.Add(new Item(content, amount)); }

            return ReplyAsync($"{content} (x{amount}) has been added to your order.");
        }

        [Command("order remove")]
        [Alias("order take")]
        public Task OrderRemoveAsync([Remainder]string content)
        {
            content = content[0].ToString().ToUpper() + content.Substring(1).ToLower();
            return OrderRemoveAsync(uint.MaxValue, content);
        }

        [Command("order remove")]
        [Alias("order take")]
        public Task OrderRemoveAsync(uint amount, [Remainder]string content)
        {
            content = content[0].ToString().ToUpper() + content.Substring(1).ToLower();

            if (Program.unProcessedOrders.Count(x => x.Sender.DiscordID == Context.User.Id) == 0) { return ReplyAsync("It seems like you are not in the process of creating an order. \nIf you'd like to create one, use `!order new <recipient>`.\nIf you want a list of your current orders, do `!orders`."); }
            if (Program.unProcessedOrders.Where(x => x.Sender.DiscordID == Context.User.Id).First().Items.Count(x => x.Content == content) == 0) { return ReplyAsync("It seems like you do not have that item in your order."); }

            if (Program.unProcessedOrders.Where(x => x.Sender.DiscordID == Context.User.Id).First().Items.Where(x => x.Content == content).First().Amount <= amount)
            {
                uint previousAmount = Program.unProcessedOrders.Where(x => x.Sender.DiscordID == Context.User.Id).First().Items.Where(x => x.Content == content).First().Amount;
                Program.unProcessedOrders.Where(x => x.Sender.DiscordID == Context.User.Id).First().Items.Remove(Program.unProcessedOrders.Where(x => x.Sender.DiscordID == Context.User.Id).First().Items.Where(x => x.Content == content).First());
                return ReplyAsync($"{content} (x{previousAmount}) has been removed from your order.");
            }
            else
            {
                Program.unProcessedOrders.Where(x => x.Sender.DiscordID == Context.User.Id).First().Items.Where(x => x.Content == content).First().Amount -= amount;
                return ReplyAsync($"{content} (x{amount}) has been removed from your order.");
            }
        }

        [Command("order abort")]
        [Alias("order cancel", "order stop")]
        public Task OrderAbortAsync()
        {
            if (Program.unProcessedOrders.Count(x => x.Sender.DiscordID == Context.User.Id) == 0) { return ReplyAsync("It seems like you are not in the process of creating an order. \nIf you'd like to create one, use `!order new <recipient>`.\nIf you want a list of your current orders, do `!orders`."); }
            Order o = Program.unProcessedOrders.Where(x => x.Sender.DiscordID == Context.User.Id).First();
            Program.unProcessedOrders.Remove(o);

            return ReplyAsync("The order has been cancelled.");
        }

        [Command("order")]
        public Task OrderASync(string id)
        {
            id = id.Replace("#", "");

            if (Program.ProcessedOrders.Count(x => x.ID.ToString() == id && (x.Sender.DiscordID == Context.User.Id || x.Recipient.DiscordID == Context.User.Id)) == 0) { return ReplyAsync("It seems like I couldn't find any orders with that tracking number for you. \nIf you'd like to create one, use `!order new <recipient>`.\nIf you want a list of your current orders, do `!orders`."); }

            Order o = Program.ProcessedOrders.Where(x => x.ID.ToString() == id).First();

            return ReplyAsync("", false, OrderEmbed.GenOrder(o, Context));
        }

        [Command("order")]
        public Task OrderASync()
        {
            if (Program.unProcessedOrders.Count(x => x.Sender.DiscordID == Context.User.Id) == 0) { return ReplyAsync("It seems like you are not in the process of creating an order. \nIf you'd like to create one, use `!order new <recipient>`.\nIf you want a list of your current orders, do `!orders`."); }

            Order o = Program.unProcessedOrders.Where(x => x.Sender.DiscordID == Context.User.Id).First();

            return ReplyAsync("", false, OrderEmbed.GenOrder(o, Context));
        }

        [Command("order confirm")]
        [Alias("order close")]
        public Task OrderConfirmASync()
        {
            if (Program.unProcessedOrders.Count(x => x.Sender.DiscordID == Context.User.Id) == 0) { return ReplyAsync("It seems like you are not in the process of creating an order. \nIf you'd like to create one, use `!order new <recipient>`.\nIf you want a list of your current orders, do `!orders`."); }

            Order o = Program.unProcessedOrders.Where(x => x.Sender.DiscordID == Context.User.Id).First();

            if (string.IsNullOrWhiteSpace(o.PickUpLocation)) { return ReplyAsync("Please also set a pick-up location using !order pickup <coords/po box>"); }
            if (string.IsNullOrWhiteSpace(o.DropOffLocation)) { return ReplyAsync("Please also set a drop-off location using !order dropoff <coords/po box>"); }

            Program.unProcessedOrders.Remove(o);
            o.State = "Order acknowledged";
            Program.ProcessedOrders.Add(o);
            Program.Save();

            try { Context.Client.GetUser(o.Recipient.DiscordID).SendMessageAsync($"Hello.\n{Context.User.Mention} has sent you an order!\nWe will deliver it as soon as possible.\nHere's the receipt.", false, OrderEmbed.GenOrder(o, Context)); } catch { }
            Context.Client.GetUser(o.Sender.DiscordID).SendMessageAsync("Hello, thank you for using DOD!\nHere's your order reciept.", false, OrderEmbed.GenOrder(o, Context));

            var c = Context.Client.GetChannel(531083554927542284) as ITextChannel;
            c.SendMessageAsync($"New order from {Context.User.Mention} to { OrderEmbed.GetRec(o.Recipient.DiscordID, o.Recipient.Username)}.", false, OrderEmbed.GenOrder(o, Context));

            return ReplyAsync("Your order has been confirmed.");
        }

        [Command("order pickup")]
        [Alias("order collection")]
        public Task PickUpAsync([Remainder] string s)
        {
            Program.unProcessedOrders.Where(x => x.Sender.DiscordID == Context.User.Id).First().PickUpLocation = s;
            return ReplyAsync($"Collection location set to {s}.");
        }

        [Command("order dropoff")]
        public Task DropOffAsync([Remainder] string s)
        {
            Program.unProcessedOrders.Where(x => x.Sender.DiscordID == Context.User.Id).First().DropOffLocation = s;
            return ReplyAsync($"Drop-off location set to {s}.");
        }

        [Command("orders")]
        [Alias("order list")]
        public Task OrdersAsync()
        {
            if (Program.ProcessedOrders.Count(x => x.Sender.DiscordID == Context.User.Id) == 0) { return ReplyAsync("It seems like you are do not have any active orders. \nIf you'd like to create one, use `!order new <recipient>`."); }

            Program.Load();

            List<Order> orders = Program.ProcessedOrders.Where(x => x.Sender.DiscordID == Context.User.Id || x.Recipient.DiscordID == Context.User.Id).ToList();

            foreach (var order in orders)
            {
                Context.User.SendMessageAsync("", false, OrderEmbed.GenOrder(order, Context));
            }

            if(!Context.IsPrivate) { ReplyAsync("I've sent you your orders."); }

            return Task.CompletedTask;
        }
    }
}
