using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using PititiBot.Models;

namespace PititiBot.Services;

public class LandmineService
{
    private readonly string _databasePath = Path.Combine("Databases", "landmines.db");
    private readonly string _connectionString;

    public LandmineService()
    {
        // Ensure Databases directory exists
        Directory.CreateDirectory("Databases");
        _connectionString = $"Data Source={_databasePath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            MigrateLegacySchema(connection);

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Landmines (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ChannelId INTEGER NOT NULL,
                    InitialCountdown INTEGER NOT NULL,
                    RemainingMessages INTEGER NOT NULL,
                    PlacedByUserId INTEGER,
                    PlacedByUsername TEXT,
                    PlacedAt TEXT NOT NULL
                )";
            command.ExecuteNonQuery();

            command.CommandText = "CREATE INDEX IF NOT EXISTS IX_Landmines_ChannelId ON Landmines (ChannelId)";
            command.ExecuteNonQuery();

            // Every detonation event gets logged here so Pititi can crown the
            // people who step on the most boom boxes.
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS LandmineHits (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    GuildId INTEGER NOT NULL,
                    ChannelId INTEGER NOT NULL,
                    UserId INTEGER NOT NULL,
                    Username TEXT,
                    MinesHit INTEGER NOT NULL,
                    HitAt TEXT NOT NULL
                )";
            command.ExecuteNonQuery();

            command.CommandText = "CREATE INDEX IF NOT EXISTS IX_LandmineHits_GuildId ON LandmineHits (GuildId)";
            command.ExecuteNonQuery();

            // Count existing landmines
            command.CommandText = "SELECT COUNT(*) FROM Landmines";
            var count = Convert.ToInt32(command.ExecuteScalar());

            if (count > 0)
            {
                Console.WriteLine($"#> Pititi remember {count} boom boxes from before!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't make boom box list! Error: {ex.Message}");
        }
    }

    // The old schema used ChannelId as PRIMARY KEY (one mine per channel) and had no
    // Id / PlacedAt columns. Detect that layout and drop it so the new multi-mine
    // schema can be created fresh.
    private static void MigrateLegacySchema(SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Landmines') WHERE name = 'Id'";
        var hasIdColumn = Convert.ToInt32(command.ExecuteScalar()) > 0;

        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'Landmines'";
        var tableExists = Convert.ToInt32(command.ExecuteScalar()) > 0;

        if (tableExists && !hasIdColumn)
        {
            command.CommandText = "DROP TABLE Landmines";
            command.ExecuteNonQuery();
            Console.WriteLine("#> Pititi throw away old boom box list to make room for MANY booms!");
        }
    }

    private static Landmine ReadLandmine(SqliteDataReader reader)
    {
        return new Landmine
        {
            Id = reader.GetInt64(0),
            ChannelId = (ulong)reader.GetInt64(1),
            InitialCountdown = reader.GetInt32(2),
            RemainingMessages = reader.GetInt32(3),
            PlacedByUserId = reader.IsDBNull(4) ? 0 : (ulong)reader.GetInt64(4),
            PlacedByUsername = reader.IsDBNull(5) ? "Unknown" : reader.GetString(5),
            PlacedAt = DateTimeOffset.TryParse(reader.IsDBNull(6) ? null : reader.GetString(6), out var placedAt)
                ? placedAt
                : DateTimeOffset.UtcNow
        };
    }

    private const string SelectColumns =
        "Id, ChannelId, InitialCountdown, RemainingMessages, PlacedByUserId, PlacedByUsername, PlacedAt";

