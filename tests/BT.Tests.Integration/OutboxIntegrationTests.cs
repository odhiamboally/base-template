using BT.Domain.Enums;
using BT.Domain.Events;
using BT.Persistence.DataContext;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.Testing;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Serilog;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BT.Tests.Integration;

public class OutboxIntegrationTests
{
    [Fact]
    public async Task Should_Persist_Event_To_Outbox()
    {
        // Arrange
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var provider = new ServiceCollection()
            .AddDbContext<DBContext>(o => o.UseSqlite(connection))
            .AddMassTransitTestHarness(x =>
            {
                x.AddEntityFrameworkOutbox<DBContext>(o =>
                {
                    o.UseSqlite();
                    o.UseBusOutbox();
                });

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            })
            .BuildServiceProvider(true);

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DBContext>();

        await db.Database.EnsureCreatedAsync();

        var message = new CustomerCreatedEvent(Guid.CreateVersion7(), "", "", "", default);

        // Act
        await using var tx = await db.Database.BeginTransactionAsync();

        var bus = scope.ServiceProvider.GetRequiredService<IBus>();
        await bus.Publish(message);
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        // Assert
        var outboxMessages = await db.Set<OutboxMessage>().ToListAsync();
        Assert.Single(outboxMessages);
    }

    [Fact]
    public async Task Should_Dispatch_Event_From_Outbox()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var provider = new ServiceCollection()
            .AddDbContext<DBContext>(o => o.UseSqlite(connection))
            .AddMassTransitTestHarness(x =>
            {
                x.AddEntityFrameworkOutbox<DBContext>(o =>
                {
                    o.UseSqlite();
                    o.UseBusOutbox();
                    o.QueryDelay = TimeSpan.FromMilliseconds(100);
                });

                x.AddConfigureEndpointsCallback((context, name, cfg) =>
                {
                    cfg.UseEntityFrameworkOutbox<DBContext>(context);
                });
            })
            .BuildServiceProvider(true);

        using var scope = provider.CreateScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var db = scope.ServiceProvider.GetRequiredService<DBContext>();

        await db.Database.EnsureCreatedAsync();
        await harness.Start();

        try
        {
            var message = new CustomerCreatedEvent(Guid.CreateVersion7(), "", "", "", default(CustomerType));

            await using var tx = await db.Database.BeginTransactionAsync();

            await harness.Bus.Publish(message);
            await db.SaveChangesAsync();
            await tx.CommitAsync();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var published = await harness.Published.Any<CustomerCreatedEvent>(cts.Token);

            Assert.True(published);
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Outbox_Should_Capture_All_Events()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync().ConfigureAwait(true);

        await using var provider = new ServiceCollection()
            .AddDbContext<DBContext>(options =>
                options.UseSqlite(connection))
            .AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<DBContext>(o =>
                {
                    o.UseSqlite();
                    o.UseBusOutbox();
                });

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            })
            .BuildServiceProvider(true);

        using var scope = provider.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<DBContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IBus>();

        await db.Database.EnsureCreatedAsync();

        var clientEvent = new CustomerCreatedEvent(Guid.CreateVersion7(), "Test", "User", "test@example.com", default);

        await using var tx = await db.Database.BeginTransactionAsync();

        await bus.Publish(clientEvent);
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        var existsInOutbox = await db.Set<OutboxMessage>()
            .AnyAsync(m => m.MessageType.Contains(nameof(CustomerCreatedEvent)));

        Assert.True(existsInOutbox);
    }

}
