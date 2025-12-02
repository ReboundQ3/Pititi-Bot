using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using System.Reflection;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
    
var token = configuration["Discord:Token"];
var guildId = configuration["Discord:GuildId"];
ulong guildIdUlong = ulong.Parse(guildId);

// Set the config values
BotConfig.Token = token!;
BotConfig.GuildId = guildIdUlong;

// Configure the Discord client
var discordConfig = new DiscordSocketConfig
{
    DefaultRetryMode = RetryMode.AlwaysRetry,
    GatewayIntents = GatewayIntents.Guilds // Only use intents we need
};

var client = new DiscordSocketClient(discordConfig);
var InteractionService = new InteractionService(client);

// Event handlers
client.Log += Log;
InteractionService.Log += Log;
client.Ready += Ready;
client.InteractionCreated += async interaction =>
{
    Console.WriteLine($"Interaction received: {interaction.Type}");
    var context = new SocketInteractionContext(client, interaction);
    var result = await InteractionService.ExecuteCommandAsync(context, null);

    if (!result.IsSuccess)
    {
        Console.WriteLine($"Error executing command: {result.Error} - {result.ErrorReason}");
    }
};

// Login and start
await client.LoginAsync(TokenType.Bot, token);
await client.StartAsync();

// Keep running
await Task.Delay(-1);

// Event handler methods
Task Log(LogMessage msg)
{
    Console.WriteLine(msg.ToString());
    return Task.CompletedTask;
}

async Task Ready()
{
    Console.WriteLine($"Is of CONNECTINGS YAYA! ({client.CurrentUser})");

    await InteractionService.AddModulesAsync(Assembly.GetEntryAssembly(), null);
    Console.WriteLine($"Loaded {InteractionService.Modules.Count()} modules");

    await InteractionService.RegisterCommandsToGuildAsync(guildIdUlong);
    Console.WriteLine($"Registered {InteractionService.SlashCommands.Count()} slash commands");

    return;
}

// Configuration class accessible for modules
public static class BotConfig
{
    public static string Token { get; set; } = string.Empty;
    public static ulong GuildId { get; set; }
}