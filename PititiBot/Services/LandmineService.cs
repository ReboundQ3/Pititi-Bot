using Discord.WebSocket;

namespace PititiBot.Services;

public class LandmineService
{
    private readonly Dictionary<ulong, int> _landmines = new();

    public bool PlaceLandmine(ulong channelId, int countdown)
    {
        if (_landmines.ContainsKey(channelId))
            return false; // Already has a landmine

        _landmines[channelId] = countdown;
        return true;
    }

    public bool RemoveLandmine(ulong channelId, out int remainingMessages)
    {
        if (_landmines.TryGetValue(channelId, out remainingMessages))
        {
            _landmines.Remove(channelId);
            return true;
        }

        remainingMessages = 0;
        return false;
    }

    public bool HasLandmine(ulong channelId)
    {
        return _landmines.ContainsKey(channelId);
    }

    public int GetRemainingMessages(ulong channelId)
    {
        return _landmines.TryGetValue(channelId, out var count) ? count : 0;
    }

    public async Task HandleMessage(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        var channelId = message.Channel.Id;

        if (_landmines.ContainsKey(channelId))
        {
            _landmines[channelId]--;

            if (_landmines[channelId] <= 0)
            {
                // BOOM!
                await message.Channel.SendMessageAsync("💥 **BOOM!!** 💥\nPITITI'S BOOM BOX GO BOOM!");
                _landmines.Remove(channelId);
            }
        }
    }
}
