using Discord;
using Discord.Interactions;
using Microsoft.VisualBasic;

namespace PititiBot.Modules;

public class DiceModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("dice", "Pititi throws dice")]
    public async Task HandleDiceCommand(
        [Choice("D20", "d20"), Choice("D12", "d12"), Choice("D10", "d10"), Choice("D8", "d8"), Choice("D6", "d6"), Choice("D4", "d4")] string choice,
        [Summary("count", "How many dice to roll (1-10)")] int count = 1)
    {
        // Validate count
        if (count < 1 || count > 10)
        {
            await RespondAsync("PITITI CAN ONLY HOLD 1 TO 10 DICE!! Not more, not less!", ephemeral: true);
            return;
        }

        int maxValue;
        string diceName;

        switch (choice)
        {
            case "d20":
                maxValue = 20;
                diceName = "D20";
                break;
            case "d12":
                maxValue = 12;
                diceName = "D12";
                break;
            case "d10":
                maxValue = 10;
                diceName = "D10";
                break;
            case "d8":
                maxValue = 8;
                diceName = "D8";
                break;
            case "d6":
                maxValue = 6;
                diceName = "D6";
                break;
            case "d4":
                maxValue = 4;
                diceName = "D4";
                break;
            default:
                await RespondAsync("PITITI CONFUSED!! What dice is this??", ephemeral: true);
                return;
        }

        // Roll multiple dice
        var rolls = new List<int>();
        int total = 0;

        for (int i = 0; i < count; i++)
        {
            var roll = Random.Shared.Next(1, maxValue + 1);
            rolls.Add(roll);
            total += roll;
        }

        // Build response
        string response;
        if (count == 1)
        {
            var roll = rolls[0];
            string flavor = roll == maxValue ? "**IS MAX!! PITITI SO LUCKY!!**" :
                           roll == 1 ? "Oh no... is tiny number..." :
                           roll > maxValue / 2 ? "Is good roll!" :
                           "Hmm, not bad!";

            response = $"🎲 PITITI SHAKE {diceName}!! Is rolling for {Context.User.Mention}... \n**{roll}**! {flavor}";
        }
        else
        {
            // Multiple dice - show individual rolls and total
            var rollsText = string.Join(", ", rolls.Select(r => $"**{r}**"));
            var maxPossible = count * maxValue;
            var minPossible = count;

            string flavor = total == maxPossible ? "**ALL MAX!! PITITI IS AMAZINGS!!**" :
                           total == minPossible ? "Oh no... all tiny numbers..." :
                           total > (maxPossible + minPossible) / 2 ? "Is VERY good rolls!" :
                           "Is okay rolls!";

            response = $"🎲 PITITI SHAKE {count}{diceName}!! Is rolling for {Context.User.Mention}...\n" +
                      $"Rolls: {rollsText}\n" +
                      $"**Total: {total}**! {flavor}";
        }

        await RespondAsync(response);
    }
}