using CommunityToolkit.Aspire.Messages;

namespace CommunityToolkit.Aspire.Shipping;

class ShipOrderHandler(ILogger<ShipOrderHandler> log) : IHandleMessages<ShipOrder>
{
    public Task Handle(ShipOrder message, IMessageHandlerContext context)
    {
        log.LogInformation($"Order [{message.OrderId}] - Successfully shipped.");
        return Task.CompletedTask;
    }
}
