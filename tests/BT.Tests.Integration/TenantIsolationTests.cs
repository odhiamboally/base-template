using BT.Domain.Features.IAM.Menus.Entities;
using BT.Domain.Shared.Contracts.Common;
using BT.Persistence.Features.IAM.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BT.Tests.Integration;

public class TenantIsolationTests : IDisposable
{
    private readonly DbContextOptions<IamDBContext> _options;

    public TenantIsolationTests()
    {
        _options = new DbContextOptionsBuilder<IamDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
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
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();

        var tenantProvider = new TestTenantProvider { TenantId = tenant1 };
        var actorProvider = new TestActorProvider();

        // Seed data acting as tenant 1
        using (var context = new IamDBContext(_options, tenantProvider, actorProvider))
        {
            await context.Database.EnsureCreatedAsync();

            var menu1 = MenuItem.Create(null, null, "menu-1", "Menu 1", "Desc 1", "/m1", "icon1", "top", null, null, 1, "test");
            context.MenuItems.Add(menu1);
            await context.SaveChangesAsync();
        }

        // Seed data acting as tenant 2
        tenantProvider.TenantId = tenant2;
        using (var context = new IamDBContext(_options, tenantProvider, actorProvider))
        {
            var menu2 = MenuItem.Create(null, null, "menu-2", "Menu 2", "Desc 2", "/m2", "icon2", "top", null, null, 2, "test");
            context.MenuItems.Add(menu2);
            await context.SaveChangesAsync();
        }

        // Act - Query as tenant 1
        tenantProvider.TenantId = tenant1;
        using (var context = new IamDBContext(_options, tenantProvider, actorProvider))
        {
            var items = await context.MenuItems.ToListAsync();

            // Assert
            Assert.Single(items);
            Assert.Equal("menu-1", items[0].Key);
        }
        
        // Act - Query as tenant 2
        tenantProvider.TenantId = tenant2;
        using (var context = new IamDBContext(_options, tenantProvider, actorProvider))
        {
            var items = await context.MenuItems.ToListAsync();

            // Assert
            Assert.Single(items);
            Assert.Equal("menu-2", items[0].Key);
        }
    }
    
    [Fact]
    public async Task IgnoreQueryFilters_ShouldReturnDataForAllTenants()
    {
        // Arrange
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();

        var tenantProvider = new TestTenantProvider { TenantId = tenant1 };
        var actorProvider = new TestActorProvider();

        // Seed data acting as tenant 1
        using (var context = new IamDBContext(_options, tenantProvider, actorProvider))
        {
            await context.Database.EnsureCreatedAsync();

            var menu1 = MenuItem.Create(null, null, "menu-1", "Menu 1", "Desc 1", "/m1", "icon1", "top", null, null, 1, "test");
            context.MenuItems.Add(menu1);
            await context.SaveChangesAsync();
        }

        // Seed data acting as tenant 2
        tenantProvider.TenantId = tenant2;
        using (var context = new IamDBContext(_options, tenantProvider, actorProvider))
        {
            var menu2 = MenuItem.Create(null, null, "menu-2", "Menu 2", "Desc 2", "/m2", "icon2", "top", null, null, 2, "test");
            context.MenuItems.Add(menu2);
            await context.SaveChangesAsync();
        }

        // Act - Ignore query filters
        tenantProvider.TenantId = tenant1; // Doesn't matter for this query
        using (var context = new IamDBContext(_options, tenantProvider, actorProvider))
        {
            var items = await context.MenuItems.IgnoreQueryFilters().ToListAsync();

            // Assert
            Assert.True(items.Count >= 2);
        }
    }

    public void Dispose()
    {
    }
}
