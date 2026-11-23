using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using SignalRSample.Data;
using SignalRSample.Hubs;

namespace SignalRSample.Tests.Helpers;

internal sealed class HubTestDoubles
{
    public Mock<IHubCallerClients> Clients { get; } = new();
    public Mock<IClientProxy> All { get; } = new();
    public Mock<ISingleClientProxy> Caller { get; } = new();
    public Mock<IClientProxy> Others { get; } = new();
    public Mock<IClientProxy> UserProxy { get; } = new();
    public Mock<IClientProxy> UsersProxy { get; } = new();
    public Mock<IClientProxy> GroupProxy { get; } = new();
    public Mock<HubCallerContext> Context { get; } = new();
    public Mock<IGroupManager> Groups { get; } = new();

    public HubTestDoubles(string connectionId = "conn-1", ClaimsPrincipal? user = null)
    {
        Clients.Setup(c => c.All).Returns(All.Object);
        Clients.Setup(c => c.Caller).Returns(Caller.Object);
        Clients.Setup(c => c.Others).Returns(Others.Object);
        Clients.Setup(c => c.User(It.IsAny<string>())).Returns(UserProxy.Object);
        Clients.Setup(c => c.Users(It.IsAny<IReadOnlyList<string>>())).Returns(UsersProxy.Object);
        Clients.Setup(c => c.Group(It.IsAny<string>())).Returns(GroupProxy.Object);

        SetupSend(All);
        SetupSend(Caller);
        SetupSend(Others);
        SetupSend(UserProxy);
        SetupSend(UsersProxy);
        SetupSend(GroupProxy);

        Context.Setup(c => c.ConnectionId).Returns(connectionId);
        Context.Setup(c => c.User).Returns(user ?? new ClaimsPrincipal(new ClaimsIdentity()));

        Groups.Setup(g => g.AddToGroupAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        Groups.Setup(g => g.RemoveFromGroupAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    public void Attach(Hub hub)
    {
        hub.Clients = Clients.Object;
        hub.Context = Context.Object;
        hub.Groups = Groups.Object;
    }

    public static ClaimsPrincipal Principal(string userId, string? name = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };
        if (name is not null)
        {
            claims.Add(new Claim(ClaimTypes.Name, name));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    public static void VerifySend<TProxy>(Mock<TProxy> proxy, string method, params object?[] args)
        where TProxy : class, IClientProxy
    {
        proxy.Verify(p => p.SendCoreAsync(
            method,
            It.Is<object?[]>(actual => ArgumentsMatch(actual, args)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyNeverSent<TProxy>(Mock<TProxy> proxy)
        where TProxy : class, IClientProxy
    {
        proxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object?[]>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static void SetupSend<TProxy>(Mock<TProxy> proxy)
        where TProxy : class, IClientProxy
    {
        proxy.Setup(p => p.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static bool ArgumentsMatch(object?[] actual, object?[] expected)
    {
        if (actual.Length != expected.Length)
        {
            return false;
        }

        for (var i = 0; i < expected.Length; i++)
        {
            if (expected[i] is Func<object?, bool> predicate)
            {
                if (!predicate(actual[i]))
                {
                    return false;
                }

                continue;
            }

            if (!Equals(actual[i], expected[i]))
            {
                return false;
            }
        }

        return true;
    }
}

internal static class TestDb
{
    public static ApplicationDbContext Create(params (string Id, string UserName, string Email)[] users)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();
        foreach (var (id, userName, email) in users)
        {
            db.Users.Add(new Microsoft.AspNetCore.Identity.IdentityUser
            {
                Id = id,
                UserName = userName,
                Email = email,
                NormalizedUserName = userName.ToUpperInvariant(),
                NormalizedEmail = email.ToUpperInvariant()
            });
        }

        db.SaveChanges();
        return db;
    }

    public static void ResetConnections() => HubConnections.Users.Clear();
}
