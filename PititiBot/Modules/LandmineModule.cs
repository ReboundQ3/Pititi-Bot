using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.VisualBasic;

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
    public async Task HandleLandmineCommand([Choice("Place", "place"), Choice("Remove", "remove"), Choice("Status", "status")] string action)
    {
        var channelId = Context.Channel.Id;

        if (action == "place")
        {
            // Random number between 1 and 250
            var random = new Random();
            var countdown = random.Next(1, 250);

            bool success = BotConfig.LandmineService.PlaceLandmine(channelId, countdown, Context.User.Id, Context.User.GlobalName ?? Context.User.Username);

            if (!success)
            {
                await RespondAsync("PITITI ALREADY PUT BOOM BOX HERE! Only one boom per place!", ephemeral: true);
                return;
            }

            await RespondAsync($"PITITI PLACE BOOM BOX!! 💣 Will go BOOM in.. Shhhh Gorb say is secret");
        }
        else if (action == "remove")
        {
            bool success = BotConfig.LandmineService.RemoveLandmine(channelId, out int remaining);

            if (!success)
            {
                await RespondAsync("NO BOOM BOX HERE! Pititi already took it or never put it!", ephemeral: true);
                return;
            }

            await RespondAsync($"PITITI TAKE BOOM BOX AWAY! 🧹 Was gonna boom in {remaining} messages. Is safe now!");
        }
        else if (action == "status")
        {
            bool hasLandmine = BotConfig.LandmineService.GetLandmineStatus(channelId, out int initial, out int remaining, out string placedByUsername);

            if (!hasLandmine)
            {
                await RespondAsync("NO BOOM BOX HERE! Is safe place, no boom!", ephemeral: true);
                return;
            }

            var messagesElapsed = initial - remaining;

            var embedBuilder = new EmbedBuilder()
                .WithTitle("Pititi boombox of checkings!")
                .WithDescription("Pititi will check the landmine status")
                .AddField($"Placed down by", placedByUsername)
                .AddField($"Placed down at", DateTimeOffset.UtcNow)
                .AddField($"Messages passed:", messagesElapsed)
                .AddField($"Remaining:", remaining)
                .WithColor(Color.Green)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter("YAYA!!");

            var embed = embedBuilder.Build();

            await RespondAsync(embed: embed, ephemeral: true);
        }
    }
}