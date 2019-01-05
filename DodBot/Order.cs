using System.Collections.Generic;
using Discord.WebSocket;

namespace DodBot
{
    public class Order
    {
        public Client Recipient;
        public Client Sender;
        public List<Item> Items;
        public string State;
        public uint Costs;
        public long ID;
        public string PickUpLocation;
        public string DropOffLocation;
    }

    public class Item
    {
        public string Content;
        public uint Amount;

        public Item(string content, uint amount)
        {
            Content = content;
            Amount = amount;
        }
    }

    public class Client
    {
        public string Username;
        public ulong DiscordID;

        public Client(string username, ulong discordID)
        {
            Username = username;
            DiscordID = discordID;
        }
    }
}
