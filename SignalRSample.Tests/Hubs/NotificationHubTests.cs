using SignalRSample.Hubs;
using SignalRSample.Tests.Helpers;

namespace SignalRSample.Tests.Hubs;

public class NotificationHubTests
{
    private HubTestDoubles _doubles = null!;
    private NotificationHub _hub = null!;

    [SetUp]
    public void SetUp()
    {
        NotificationHub.notificationCounter = 0;
        NotificationHub.messages.Clear();
        _doubles = new HubTestDoubles();
        _hub = new NotificationHub();
        _doubles.Attach(_hub);
    }

    [Test]
    public async Task SendMessage_IgnoresNullOrEmptyMessage()
    {
        await _hub.SendMessage(string.Empty);
        await _hub.SendMessage(null!);

        Assert.That(NotificationHub.notificationCounter, Is.EqualTo(0));
        Assert.That(NotificationHub.messages, Is.Empty);
        HubTestDoubles.VerifyNeverSent(_doubles.All);
    }

    [Test]
    public async Task SendMessage_StoresMessageIncrementsCounterAndBroadcasts()
    {
        await _hub.SendMessage("The wand was found");

        Assert.That(NotificationHub.notificationCounter, Is.EqualTo(1));
        Assert.That(NotificationHub.messages, Is.EqualTo(new[] { "The wand was found" }));
        HubTestDoubles.VerifySend(
            _doubles.All,
            "LoadNotification",
            (Func<object?, bool>)(messages => messages is List<string> list && list.Count == 1 && list[0] == "The wand was found"),
            1);
    }

    [Test]
    public async Task LoadMessages_BroadcastsCurrentMessagesAndCounter()
    {
        NotificationHub.messages.Add("existing");
        NotificationHub.notificationCounter = 7;

        await _hub.LoadMessages();

        HubTestDoubles.VerifySend(
            _doubles.All,
            "LoadNotification",
            (Func<object?, bool>)(messages => messages is List<string> list && list.Single() == "existing"),
            7);
    }
}
