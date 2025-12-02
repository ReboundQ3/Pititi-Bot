using Discord;
using Discord.Interactions;

public class LandmineModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("landmine", "Pititi places a landmine in the chat for someone to stumble over")]
    public async Task HandleLandmineCommand([Choice("Place", "place"), Choice("Remove", "remove")] string action)
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
    }
}