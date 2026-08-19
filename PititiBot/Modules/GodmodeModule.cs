using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace PititiBot.Modules;

public class GodmodeModule : InteractionModuleBase<SocketInteractionContext>
{
    [DefaultMemberPermissions(GuildPermission.ManageMessages)]
    [SlashCommand("godmode", "Spares people from Pititi's landmines")]
    public async Task HandleGodmodeCommand(
        [Choice("Add", "add"), Choice("Remove", "remove"), Choice("List", "list")] string action,
        [Summary("user", "Who to bless or unbless. Only used with Add and Remove.")] SocketUser? user = null)
    {
        var guildId = Context.Guild?.Id;

        if (guildId == null)
        {
            await RespondAsync("❌ PITITI ONLY GIVE SHINY PROTECTINGS IN SERVERS!! Not in DMs!", ephemeral: true);
            return;
        }

        if (action == "list")
        {
            var entries = BotConfig.LandmineService.GetGodmodes(guildId.Value);

            if (entries.Count == 0)
            {
                await RespondAsync("NOBODY IS OF SAFESIES HERE! Everyone go BOOM, yaya! 💣", ephemeral: true);
                return;
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle("✨ Pititi list of NO-BOOM peoples!")
                .WithDescription($"Pititi count {entries.Count} people who walksies safe past boom boxes!")
                .WithColor(Color.Gold)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter("YAYA!! No boom for them!");

            // Discord embeds allow at most 25 fields.
            const int maxFields = 25;
            foreach (var entry in entries.Take(maxFields))
            {
                embedBuilder.AddField(
                    $"✨ {entry.Username}",
                    $"Blessed by **{entry.GrantedByUsername}** <t:{entry.GrantedAt.ToUnixTimeSeconds()}:R>");
            }

            if (entries.Count > maxFields)
            {
                embedBuilder.WithFooter($"YAYA!! ...and {entries.Count - maxFields} more safesies people Pititi no show here!");
            }

            await RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
            return;
        }

        if (user == null)
        {
            await RespondAsync("PITITI NEED PERSON! Tell Pititi who to give protectings to!", ephemeral: true);
            return;
        }

        var username = user.GlobalName ?? user.Username;

        if (action == "add")
        {
            var granterName = Context.User.GlobalName ?? Context.User.Username;
            var added = BotConfig.LandmineService.AddGodmode(guildId.Value, user.Id, username, granterName);

            if (!added)
            {
                await RespondAsync($"**{username}** ALREADY OF SAFESIES!", ephemeral: true);
                return;
            }

            await RespondAsync($"PITITI GIVE **{username}** SHINY NO-BOOM HATSIES!!");
        }
        else if (action == "remove")
        {
            var removed = BotConfig.LandmineService.RemoveGodmode(guildId.Value, user.Id);

            if (!removed)
            {
                await RespondAsync($"**{username}** NEVER HAD SHINY HATSIES!", ephemeral: true);
                return;
            }

            await RespondAsync($"PITITI TAKEINGS **{username}** SHINY NO-BOOM HATSIES AWAY! 💣 They go BOOMSIES again now, yaya!");
        }
    }
}
