namespace CommunityToolkit.Aspire.Messages;

public class ShipOrder : ICommand
{
    public string? OrderId { get; set; }
}