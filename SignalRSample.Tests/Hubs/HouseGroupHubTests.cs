using Microsoft.AspNetCore.SignalR;
using Moq;
using SignalRSample.Hubs;
using SignalRSample.Tests.Helpers;

namespace SignalRSample.Tests.Hubs;

public class HouseGroupHubTests
{
    private HubTestDoubles _doubles = null!;
    private HouseGroupHub _hub = null!;

    [SetUp]
    public void SetUp()
    {
        HouseGroupHub.GroupsJoined.Clear();
        _doubles = new HubTestDoubles(connectionId: "conn-1");
        _hub = new HouseGroupHub();
        _doubles.Attach(_hub);
    }

    [Test]
    public async Task JoinHouse_AddsMembershipNotifiesCallerAndOthersAndJoinsGroup()
    {
        await _hub.JoinHouse("Gryffindor");

        Assert.That(HouseGroupHub.GroupsJoined, Does.Contain("conn-1:Gryffindor"));
        HubTestDoubles.VerifySend(_doubles.Caller, "subscriptionStatus", "Gryffindor ", "gryffindor", true);
        HubTestDoubles.VerifySend(_doubles.Others, "newMemberAddedToHouse", "Gryffindor");
        _doubles.Groups.Verify(g => g.AddToGroupAsync("conn-1", "Gryffindor", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task JoinHouse_DoesNothingWhenConnectionAlreadyJoinedThatHouse()
    {
        HouseGroupHub.GroupsJoined.Add("conn-1:Gryffindor");

        await _hub.JoinHouse("Gryffindor");

        Assert.That(HouseGroupHub.GroupsJoined, Has.Count.EqualTo(1));
        HubTestDoubles.VerifyNeverSent(_doubles.Caller);
        HubTestDoubles.VerifyNeverSent(_doubles.Others);
        _doubles.Groups.Verify(
            g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task JoinHouse_IncludesEveryHouseForTheSameConnection()
    {
        await _hub.JoinHouse("Gryffindor");
        await _hub.JoinHouse("Slytherin");

        HubTestDoubles.VerifySend(_doubles.Caller, "subscriptionStatus", "Gryffindor Slytherin ", "slytherin", true);
    }

    [Test]
    public async Task LeaveHouse_RemovesMembershipNotifiesClientsAndLeavesGroup()
    {
        HouseGroupHub.GroupsJoined.Add("conn-1:Gryffindor");
        HouseGroupHub.GroupsJoined.Add("conn-1:Ravenclaw");

        await _hub.LeaveHouse("Gryffindor");

        Assert.That(HouseGroupHub.GroupsJoined, Is.EqualTo(new[] { "conn-1:Ravenclaw" }));
        HubTestDoubles.VerifySend(_doubles.Caller, "subscriptionStatus", "Ravenclaw ", "gryffindor", false);
        HubTestDoubles.VerifySend(_doubles.Others, "newMemberRemovedFromHouse", "Gryffindor");
        _doubles.Groups.Verify(g => g.RemoveFromGroupAsync("conn-1", "Gryffindor", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task LeaveHouse_DoesNothingWhenConnectionIsNotInTheHouse()
    {
        await _hub.LeaveHouse("Hufflepuff");

        Assert.That(HouseGroupHub.GroupsJoined, Is.Empty);
        HubTestDoubles.VerifyNeverSent(_doubles.Caller);
        _doubles.Groups.Verify(
            g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task TriggerHouseNotify_SendsNotificationToTheHouseGroup()
    {
        await _hub.TriggerHouseNotify("Gryffindor");

        _doubles.Clients.Verify(c => c.Group("Gryffindor"), Times.Once);
        HubTestDoubles.VerifySend(_doubles.GroupProxy, "triggerHouseNotification", "Gryffindor");
    }
}
