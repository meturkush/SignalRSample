using SignalRSample.Hubs;
using SignalRSample.Tests.Helpers;

namespace SignalRSample.Tests.Hubs;

public class UserHubTests
{
    private HubTestDoubles _doubles = null!;
    private UserHub _hub = null!;

    [SetUp]
    public void SetUp()
    {
        UserHub.TotalUsers = 0;
        UserHub.TotalViews = 0;
        _doubles = new HubTestDoubles();
        _hub = new UserHub();
        _doubles.Attach(_hub);
    }

    [Test]
    public async Task OnConnectedAsync_IncrementsTotalUsersAndNotifiesClients()
    {
        await _hub.OnConnectedAsync();

        Assert.That(UserHub.TotalUsers, Is.EqualTo(1));
        HubTestDoubles.VerifySend(_doubles.All, "updateTotalUsers", 1);
    }

    [Test]
    public async Task OnDisconnectedAsync_DecrementsTotalUsersAndNotifiesClients()
    {
        UserHub.TotalUsers = 4;

        await _hub.OnDisconnectedAsync(null);

        Assert.That(UserHub.TotalUsers, Is.EqualTo(3));
        HubTestDoubles.VerifySend(_doubles.All, "updateTotalUsers", 3);
    }

    [Test]
    public async Task NewWindowLoaded_IncrementsTotalViewsNotifiesClientsAndReturnsSummary()
    {
        var result = await _hub.NewWindowLoaded("harry");

        Assert.That(UserHub.TotalViews, Is.EqualTo(1));
        Assert.That(result, Is.EqualTo("total views from harry - 1"));
        HubTestDoubles.VerifySend(_doubles.All, "updateTotalViews", 1);
    }
}