    // Places multiple landmines in one transaction, each with its own random countdown
    // supplied by the caller. Returns the created landmines (with their new Ids).
    public List<Landmine> PlaceLandmines(ulong channelId, IReadOnlyList<int> countdowns, ulong userId, string username)
    {
        var placed = new List<Landmine>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            var insertCommand = connection.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = @"
                INSERT INTO Landmines (ChannelId, InitialCountdown, RemainingMessages, PlacedByUserId, PlacedByUsername, PlacedAt)
                VALUES ($channelId, $countdown, $countdown, $userId, $username, $placedAt);
                SELECT last_insert_rowid();";

            var channelParam = insertCommand.Parameters.Add("$channelId", SqliteType.Integer);
            var countdownParam = insertCommand.Parameters.Add("$countdown", SqliteType.Integer);
            var userParam = insertCommand.Parameters.Add("$userId", SqliteType.Integer);
            var usernameParam = insertCommand.Parameters.Add("$username", SqliteType.Text);
            var placedAtParam = insertCommand.Parameters.Add("$placedAt", SqliteType.Text);

            channelParam.Value = (long)channelId;
            userParam.Value = (long)userId;
            usernameParam.Value = username;

            foreach (var countdown in countdowns)
            {
                var placedAt = DateTimeOffset.UtcNow;
                countdownParam.Value = countdown;
                placedAtParam.Value = placedAt.ToString("o");

                var newId = Convert.ToInt64(insertCommand.ExecuteScalar());

                placed.Add(new Landmine
                {
                    Id = newId,
                    ChannelId = channelId,
                    InitialCountdown = countdown,
                    RemainingMessages = countdown,
                    PlacedByUserId = userId,
                    PlacedByUsername = username,
                    PlacedAt = placedAt
                });
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't place boom boxes! Error: {ex.Message}");
        }

        return placed;
    }

    public List<Landmine> GetLandmines(ulong channelId)
    {
        var landmines = new List<Landmine>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = $"SELECT {SelectColumns} FROM Landmines WHERE ChannelId = $channelId ORDER BY Id";
            command.Parameters.AddWithValue("$channelId", (long)channelId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                landmines.Add(ReadLandmine(reader));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't get boom box list! Error: {ex.Message}");
        }

        return landmines;
    }

