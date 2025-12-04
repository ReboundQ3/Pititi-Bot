using Discord;
using Discord.Interactions;

namespace PititiBot.Modules;

public class PingModule : InteractionModuleBase<SocketInteractionContext>
{
    [DefaultMemberPermissions(GuildPermission.ManageRoles)]
    [SlashCommand("ping", "Checks if Pititi is alive")]
    public async Task HandlePingCommand()
    {
        await RespondAsync("HIHI IS OF PITITI!!");
    }
}