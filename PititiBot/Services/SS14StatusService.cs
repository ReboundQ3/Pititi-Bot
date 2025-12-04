using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace PititiBot.Services;

public class SS14StatusService
{
    private readonly string _databasePath = Path.Combine("Databases", "ss14status.db");
    private readonly string _connectionString;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, ServerStatus?> _lastStatusByUrl = new();
    private DiscordSocketClient? _client;

    public class ServerStatus
    {
        public string name { get; set; } = string.Empty;
        public int players { get; set; }
        public List<string> tags { get; set; } = new();
        public string? map { get; set; }
        public int round_id { get; set; }
        public int soft_max_players { get; set; }
        public bool panic_bunker { get; set; }
        public int run_level { get; set; }
        public string preset { get; set; } = string.Empty;
    }

    public SS14StatusService()
    {
        Directory.CreateDirectory("Databases");
        _connectionString = $"Data Source={_databasePath}";
        _httpClient = new HttpClient();
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS ServerStatus (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp INTEGER NOT NULL,
                    ServerUrl TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Players INTEGER NOT NULL,
                    RoundId INTEGER NOT NULL,
                    Map TEXT,
                    Preset TEXT NOT NULL,
                    RunLevel INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS GuildServers (
                    GuildId INTEGER PRIMARY KEY,
                    ServerUrl TEXT NOT NULL,
                    ServerName TEXT
                );

                CREATE TABLE IF NOT EXISTS ChannelSubscriptions (
                    ChannelId INTEGER PRIMARY KEY,
                    GuildId INTEGER NOT NULL,
                    NotifyRoundStart INTEGER DEFAULT 1,
                    NotifyRoundEnd INTEGER DEFAULT 1,
                    NotifyPlayerCount INTEGER DEFAULT 0
                )";
            command.ExecuteNonQuery();

            Console.WriteLine("#> Pititi ready to watch Space Station 14 servers!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't make server watching list! Error: {ex.Message}");
        }
    }

    public void SetDiscordClient(DiscordSocketClient client)
    {
        _client = client;
    }

    public bool SetGuildServer(ulong guildId, string serverUrl, string? serverName = null)
    {
        try
        {
            // Validate URL format
            if (!serverUrl.StartsWith("http://") && !serverUrl.StartsWith("https://"))
            {
                return false;
            }

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO GuildServers (GuildId, ServerUrl, ServerName)
                VALUES ($guildId, $serverUrl, $serverName)";

            command.Parameters.AddWithValue("$guildId", (long)guildId);
            command.Parameters.AddWithValue("$serverUrl", serverUrl);
            command.Parameters.AddWithValue("$serverName", serverName ?? (object)DBNull.Value);
            command.ExecuteNonQuery();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't set guild server! Error: {ex.Message}");
            return false;
        }
    }

