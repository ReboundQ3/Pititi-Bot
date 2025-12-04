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
        var embed = await MakeEmbed();
        await RespondAsync(embed: embed, ephemeral: true);
    }

    public async Task<Embed> MakeEmbed()
    {
        var embedBuilder = new EmbedBuilder()
            .WithTitle("Pititi list of HELPINGS!!")
            .WithDescription("These are the commands Pititi knows")
            .WithColor(Color.Green)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithFooter("Pititi is of helpings!!");

        var slashCommands = _interactionService.SlashCommands.ToList();
        foreach (var slashCommand in slashCommands)
        {
            var fieldValue = slashCommand.Description;

            // Add parameter details including choices
            foreach (var parameter in slashCommand.Parameters)
            {
                if (parameter.Choices.Count > 0)
                {
                    var choicesList = string.Join(", ", parameter.Choices.Select(c => c.Name));
                    fieldValue += $"\n**{parameter.Name}**: {choicesList}";
                }
            }

            embedBuilder.AddField($"{slashCommand.Name} | {slashCommand.Parameters}", fieldValue);
        }

        var embed = embedBuilder.Build();
        return embed;
    }
}