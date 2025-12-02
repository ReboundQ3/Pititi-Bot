using Discord;
using Discord.Interactions;

public class PingModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Checks if Pititi is alive")]
    public async Task HandlePingCommand()
    {
        await RespondAsync("HIHI IS OF PITITI!! 🦎");
    }
}