using Microsoft.AspNetCore.SignalR;
using SignalRSample.Hubs;

namespace SignalRSample.Tests.Hubs;

public class OrderHubTests
{
    [Test]
    public void OrderHub_IsASignalRHub()
    {
        var hub = new OrderHub();

        Assert.That(hub, Is.InstanceOf<Hub>());
    }
}
