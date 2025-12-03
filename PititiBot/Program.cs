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

var _client = new DiscordSocketClient(discordConfig);
var _InteractionService = new InteractionService(_client);

// Event handlers
_client.Log += Log;
_InteractionService.Log += Log;
_client.Ready += Ready;
_client.MessageReceived += BotConfig.LandmineService.HandleMessage; // Note to self: Clean landmine handling!
_client.InteractionCreated += async interaction =>
{
    var guildname = interaction.GuildId.HasValue ? _client.GetGuild(interaction.GuildId.Value) : null;
    var commandName = interaction is SocketSlashCommand slashCommand ? slashCommand.Data.Name : interaction.Type.ToString();
    var options = interaction is SocketSlashCommand cmd && cmd.Data.Options.Any()
        ? " [" + string.Join(", ", cmd.Data.Options.Select(o => $"{o.Name}:{o.Value}")) + "]"
        : "";
    Console.WriteLine($"#> Interaction received: {commandName}{options} from {guildname} ({interaction.GuildId}) by {interaction.User.GlobalName} ({interaction.User.Id})");
    var context = new SocketInteractionContext(_client, interaction);
    var result = await _InteractionService.ExecuteCommandAsync(context, null);

    if (!result.IsSuccess)
    {
        Console.WriteLine($"#> Error executing command: {result.Error} - {result.ErrorReason}");
    }
};



// Login and start
await _client.LoginAsync(TokenType.Bot, token);
await _client.StartAsync();

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
    Console.WriteLine($"#> Is of CONNECTINGS YAYA! ({_client.CurrentUser})");

    await _InteractionService.AddModulesAsync(Assembly.GetEntryAssembly(), null);
    Console.WriteLine($"#> Loaded {_InteractionService.Modules.Count} modules");

    // Register commands globally (works on all servers)
    await _InteractionService.RegisterCommandsGloballyAsync();
    Console.WriteLine($"#> Registered {_InteractionService.SlashCommands.Count} global slash commands (may take up to 1 hour to propagate)");

    return;
}

// Configuration class accessible for modules
public static class BotConfig
{
    public static string Token { get; set; } = string.Empty;
    public static PititiBot.Services.LandmineService LandmineService { get; set; } = new();
}