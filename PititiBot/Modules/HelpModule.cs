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

    public HelpModule(DiscordSocketClient client, InteractionService interactionService)
    {
        _client = client;
        _interactionService = interactionService;
    }

    [SlashCommand("help", "Shows all commands")]
    public async Task HandleServerCommand()
    {
        
    }

    public async Task ConvertToEmbed()
    {
        var embed = new EmbedBuilder()
            .WithTitle("Pititi list of HELPINGS!!")
            .WithDescription("These are the commands Pititi knows");

        var slashCommands = _interactionService.SlashCommands.ToList();
        foreach (var slashCommand in slashCommands)
        {
            embed.AddField($"{slashCommand.Name}",slashCommand.Description);
        }
    }
}