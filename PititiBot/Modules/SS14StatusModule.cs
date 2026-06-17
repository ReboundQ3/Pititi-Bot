using Discord;
using Discord.Interactions;

namespace PititiBot.Modules;

public class SS14StatusModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("server", "Pititi checks Space Station 14 server status")]
    public async Task HandleServerCommand(
        [Choice("Status", "status"), Choice("Subscribe", "subscribe"), Choice("Unsubscribe", "unsubscribe"), Choice("Config", "config")] string action,
        [Summary("url", "SS14 server status URL (for config action)")] string? url = null)
    {
        var guildId = Context.Guild?.Id;

        if (guildId == null)
        {
            await RespondAsync("❌ PITITI ONLY WORK IN SERVERS!! Not in DMs!", ephemeral: true);
            return;
        }

        if (action == "config")
        {
            // Check for admin permission
            var guildUser = Context.User as Discord.WebSocket.SocketGuildUser;
            if (guildUser == null || !guildUser.GuildPermissions.Administrator)
            {
                await RespondAsync("❌ ONLY BIG BOSS CAN SET UP SERVER!! Pititi say no!", ephemeral: true);
                return;
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                await RespondAsync("❌ PITITI NEED URL!! Use like: `/server action:config url:https://your-server.com/status`", ephemeral: true);
                return;
            }

            // Validate URL ends with /status
            if (!url.EndsWith("/status"))
            {
                await RespondAsync("⚠️ URL SHOULD END WITH `/status`!! Pititi will try anyway...", ephemeral: true);
            }

            bool success = BotConfig.SS14StatusService.SetGuildServer(guildId.Value, url);

            if (success)
            {
                // Try to fetch status to verify
                await DeferAsync(ephemeral: true);
                var testStatus = await BotConfig.SS14StatusService.GetServerStatusAsync(url);

                if (testStatus != null)
                {
                    await FollowupAsync($"✅ **SERVER ALL SET UP!!**\nPititi now watching: **{testStatus.name}**\nURL: {url}", ephemeral: true);
                }
                else
                {
                    await FollowupAsync($"⚠️ **SERVER SAVED BUT PITITI CAN'T REACH IT!!**\nURL saved: {url}\nMaybe server is down or URL is wrong?", ephemeral: true);
                }
            }
            else
            {
                await RespondAsync("❌ PITITI CAN'T SAVE SERVER!! URL look wrong? Must start with http:// or https://", ephemeral: true);
            }
            return;
        }

        // Get guild's configured server URL
        var serverUrl = BotConfig.SS14StatusService.GetGuildServerUrl(guildId.Value);

        if (serverUrl == null)
        {
            await RespondAsync("❌ **NO SERVER SET UP!!**\nPititi don't know which server to watch!\n\nBig boss can set up with:\n`/server action:config url:https://your-server.com/status`", ephemeral: true);
            return;
        }

        if (action == "status")
        {
            await DeferAsync(); // This might take a moment

            var status = await BotConfig.SS14StatusService.GetServerStatusAsync(serverUrl);

            if (status == null)
            {
                await FollowupAsync($"😢 PITITI CAN'T SEE SERVER!! Maybe server is sleeping?\nServer URL: {serverUrl}", ephemeral: true);
                return;
            }

            // Determine run level status
            string runLevelText = GetRunLevelText(status.run_level);

            var embed = new EmbedBuilder()
                .WithTitle("🚀 SPACE STATION 14 SERVER STATUS!!")
                .WithDescription($"🦤 Pititi is watching **{status.name}**!")
                .WithColor(status.run_level == 1 ? Color.Green : Color.Orange)
                .AddField("👥 Players", $"{status.players}/{status.soft_max_players}", inline: true)
                .AddField("🎮 Round", $"#{status.round_id}", inline: true)
                .AddField("📊 Status", runLevelText, inline: true)
                .AddField("🗺️ Map", status.map ?? "Unknown", inline: true)
                .AddField("🎯 Preset", status.preset, inline: true)
                .AddField("🛡️ Panic Bunker", status.panic_bunker ? "ON" : "OFF", inline: true)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter("Pititi is of helpings!!")
                .Build();

            await FollowupAsync(embed: embed);
        }
        else if (action == "subscribe")
        {
            var channelId = Context.Channel.Id;

            if (BotConfig.SS14StatusService.IsChannelSubscribed(channelId))
            {
                await RespondAsync("📢 PITITI ALREADY WATCHING THIS CHANNEL!! Is already telling you about server!", ephemeral: true);
                return;
            }

            bool success = BotConfig.SS14StatusService.SubscribeChannel(channelId, guildId.Value);

            if (success)
            {
                await RespondAsync($"✅ **PITITI NOW WATCHING!!**\nPititi will tell you when:\n• Round starts 🚀\n• Round ends 🏁\n• New round begins 🔄\n\nWatching server: {serverUrl}", ephemeral: true);
            }
            else
            {
                await RespondAsync("❌ PITITI CAN'T START WATCHING!! Something went wrong...", ephemeral: true);
            }
        }
        else if (action == "unsubscribe")
        {
            var channelId = Context.Channel.Id;

            if (!BotConfig.SS14StatusService.IsChannelSubscribed(channelId))
            {
                await RespondAsync("📢 PITITI NOT WATCHING THIS CHANNEL!! Nothing to stop!", ephemeral: true);
                return;
            }

            bool success = BotConfig.SS14StatusService.UnsubscribeChannel(channelId);

            if (success)
            {
                await RespondAsync("👋 **PITITI STOP WATCHING!!**\nPititi won't tell you about server anymore. Is sad...", ephemeral: true);
            }
            else
            {
                await RespondAsync("❌ PITITI CAN'T STOP WATCHING!! Something went wrong...", ephemeral: true);
            }
        }
    }

    private static string GetRunLevelText(int runLevel)
    {
        if (runLevel == 0)
            return "🔴 **LOBBY** - Waiting for round start!";

        if (runLevel == 1)
            return "🟢 **PLAYING** - Round is HAPPENING!!";

        if (runLevel == 2)
            return "🟡 **PAUSED** - Round is taking break!";

        return $"⚪ **UNKNOWN** - Pititi confused about {runLevel}?";
    }
}
