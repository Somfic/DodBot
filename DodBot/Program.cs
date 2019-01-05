using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DodBot.Services;
using Microsoft.Extensions.DependencyInjection;
using DodBot.Services;
using System.Collections.Generic;
using Sheets;

namespace DodBot
{
    class Program
    {
        private static DiscordSocketClient client;

        public static List<Order> unProcessedOrders = new List<Order>();
        public static List<Order> ProcessedOrders = new List<Order>();

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        public static void Save()
        {
            SheetsConnection.Write(Newtonsoft.Json.JsonConvert.SerializeObject(ProcessedOrders));
            Load();
        }

        public static void Load()
        {
            ProcessedOrders = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Order>>(SheetsConnection.Read());
        }

        static void Main(string[] args)
        {
            Load();
            new Program().Start().GetAwaiter().GetResult();
        }

        private async Task Start()
        {
            ServiceProvider services = ConfigureServices();

            client = services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;
            services.GetRequiredService<CommandService>().Log += LogAsync;

            string token = Environment.GetEnvironmentVariable("token", EnvironmentVariableTarget.User);

            await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("TOKEN"));
            await client.StartAsync();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

            await Task.Delay(-1);
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<CommandService>()
                .BuildServiceProvider();
        }
    }
}
