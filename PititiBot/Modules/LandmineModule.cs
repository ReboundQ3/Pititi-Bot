using Discord;
using Discord.Interactions;

public class LandmineModule : InteractionModuleBase<SocketInteractionContext>
{
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

            bool success = BotConfig.LandmineService.PlaceLandmine(channelId, countdown);

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
            bool hasLandmine = BotConfig.LandmineService.GetLandmineStatus(channelId, out int initial, out int remaining);

            if (!hasLandmine)
            {
                await RespondAsync("NO BOOM BOX HERE! Is safe place, no boom!", ephemeral: true);
                return;
            }

            var messagesElapsed = initial - remaining;
            await RespondAsync($"🔍 PITITI CHECK BOOM BOX!!\n" +
                             $"📋 Started at: **{initial}** messages\n" +
                             $"💬 Messages passed: **{messagesElapsed}**\n" +
                             $"⏱️ Remaining: **{remaining}** messages\n" +
                             $"💣 Boom is coming... shhhh!", ephemeral: true);
        }
    }
}