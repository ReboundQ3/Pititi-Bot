using Discord;
using Discord.Interactions;

namespace PititiBot.Modules;

public class CoinflipModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("coinflip", "Pititi flips a coin")]
    public async Task HandleCoinflipCommand()
    {
        var isHeads = Random.Shared.Next(0, 2) == 0;

        if (isHeads)
        {
            await RespondAsync("🪙 PITITI FLIP SHINY!! Is of... **HEADINGS!!** YAYA!");
        }
        else
        {
            await RespondAsync("🪙 PITITI FLIP SHINY!! Is of... **TAILINGS!!** YAYA!");
        }
    }
}