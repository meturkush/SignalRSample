using SignalRSample.Hubs;

namespace SignalRSample.Tests.Hubs;

public class DeathlyHallowsHubTests
{
    [Test]
    public void GetRaceStatus_ReturnsTheSharedRaceDictionary()
    {
        var hub = new DeathlyHallowsHub();

        var status = hub.GetRaceStatus();

        Assert.That(status, Is.SameAs(SD.DealthyHallowRace));
        Assert.That(status.Keys, Is.EquivalentTo(new[] { SD.Wand, SD.Stone, SD.Cloak }));
    }
}
