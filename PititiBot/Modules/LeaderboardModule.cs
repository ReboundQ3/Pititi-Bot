using Discord;
using Discord.Interactions;

namespace PititiBot.Modules;

public class LeaderboardModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("leaderboard", "Pititi shows who steps on the most boom boxes!")]
    public async Task HandleLeaderboardCommand()
    {
        var guildId = Context.Guild?.Id;

        if (guildId == null)
        {
            await RespondAsync("❌ PITITI ONLY KEEP SHINE HATSIES IN SERVERS!! Not in DMs!", ephemeral: true);
            return;
        }

        var entries = BotConfig.LandmineService.GetLeaderboard(guildId.Value);

        if (entries.Count == 0)
        {
            await RespondAsync("NOBODY WALKSIES ON BOOM BOX Crownsies is of empty... go find boom yaya! 💣", ephemeral: true);
            return;
        }

        var medals = new[] { "🥇", "🥈", "🥉" };
        var lines = new List<string>();
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var rank = i < medals.Length ? medals[i] : $"**#{i + 1}**";
            lines.Add($"{rank} **{entry.Username}** — {entry.TotalMines} boom boxsies over {entry.TimesExploded} big boom(s)!");
        }

        var embed = new EmbedBuilder()
            .WithTitle("👑 Pititi BOOM BOX hat of shinyhat bestwinner YAYA!!")
            .WithDescription(string.Join("\n", lines))
            .WithColor(Color.Gold)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithFooter("YAYA!! Big boom, big hatsies!")
            .Build();

        await RespondAsync(embed: embed);
    }
}
