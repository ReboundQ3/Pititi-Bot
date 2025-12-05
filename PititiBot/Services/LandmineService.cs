using Discord.WebSocket;
using Microsoft.Data.Sqlite;

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

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Landmines (
                    ChannelId INTEGER PRIMARY KEY,
                    InitialCountdown INTEGER NOT NULL,
                    RemainingMessages INTEGER NOT NULL,
                    PlacedByUserId INTEGER,
                    PlacedByUsername TEXT
                )";
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

    public bool PlaceLandmine(ulong channelId, int countdown, ulong userId, string username)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Check if landmine already exists
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM Landmines WHERE ChannelId = $channelId";
            checkCommand.Parameters.AddWithValue("$channelId", (long)channelId);
            var exists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

            if (exists)
                return false;

            // Insert new landmine
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO Landmines (ChannelId, InitialCountdown, RemainingMessages, PlacedByUserId, PlacedByUsername)
                VALUES ($channelId, $countdown, $countdown, $userId, $username)";
            insertCommand.Parameters.AddWithValue("$channelId", (long)channelId);
            insertCommand.Parameters.AddWithValue("$countdown", countdown);
            insertCommand.Parameters.AddWithValue("$userId", (long)userId);
            insertCommand.Parameters.AddWithValue("$username", username);
            insertCommand.ExecuteNonQuery();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't place boom box! Error: {ex.Message}");
            return false;
        }
    }

    public bool RemoveLandmine(ulong channelId, out int remainingMessages)
    {
        remainingMessages = 0;

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Get remaining messages before deleting
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "SELECT RemainingMessages FROM Landmines WHERE ChannelId = $channelId";
            selectCommand.Parameters.AddWithValue("$channelId", (long)channelId);
            var result = selectCommand.ExecuteScalar();

            if (result == null)
                return false;

            remainingMessages = Convert.ToInt32(result);

            // Delete the landmine
            var deleteCommand = connection.CreateCommand();
            deleteCommand.CommandText = "DELETE FROM Landmines WHERE ChannelId = $channelId";
            deleteCommand.Parameters.AddWithValue("$channelId", (long)channelId);
            deleteCommand.ExecuteNonQuery();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't remove boom box! Error: {ex.Message}");
            return false;
        }
    }

    public bool HasLandmine(ulong channelId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Check if landmine exists and has valid remaining messages
            var command = connection.CreateCommand();
            command.CommandText = "SELECT RemainingMessages FROM Landmines WHERE ChannelId = $channelId";
            command.Parameters.AddWithValue("$channelId", (long)channelId);
            var result = command.ExecuteScalar();

            if (result == null)
                return false;

            var remaining = Convert.ToInt32(result);

            // Clean up ghost landmines
            if (remaining <= 0)
            {
                var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = "DELETE FROM Landmines WHERE ChannelId = $channelId";
                deleteCommand.Parameters.AddWithValue("$channelId", (long)channelId);
                deleteCommand.ExecuteNonQuery();
                Console.WriteLine($"#> Pititi clean up ghost boom box in channel {channelId}!");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't check boom box! Error: {ex.Message}");
            return false;
        }
    }

    public int GetRemainingMessages(ulong channelId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT RemainingMessages FROM Landmines WHERE ChannelId = $channelId";
            command.Parameters.AddWithValue("$channelId", (long)channelId);
            var result = command.ExecuteScalar();

            if (result != null)
                return Convert.ToInt32(result);

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't count boom box messages! Error: {ex.Message}");
            return 0;
        }
    }

    public bool GetLandmineStatus(ulong channelId, out int initialCountdown, out int remainingMessages, out string placedByUsername)
    {
        initialCountdown = 0;
        remainingMessages = 0;
        placedByUsername = string.Empty;

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT InitialCountdown, RemainingMessages, PlacedByUsername FROM Landmines WHERE ChannelId = $channelId";
            command.Parameters.AddWithValue("$channelId", (long)channelId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                initialCountdown = reader.GetInt32(0);
                remainingMessages = reader.GetInt32(1);
                placedByUsername = reader.IsDBNull(2) ? "Unknown" : reader.GetString(2);

                // Clean up ghost landmines that should have exploded already
                if (remainingMessages <= 0)
                {
                    reader.Close();
                    var deleteCommand = connection.CreateCommand();
                    deleteCommand.CommandText = "DELETE FROM Landmines WHERE ChannelId = $channelId";
                    deleteCommand.Parameters.AddWithValue("$channelId", (long)channelId);
                    deleteCommand.ExecuteNonQuery();
                    Console.WriteLine($"#> Pititi clean up ghost boom box in channel {channelId}!");
                    return false;
                }

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't get boom box status! Error: {ex.Message}");
            return false;
        }
    }

    public async Task HandleMessage(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        var channelId = message.Channel.Id;

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Get current remaining messages
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "SELECT RemainingMessages FROM Landmines WHERE ChannelId = $channelId";
            selectCommand.Parameters.AddWithValue("$channelId", (long)channelId);
            var result = selectCommand.ExecuteScalar();

            if (result == null)
                return; // No landmine in this channel

            var remaining = Convert.ToInt32(result) - 1;

            if (remaining <= 0)
            {
                // Delete the landmine FIRST to prevent race conditions
                var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = "DELETE FROM Landmines WHERE ChannelId = $channelId";
                deleteCommand.Parameters.AddWithValue("$channelId", (long)channelId);
                var rowsDeleted = deleteCommand.ExecuteNonQuery();

                // Only send BOOM message if we actually deleted a landmine
                if (rowsDeleted > 0)
                {
                    await message.Channel.SendMessageAsync($"💥 **BOOM!!** 💥\n{message.Author.Mention} STEPPED ON PITITI'S BOOM BOX!! IT GO BOOM!");
                }
            }
            else
            {
                // Update remaining messages
                var updateCommand = connection.CreateCommand();
                updateCommand.CommandText = "UPDATE Landmines SET RemainingMessages = $remaining WHERE ChannelId = $channelId";
                updateCommand.Parameters.AddWithValue("$remaining", remaining);
                updateCommand.Parameters.AddWithValue("$channelId", (long)channelId);
                updateCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Pititi can't handle message for boom box! Error: {ex.Message}");
        }
    }
}
