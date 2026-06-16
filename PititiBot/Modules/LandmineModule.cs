using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace PititiBot.Modules;

public class LandmineModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;

    public LandmineModule(DiscordSocketClient client, InteractionService interactionService)
    {
        _client = client;
        _interactionService = interactionService;
    }

    [DefaultMemberPermissions(GuildPermission.ManageMessages)]
    [SlashCommand("landmine", "Pititi places a landmine in the chat for someone to stumble over")]
    public async Task HandleLandmineCommand(
        [Choice("Place", "place"), Choice("Remove", "remove"), Choice("Status", "status")] string action,
        [Summary("id", "The boom box Id to remove (see Status). Only used with Remove.")] long id = 0)
    {
        var channelId = Context.Channel.Id;

        if (action == "place")
        {
            // Random number between 1 and 250
            var random = new Random();
            var countdown = random.Next(1, 250);

            var landmine = BotConfig.LandmineService.PlaceLandmine(channelId, countdown, Context.User.Id, Context.User.GlobalName ?? Context.User.Username);

            if (landmine == null)
            {
                await RespondAsync("PITITI BOOM BOX BROKE! No place this time, try again!", ephemeral: true);
                return;
            }

            await RespondAsync($"PITITI PLACE BOOM BOX!! 💣 (#{landmine.Id}) Will go BOOM in.. Shhhh Gorb say is secret");
        }
        else if (action == "remove")
        {
            if (id <= 0)
            {
                await RespondAsync("PITITI NEED NUMBER! Tell Pititi which boom box Id to take (look at Status to see them)!", ephemeral: true);
                return;
            }

            var removed = BotConfig.LandmineService.RemoveLandmine(channelId, id);

            if (removed == null)
            {
                await RespondAsync($"NO BOOM BOX #{id} HERE! Pititi already took it or never put it!", ephemeral: true);
                return;
            }

            await RespondAsync($"PITITI TAKE BOOM BOX #{removed.Id} AWAY! 🧹 Was gonna boom in {removed.RemainingMessages} messages. Is safe now!");
        }
        else if (action == "status")
        {
            var landmines = BotConfig.LandmineService.GetLandmines(channelId);

            if (landmines.Count == 0)
            {
                await RespondAsync("NO BOOM BOX HERE! Is safe place, no boom!", ephemeral: true);
                return;
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle("🦤 Pititi boombox of checkings!")
                .WithDescription($"Pititi count {landmines.Count} boom box(es) in here!")
                .WithColor(Color.Green)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter("YAYA!!");

            // Discord embeds allow at most 25 fields.
            const int maxFields = 25;
            foreach (var landmine in landmines.Take(maxFields))
            {
                embedBuilder.AddField(
                    $"💣 Boom box #{landmine.Id}",
                    $"Placed by **{landmine.PlacedByUsername}** <t:{landmine.PlacedAt.ToUnixTimeSeconds()}:R>\n" +
                    $"Messages passed: {landmine.MessagesElapsed} • Remaining: {landmine.RemainingMessages}");
            }

            if (landmines.Count > maxFields)
            {
                embedBuilder.WithFooter($"YAYA!! ...and {landmines.Count - maxFields} more boom box(es) Pititi no show here!");
            }

            await RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
        }
    }
}
