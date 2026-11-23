using SignalRSample.Hubs;
using SignalRSample.Tests.Helpers;

namespace SignalRSample.Tests.Hubs;

public class BasicChatHubTests
{
    [Test]
    public async Task SendMessageToAll_BroadcastsMessageReceived()
    {
        var doubles = new HubTestDoubles();
        var hub = new BasicChatHub(TestDb.Create());
        doubles.Attach(hub);

        await hub.SendMessageToAll("harry", "hello everyone");

        HubTestDoubles.VerifySend(doubles.All, "MessageReceived", "harry", "hello everyone");
    }

    [Test]
    public async Task SendMessageToReceiver_SendsToTheUserMatchingTheEmail()
    {
        var doubles = new HubTestDoubles();
        using var db = TestDb.Create(("user-ron", "ron", "ron@hogwarts.com"));
        var hub = new BasicChatHub(db);
        doubles.Attach(hub);

        await hub.SendMessageToReceiver("harry", "Ron@Hogwarts.com", "privately");

        doubles.Clients.Verify(c => c.User("user-ron"));
        HubTestDoubles.VerifySend(doubles.UserProxy, "MessageReceived", "harry", "privately");
    }

    [Test]
    public void SendMessageToReceiver_ThrowsWhenReceiverEmailIsUnknown()
    {
        var doubles = new HubTestDoubles();
        using var db = TestDb.Create();
        var hub = new BasicChatHub(db);
        doubles.Attach(hub);

        Assert.ThrowsAsync<NullReferenceException>(() =>
            hub.SendMessageToReceiver("harry", "missing@hogwarts.com", "privately"));
    }
}
