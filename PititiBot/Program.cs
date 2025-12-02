using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using System.Reflection;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var token = configuration["DISCORD_TOKEN"] ?? configuration["Discord:Token"];

// Set the config value
BotConfig.Token = token!;

// Configure the Discord client
var discordConfig = new DiscordSocketConfig
{
    DefaultRetryMode = RetryMode.AlwaysRetry,
    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
};

var client = new DiscordSocketClient(discordConfig);
var InteractionService = new InteractionService(client);

// Event handlers
client.Log += Log;
InteractionService.Log += Log;
client.Ready += Ready;
client.MessageReceived += BotConfig.LandmineService.HandleMessage; // Note to self: Clean landmine handling!
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
    Console.WriteLine($"Loaded {InteractionService.Modules.Count} modules");

    // Register commands globally (works on all servers)
    await InteractionService.RegisterCommandsGloballyAsync();
    Console.WriteLine($"Registered {InteractionService.SlashCommands.Count} global slash commands (may take up to 1 hour to propagate)");

    return;
}

// Configuration class accessible for modules
public static class BotConfig
{
    public static string Token { get; set; } = string.Empty;
    public static PititiBot.Services.LandmineService LandmineService { get; set; } = new();
}