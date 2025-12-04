using Discord;
using Discord.Interactions;

namespace PititiBot.Modules;

public class DiceModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("dice", "Pititi throws dice")]
    public async Task HandleDiceCommand(
        [Choice("D100", "d100"), Choice("D20", "d20"), Choice("D12", "d12"), Choice("D10", "d10"), Choice("D8", "d8"), Choice("D6", "d6"), Choice("D4", "d4")] string choice,
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
            case "d100":
                maxValue = 100;
                diceName = "D100";
                break;
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
            string flavor = GetSingleRollFlavor(roll, maxValue);
            response = $"🎲 PITITI SHAKE {diceName}!! Is rolling for {Context.User.Mention}... \n**{roll}**! {flavor}";
        }
        else
        {
            // Multiple dice - show individual rolls and total
            var rollsText = string.Join(", ", rolls.Select(r => $"**{r}**"));
            var maxPossible = count * maxValue;
            var minPossible = count;

            string flavor = GetMultipleRollFlavor(total, maxPossible, minPossible);
            response = $"🎲 PITITI SHAKE {count}{diceName}!! Is rolling for {Context.User.Mention} timings {count}...\n" +
                      $"Rolls: {rollsText}\n" +
                      $"**Total: {total}**! {flavor}";
        }

        await RespondAsync(response);
    }

    private string GetSingleRollFlavor(int roll, int maxValue)
    {
        if (roll == maxValue)
            return "**IS MAX!! PITITI SO LUCKY!!**";

        if (roll == 1)
            return "Oh no... is tiny number...";

        if (roll > maxValue / 2)
            return "Is good roll!";

        return "Hmm, not bad!";
    }

    private string GetMultipleRollFlavor(int total, int maxPossible, int minPossible)
    {
        if (total == maxPossible)
            return "**ALL MAX!! PITITI IS AMAZINGS!!**";

        if (total == minPossible)
            return "Oh no... all tiny numbers...";

        if (total > (maxPossible + minPossible) / 2)
            return "Is VERY good rolls!";

        return "Is okay rolls!";
    }
}