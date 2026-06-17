using Discord.Interactions;

namespace PititiBot.Modules;

public class EightballModule : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly string[] Responses =
    {
        // Positive responses
        "🔮 YAYA!! Pititi sees YES in the shiny ball!!",
        "🔮 Is of CERTAIN!! Pititi knows this!!",
        "🔮 Without of DOUBT!! Pititi is sure YAYA!!",
        "🔮 YES YES!! Pititi sees good things!!",
        "🔮 You can of COUNT on it!! YAYA!!",
        "🔮 Most of DEFINITELY!! Pititi approves!!",
        "🔮 Outlook is of GOOD!! Pititi is happy!!",
        "🔮 Signs point to of YES!! YAYA!!",

        // Uncertain responses
        "🔮 Reply of HAZY... Pititi cannot see clear...",
        "🔮 Ask of AGAIN later... Pititi is confused...",
        "🔮 Better not of TELL you now... shh...",
        "🔮 Cannot of SEE now... ball is cloudy...",
        "🔮 Think hard and of ASK again... Pititi needs think...",

        // Negative responses
        "🔮 Don't of COUNT on it... Pititi says no...",
        "🔮 Pititi's reply is of NO...",
        "🔮 Pititi sources say of NO... sorry friend...",
        "🔮 Outlook not of SO GOOD... is sad...",
        "🔮 Very of DOUBTFUL... Pititi doesn't think so...",
        "🔮 NO NO!! Pititi sees bad in ball!!"
    };

    [SlashCommand("8ball", "Shake the Pititi eightball and let Pititi decide fate")]
    public async Task HandleEightballCommand()
    {
        var response = Responses[Random.Shared.Next(Responses.Length)];
        await RespondAsync(response);
    }
}