    public string? GetGuildServerUrl(ulong guildId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT ServerUrl FROM GuildServers WHERE GuildId = $guildId";
            command.Parameters.AddWithValue("$guildId", (long)guildId);

            var result = command.ExecuteScalar();
            return result?.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't get guild server! Error: {ex.Message}");
            return null;
        }
    }

    public async Task<ServerStatus?> GetServerStatusAsync(string serverUrl)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(serverUrl);
            var status = JsonSerializer.Deserialize<ServerStatus>(response);

            if (status != null)
            {
                // Save to database
                SaveStatusToDatabase(serverUrl, status);
            }

            return status;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't check server at {serverUrl}! Error: {ex.Message}");
            return null;
        }
    }

    private void SaveStatusToDatabase(string serverUrl, ServerStatus status)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO ServerStatus (Timestamp, ServerUrl, Name, Players, RoundId, Map, Preset, RunLevel)
                VALUES ($timestamp, $serverUrl, $name, $players, $roundId, $map, $preset, $runLevel)";

            command.Parameters.AddWithValue("$timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            command.Parameters.AddWithValue("$serverUrl", serverUrl);
            command.Parameters.AddWithValue("$name", status.name);
            command.Parameters.AddWithValue("$players", status.players);
            command.Parameters.AddWithValue("$roundId", status.round_id);
            command.Parameters.AddWithValue("$map", status.map ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$preset", status.preset);
            command.Parameters.AddWithValue("$runLevel", status.run_level);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't save server status! Error: {ex.Message}");
        }
    }

    public async Task StartMonitoringAsync(TimeSpan interval)
    {
        while (true)
        {
            try
            {
                // Get all unique server URLs from guilds
                var serverUrls = GetAllMonitoredServerUrls();

                foreach (var serverUrl in serverUrls)
                {
                    var currentStatus = await GetServerStatusAsync(serverUrl);

                    if (ShouldCheckForNotifications(currentStatus, serverUrl))
                    {
                        var lastStatus = _lastStatusByUrl[serverUrl];
                        await CheckForNotifications(serverUrl, currentStatus!, lastStatus!);
                    }

                    _lastStatusByUrl[serverUrl] = currentStatus;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"#> Pititi monitoring error! Error: {ex.Message}");
            }

            await Task.Delay(interval);
        }
    }

    private List<string> GetAllMonitoredServerUrls()
    {
        var urls = new List<string>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT DISTINCT gs.ServerUrl
                FROM GuildServers gs
                INNER JOIN ChannelSubscriptions cs ON gs.GuildId = cs.GuildId";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                urls.Add(reader.GetString(0));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't get monitored servers! Error: {ex.Message}");
        }

        return urls;
    }

    private async Task CheckForNotifications(string serverUrl, ServerStatus current, ServerStatus previous)
    {
        if (_client == null) return;

        // Round started (run_level changed from 0 to 1+)
        if (previous.run_level == 0 && current.run_level > 0)
        {
            await NotifySubscribedChannels(serverUrl, $"🚀 **ROUND START!!**\nPititi see round #{current.round_id} is STARTING!! Map is {current.map ?? "unknown"}! Is time for SPACE ADVENTURES!!", notifyRoundStart: true);
        }

        // Round ended (run_level changed from 1+ to 0)
        if (previous.run_level > 0 && current.run_level == 0)
        {
            await NotifySubscribedChannels(serverUrl, $"🏁 **ROUND END!!**\nPititi see round #{previous.round_id} is DONE!! Was good round? Pititi hope so!", notifyRoundEnd: true);
        }

        // New round started (round_id changed)
        if (current.round_id != previous.round_id)
        {
            await NotifySubscribedChannels(serverUrl, $"🔄 **NEW ROUND!!**\nPititi see new round #{current.round_id} is here! Preset: {current.preset}", notifyRoundStart: true);
        }
    }

    private async Task NotifySubscribedChannels(string serverUrl, string message, bool notifyRoundStart = false, bool notifyRoundEnd = false)
    {
        if (_client == null) return;

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT cs.ChannelId, cs.NotifyRoundStart, cs.NotifyRoundEnd
                FROM ChannelSubscriptions cs
                INNER JOIN GuildServers gs ON cs.GuildId = gs.GuildId
                WHERE gs.ServerUrl = $serverUrl";

            command.Parameters.AddWithValue("$serverUrl", serverUrl);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var channelId = (ulong)reader.GetInt64(0);
                var wantsRoundStart = reader.GetInt32(1) == 1;
                var wantsRoundEnd = reader.GetInt32(2) == 1;

                // Check if this channel wants this type of notification
                bool shouldNotify = (notifyRoundStart && wantsRoundStart) || (notifyRoundEnd && wantsRoundEnd);

                if (shouldNotify)
                {
                    await SendMessageToChannel(channelId, message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't notify channels! Error: {ex.Message}");
        }
    }

    public bool SubscribeChannel(ulong channelId, ulong guildId, bool roundStart = true, bool roundEnd = true)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO ChannelSubscriptions (ChannelId, GuildId, NotifyRoundStart, NotifyRoundEnd)
                VALUES ($channelId, $guildId, $roundStart, $roundEnd)";

            command.Parameters.AddWithValue("$channelId", (long)channelId);
            command.Parameters.AddWithValue("$guildId", (long)guildId);
            command.Parameters.AddWithValue("$roundStart", roundStart ? 1 : 0);
            command.Parameters.AddWithValue("$roundEnd", roundEnd ? 1 : 0);
            command.ExecuteNonQuery();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't subscribe channel! Error: {ex.Message}");
            return false;
        }
    }

    public bool UnsubscribeChannel(ulong channelId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM ChannelSubscriptions WHERE ChannelId = $channelId";
            command.Parameters.AddWithValue("$channelId", (long)channelId);
            var rowsAffected = command.ExecuteNonQuery();

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't unsubscribe channel! Error: {ex.Message}");
            return false;
        }
    }

    public bool IsChannelSubscribed(ulong channelId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM ChannelSubscriptions WHERE ChannelId = $channelId";
            command.Parameters.AddWithValue("$channelId", (long)channelId);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't check subscription! Error: {ex.Message}");
            return false;
        }
    }

    private bool ShouldCheckForNotifications(ServerStatus? currentStatus, string serverUrl)
    {
        if (currentStatus == null)
            return false;

        if (_client == null)
            return false;

        if (!_lastStatusByUrl.TryGetValue(serverUrl, out var lastStatus))
            return false;

        if (lastStatus == null)
            return false;

        return true;
    }

    private async Task SendMessageToChannel(ulong channelId, string message)
    {
        try
        {
            if (_client?.GetChannel(channelId) is Discord.IMessageChannel channel)
            {
                await channel.SendMessageAsync(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't send to channel {channelId}! Error: {ex.Message}");
        }
    }
}
