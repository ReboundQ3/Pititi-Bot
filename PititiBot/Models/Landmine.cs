namespace PititiBot.Models;

public class Landmine
{
    public long Id { get; set; }
    public ulong ChannelId { get; set; }
    public int InitialCountdown { get; set; }
    public int RemainingMessages { get; set; }
    public ulong PlacedByUserId { get; set; }
    public string PlacedByUsername { get; set; } = "Unknown";
    public DateTimeOffset PlacedAt { get; set; }

    public int MessagesElapsed => InitialCountdown - RemainingMessages;
}
