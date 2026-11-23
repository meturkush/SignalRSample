using Microsoft.AspNetCore.SignalR;
using Moq;
using SignalRSample.Hubs;
using SignalRSample.Tests.Helpers;

namespace SignalRSample.Tests.Hubs;

public class ChatHubTests
{
    private const string UserId = "user-harry";
    private const string UserName = "harry";
    private const string ConnectionId = "conn-1";

    [SetUp]
    public void SetUp() => TestDb.ResetConnections();

    [TearDown]
    public void TearDown() => TestDb.ResetConnections();

    [Test]
    public async Task OnConnectedAsync_WithAuthenticatedUser_NotifiesOnlineUsersAndTracksConnection()
    {
        using var db = TestDb.Create((UserId, UserName, "harry@hogwarts.com"));
        var doubles = CreateDoubles();
        var hub = new ChatHub(db);
        doubles.Attach(hub);

        await hub.OnConnectedAsync();

        Assert.That(HubConnections.HasUserConnection(UserId, ConnectionId), Is.True);
        doubles.Clients.Verify(c => c.Users(It.IsAny<IReadOnlyList<string>>()), Times.Once);
        HubTestDoubles.VerifySend(doubles.UsersProxy, "ReceiveUserConnected", UserId, UserName);
    }

    [Test]
    public async Task OnConnectedAsync_WithoutUserId_DoesNotTrackOrNotify()
    {
        using var db = TestDb.Create();
        var doubles = new HubTestDoubles(ConnectionId);
        var hub = new ChatHub(db);
        doubles.Attach(hub);

        await hub.OnConnectedAsync();

        Assert.That(HubConnections.Users, Is.Empty);
        HubTestDoubles.VerifyNeverSent(doubles.UsersProxy);
    }

    [Test]
    public async Task OnDisconnectedAsync_NotifiesOnlineUsers()
    {
        HubConnections.AddUserConnection(UserId, ConnectionId);
        using var db = TestDb.Create((UserId, UserName, "harry@hogwarts.com"));
        var doubles = CreateDoubles();
        var hub = new ChatHub(db);
        doubles.Attach(hub);

        await hub.OnDisconnectedAsync(new InvalidOperationException("socket closed"));

        HubTestDoubles.VerifySend(doubles.UsersProxy, "ReceiveUserDisconnected", UserId, UserName);
        Assert.That(HubConnections.HasUserConnection(UserId, ConnectionId), Is.True);
    }

    [Test]
    public async Task OnDisconnectedAsync_WithoutUserId_DoesNotNotify()
    {
        using var db = TestDb.Create();
        var doubles = new HubTestDoubles(ConnectionId);
        var hub = new ChatHub(db);
        doubles.Attach(hub);

        await hub.OnDisconnectedAsync(null);

        HubTestDoubles.VerifyNeverSent(doubles.UsersProxy);
    }

    [Test]
    public async Task SendAddRoomMessage_BroadcastsRoomDetailsAndUser()
    {
        var (hub, doubles) = CreateHub();

        await hub.SendAddRoomMessage(4, 12, "Room of Requirement");

        HubTestDoubles.VerifySend(
            doubles.All,
            "ReceiveAddRoomMessage",
            4,
            12,
            "Room of Requirement",
            UserId,
            UserName);
    }

    [Test]
    public async Task SendDeleteRoomMessage_BroadcastsDeletedRoom()
    {
        var (hub, doubles) = CreateHub();

        await hub.SendDeleteRoomMessage(3, 1, "Great Hall");

        HubTestDoubles.VerifySend(
            doubles.All,
            "ReceiveDeleteRoomMessage",
            3,
            1,
            "Great Hall",
            UserName);
    }

    [Test]
    public async Task SendPublicMessage_BroadcastsMessageToAllClients()
    {
        var (hub, doubles) = CreateHub();

        await hub.SendPublicMessage(8, "Expecto Patronum", "Dumbledore's Army");

        HubTestDoubles.VerifySend(
            doubles.All,
            "ReceivePublicMessage",
            8,
            UserId,
            UserName,
            "Expecto Patronum",
            "Dumbledore's Army");
    }

    [Test]
    public async Task SendPrivateMessage_SendsToSenderAndReceiver()
    {
        var (hub, doubles) = CreateHub();

        await hub.SendPrivateMessage("user-ron", "meet me", "ron");

        doubles.Clients.Verify(c => c.Users(It.Is<IReadOnlyList<string>>(ids =>
            ids.Count == 2 && ids[0] == UserId && ids[1] == "user-ron")), Times.Once);
        HubTestDoubles.VerifySend(
            doubles.UsersProxy,
            "ReceivePrivateMessage",
            UserId,
            UserName,
            "user-ron",
            "meet me",
            (Func<object?, bool>)(value => value is Guid),
            "ron");
    }

    [Test]
    public async Task SendOpenPrivateChat_NotifiesTheReceiver()
    {
        var (hub, doubles) = CreateHub();

        await hub.SendOpenPrivateChat("user-hermione");

        doubles.Clients.Verify(c => c.User("user-hermione"), Times.Once);
        HubTestDoubles.VerifySend(doubles.UserProxy, "ReceiveOpenPrivateChat", UserId, UserName);
    }

    [Test]
    public async Task SendDeletePrivateChat_BroadcastsChatId()
    {
        var (hub, doubles) = CreateHub();

        await hub.SendDeletePrivateChat("chat-42");

        HubTestDoubles.VerifySend(doubles.All, "ReceiveDeletePrivateChat", "chat-42");
    }

    private static (ChatHub Hub, HubTestDoubles Doubles) CreateHub()
    {
        var db = TestDb.Create((UserId, UserName, "harry@hogwarts.com"));
        var doubles = CreateDoubles();
        var hub = new ChatHub(db);
        doubles.Attach(hub);
        return (hub, doubles);
    }

    private static HubTestDoubles CreateDoubles() =>
        new(ConnectionId, HubTestDoubles.Principal(UserId, UserName));
}
