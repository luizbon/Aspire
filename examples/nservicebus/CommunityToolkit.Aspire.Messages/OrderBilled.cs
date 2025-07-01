namespace CommunityToolkit.Aspire.Messages;

public class OrderBilled : IEvent
{
    public string? OrderId { get; set; }
}
