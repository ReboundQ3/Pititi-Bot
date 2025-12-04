using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using PititiBot.Services;

namespace PititiBot.Modules;

public class HelpModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;

    [SlashCommand("help", "Shows all commands")]

    public async Task HandleServerCommand()
    {
        var embed = new EmbedBuilder()
        {
            
        };

        var slashCommands = _interactionService.SlashCommands.ToList();
        foreach (var slashCommand in slashCommands)
        {
            await RespondAsync($"");
        }
    }
}