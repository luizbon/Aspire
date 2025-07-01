namespace CommunityToolkit.Aspire.Messages;

public class OrderPlaced : IEvent
{
    public string? OrderId { get; set; }
}
