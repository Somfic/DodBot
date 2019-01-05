using System;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

namespace DodBot.Services
{
    public class CommandHandlingService
    {
        private readonly CommandService commandService;
        private readonly DiscordSocketClient client;
        private readonly IServiceProvider services;

        public CommandHandlingService(IServiceProvider services)
        {
            commandService = services.GetRequiredService<CommandService>();
            client = services.GetRequiredService<DiscordSocketClient>();
            this.services = services;

            commandService.CommandExecuted += CommandExecutedAsync;
            client.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;
            if (!message.Content.StartsWith("!")) return;

            var context = new SocketCommandContext(client, message);
            await commandService.ExecuteAsync(context, 1, services); // we will handle the result in CommandExecutedAsync
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
                return;

            // the command was succesful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
                return;

            // the command failed, let's notify the user that something happened.
            await context.Channel.SendMessageAsync($"Error: {result.ToString()}");
            Console.WriteLine($"Error while executing command {command.Value.Name}: {result.ToString()}");
        }
    }
}
