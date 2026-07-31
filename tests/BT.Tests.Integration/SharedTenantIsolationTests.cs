using BT.Domain.Features.Shared.TenantSettings.Entities;
using BT.Domain.Shared.Contracts.Common;
using BT.Persistence.Features.Shared.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BT.Tests.Integration;

public class SharedTenantIsolationTests : IDisposable
{
    private readonly DbContextOptions<SharedDBContext> _options;

    public SharedTenantIsolationTests()
    {
        _options = new DbContextOptionsBuilder<SharedDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.CreateVersion7().ToString())
            .Options;
    }

    private class TestTenantProvider : ICurrentTenantProvider
    {
        public Guid TenantId { get; set; }
    }

    private class TestActorProvider : ICurrentActorProvider
    {
        public string ActorId => "TestActor";
    }

    [Fact]
    public async Task Query_ShouldOnlyReturnDataForCurrentTenant()
    {
        // Arrange
        var tenant1 = Guid.CreateVersion7();
        var tenant2 = Guid.CreateVersion7();

        var tenantProvider = new TestTenantProvider { TenantId = tenant1 };
        var actorProvider = new TestActorProvider();

        // Seed data acting as tenant 1
        using (var context = new SharedDBContext(_options, tenantProvider, actorProvider))
        {
            await context.Database.EnsureCreatedAsync();

            var setting1 = new TenantSetting("SETTING_1", "Value 1", "test");
            context.TenantSettings.Add(setting1);
            await context.SaveChangesAsync();
        }

        // Seed data acting as tenant 2
        tenantProvider.TenantId = tenant2;
        using (var context = new SharedDBContext(_options, tenantProvider, actorProvider))
        {
            var setting2 = new TenantSetting("SETTING_2", "Value 2", "test");
            context.TenantSettings.Add(setting2);
            await context.SaveChangesAsync();
        }

        // Act - Query as tenant 1
        tenantProvider.TenantId = tenant1;
        using (var context = new SharedDBContext(_options, tenantProvider, actorProvider))
        {
            var items = await context.TenantSettings.ToListAsync();

            // Assert
            Assert.Single(items);
            Assert.Equal("SETTING_1", items[0].Key);
        }
        
        // Act - Query as tenant 2
        tenantProvider.TenantId = tenant2;
        using (var context = new SharedDBContext(_options, tenantProvider, actorProvider))
        {
            var items = await context.TenantSettings.ToListAsync();

            // Assert
            Assert.Single(items);
            Assert.Equal("SETTING_2", items[0].Key);
        }
    }

    public void Dispose()
    {
    }
}