    // Removes a single landmine by its Id. Returns the removed landmine, or null if
    // it didn't exist (or wasn't in this channel).
    public Landmine? RemoveLandmine(ulong channelId, long landmineId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = $"SELECT {SelectColumns} FROM Landmines WHERE Id = $id AND ChannelId = $channelId";
            selectCommand.Parameters.AddWithValue("$id", landmineId);
            selectCommand.Parameters.AddWithValue("$channelId", (long)channelId);

            Landmine? landmine = null;
            using (var reader = selectCommand.ExecuteReader())
            {
                if (reader.Read())
                    landmine = ReadLandmine(reader);
            }

            if (landmine == null)
                return null;

            var deleteCommand = connection.CreateCommand();
            deleteCommand.CommandText = "DELETE FROM Landmines WHERE Id = $id";
            deleteCommand.Parameters.AddWithValue("$id", landmineId);
            deleteCommand.ExecuteNonQuery();

            return landmine;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't remove boom boxsies! Error: {ex.Message}");
            return null;
        }
    }

    // Removes every landmine in the channel. Returns how many were cleared.
    public int ClearLandmines(ulong channelId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var deleteCommand = connection.CreateCommand();
            deleteCommand.CommandText = "DELETE FROM Landmines WHERE ChannelId = $channelId";
            deleteCommand.Parameters.AddWithValue("$channelId", (long)channelId);
            return deleteCommand.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't clear boom boxesies! Error: {ex.Message}");
            return 0;
        }
    }

    public class LeaderboardEntry
    {
        public ulong UserId { get; set; }
        public string Username { get; set; } = "Unknown";
        public int TotalMines { get; set; }
        public int TimesExploded { get; set; }
    }

    // Returns the top boom-box steppers for a guild, ranked by total mines hit
    // (so a MEGA BOOM counts for every mine in it), then by number of explosions.
    public List<LeaderboardEntry> GetLeaderboard(ulong guildId, int limit = 10)
    {
        var entries = new List<LeaderboardEntry>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT h.UserId,
                       SUM(h.MinesHit) AS TotalMines,
                       COUNT(*) AS TimesExploded,
                       (SELECT Username FROM LandmineHits
                        WHERE UserId = h.UserId AND GuildId = h.GuildId
                        ORDER BY Id DESC LIMIT 1) AS Username
                FROM LandmineHits h
                WHERE h.GuildId = $guildId
                GROUP BY h.UserId
                ORDER BY TotalMines DESC, TimesExploded DESC
                LIMIT $limit";
            command.Parameters.AddWithValue("$guildId", (long)guildId);
            command.Parameters.AddWithValue("$limit", limit);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                entries.Add(new LeaderboardEntry
                {
                    UserId = (ulong)reader.GetInt64(0),
                    TotalMines = reader.GetInt32(1),
                    TimesExploded = reader.GetInt32(2),
                    Username = reader.IsDBNull(3) ? "Unknown" : reader.GetString(3)
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't count the shiny hatsies! Error: {ex.Message}");
        }

        return entries;
    }

    public async Task HandleMessage(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        var channelId = message.Channel.Id;

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Tick every landmine in this channel down by one.
            var tickCommand = connection.CreateCommand();
            tickCommand.CommandText = "UPDATE Landmines SET RemainingMessages = RemainingMessages - 1 WHERE ChannelId = $channelId";
            tickCommand.Parameters.AddWithValue("$channelId", (long)channelId);
            var affected = tickCommand.ExecuteNonQuery();

            if (affected == 0)
                return; // No landmines in this channel

            // Find any that just reached zero (or below) — those go BOOM.
            var detonatedCount = 0;
            var countCommand = connection.CreateCommand();
            countCommand.CommandText = "SELECT COUNT(*) FROM Landmines WHERE ChannelId = $channelId AND RemainingMessages <= 0";
            countCommand.Parameters.AddWithValue("$channelId", (long)channelId);
            detonatedCount = Convert.ToInt32(countCommand.ExecuteScalar());

            if (detonatedCount == 0)
                return;

            // Clean up the detonated mines.
            var deleteCommand = connection.CreateCommand();
            deleteCommand.CommandText = "DELETE FROM Landmines WHERE ChannelId = $channelId AND RemainingMessages <= 0";
            deleteCommand.Parameters.AddWithValue("$channelId", (long)channelId);
            deleteCommand.ExecuteNonQuery();

            // Log who stepped on them so they can claim (or lose) the crown.
            var guildId = (message.Channel as IGuildChannel)?.GuildId ?? 0;
            var hitCommand = connection.CreateCommand();
            hitCommand.CommandText = @"
                INSERT INTO LandmineHits (GuildId, ChannelId, UserId, Username, MinesHit, HitAt)
                VALUES ($guildId, $channelId, $userId, $username, $minesHit, $hitAt)";
            hitCommand.Parameters.AddWithValue("$guildId", (long)guildId);
            hitCommand.Parameters.AddWithValue("$channelId", (long)channelId);
            hitCommand.Parameters.AddWithValue("$userId", (long)message.Author.Id);
            hitCommand.Parameters.AddWithValue("$username", message.Author.GlobalName ?? message.Author.Username);
            hitCommand.Parameters.AddWithValue("$minesHit", detonatedCount);
            hitCommand.Parameters.AddWithValue("$hitAt", DateTimeOffset.UtcNow.ToString("o"));
            hitCommand.ExecuteNonQuery();

            if (detonatedCount == 1)
            {
                await message.Channel.SendMessageAsync($"💥 **BOOM!!** 💥\n{message.Author.Mention} STEPPED ON PITITI'S BOOM BOX!! IT GO BOOM!");
            }
            else
            {
                await message.Channel.SendMessageAsync($"💥💥 **MEGA BOOM!!** 💥💥\n{message.Author.Mention} STEPPED ON {detonatedCount} PITITI BOOM BOXES AT ONCE!! BIG BIG BOOM!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't handle message for boom box! Error: {ex.Message}");
        }
    }
}
