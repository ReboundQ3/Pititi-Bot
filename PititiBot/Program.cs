using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

// Setup Dependency Injection
var services = new ServiceCollection()
    .AddSingleton(discordConfig)
    .AddSingleton<DiscordSocketClient>()
    .AddSingleton<InteractionService>()
    .AddSingleton(BotConfig.LandmineService)
    .AddSingleton(BotConfig.SS14StatusService)
    .BuildServiceProvider();

var _client = services.GetRequiredService<DiscordSocketClient>();
var _InteractionService = services.GetRequiredService<InteractionService>();

// Event handlers
_client.Log += Log;
_InteractionService.Log += Log;
_client.Ready += Ready;
_client.MessageReceived += BotConfig.LandmineService.HandleMessage; // Note to self: Clean landmine handling!
_client.InteractionCreated += HandleInteractionCreated;



// Login and start
await _client.LoginAsync(TokenType.Bot, token);
await _client.StartAsync();

// Start SS14 server monitoring in background
BotConfig.SS14StatusService.SetDiscordClient(_client);
_ = Task.Run(StartServerMonitoring);
Console.WriteLine("#> Pititi started watching Space Station 14 server!");

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

    await _InteractionService.AddModulesAsync(Assembly.GetEntryAssembly(), services);
    Console.WriteLine($"#> Loaded {_InteractionService.Modules.Count} modules");

    // Register commands globally (works on all servers)
    await _InteractionService.RegisterCommandsGloballyAsync();
    Console.WriteLine($"#> Registered {_InteractionService.SlashCommands.Count} global slash commands (may take up to 1 hour to propagate)");

    return;
}

async Task HandleInteractionCreated(SocketInteraction interaction)
{
    // Get guild name if available
    SocketGuild? guild = null;
    if (interaction.GuildId.HasValue)
    {
        guild = _client.GetGuild(interaction.GuildId.Value);
    }

    // Get command name
    string commandName = interaction.Type.ToString();
    if (interaction is SocketSlashCommand slashCommand)
    {
        commandName = slashCommand.Data.Name;
    }

    // Get options if available
    string options = "";
    if (interaction is SocketSlashCommand cmd && cmd.Data.Options.Count > 0)
    {
        var optionsList = new List<string>();
        foreach (var option in cmd.Data.Options)
        {
            optionsList.Add($"{option.Name}:{option.Value}");
        }
        options = " [" + string.Join(", ", optionsList) + "]";
    }

    Console.WriteLine($"#> Interaction received: {commandName}{options} from {guild} ({interaction.GuildId}) by {interaction.User.GlobalName} ({interaction.User.Id})");

    var context = new SocketInteractionContext(_client, interaction);
    var result = await _InteractionService.ExecuteCommandAsync(context, null);

    if (!result.IsSuccess)
    {
        Console.WriteLine($"#> Error executing command: {result.Error} - {result.ErrorReason}");
    }
}

async Task StartServerMonitoring()
{
    await BotConfig.SS14StatusService.StartMonitoringAsync(TimeSpan.FromMinutes(1));
}

// Configuration class accessible for modules
public static class BotConfig
{
    public static string Token { get; set; } = string.Empty;
    public static PititiBot.Services.LandmineService LandmineService { get; set; } = new();
    public static PititiBot.Services.SS14StatusService SS14StatusService { get; set; } = new();
}