using BT.Domain.Features.Banking.Customers.Entities;
using BT.Domain.Features.Banking.Customers.Enums;
using BT.Domain.Features.Banking.Customers.ValueObjects;
using BT.Domain.Shared.Contracts.Common;
using BT.Persistence.Features.Banking.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BT.Tests.Integration;

public class BankingTenantIsolationTests : IDisposable
{
    private readonly DbContextOptions<BankingDBContext> _options;

    public BankingTenantIsolationTests()
    {
        _options = new DbContextOptionsBuilder<BankingDBContext>()
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

        var corpDetail = CorporateDetail.Create("TestCorp", (LineOfBusiness)1, "IT", (IdentificationType)1, "123", DateTimeOffset.UtcNow, "test", "test", "test", null, null, null, null, null, null);
        var address = Address.Create("Street", "City", "State", "ZIP", "Country", "test@test.com", "1234567890");
        var commPref = CommunicationPreference.Create(true, true, true);

        // Seed data acting as tenant 1
        using (var context = new BankingDBContext(_options, tenantProvider, actorProvider))
        {
            await context.Database.EnsureCreatedAsync();

            var customer1 = Customer.Create("CUST-1", "Customer 1", CustomerType.Corporate, SegmentType.Corporate, SubSegmentType.Local, Guid.CreateVersion7(), DateTimeOffset.UtcNow, corpDetail, address, commPref, "test");
            context.Customers.Add(customer1);
            await context.SaveChangesAsync();
        }

        // Seed data acting as tenant 2
        tenantProvider.TenantId = tenant2;
        using (var context = new BankingDBContext(_options, tenantProvider, actorProvider))
        {
            var customer2 = Customer.Create("CUST-2", "Customer 2", CustomerType.Corporate, SegmentType.Corporate, SubSegmentType.Local, Guid.CreateVersion7(), DateTimeOffset.UtcNow, corpDetail, address, commPref, "test");
            context.Customers.Add(customer2);
            await context.SaveChangesAsync();
        }

        // Act - Query as tenant 1
        tenantProvider.TenantId = tenant1;
        using (var context = new BankingDBContext(_options, tenantProvider, actorProvider))
        {
            var items = await context.Customers.ToListAsync();

            // Assert
            Assert.Single(items);
            Assert.Equal("CUST-1", items[0].Number);
        }
        
        // Act - Query as tenant 2
        tenantProvider.TenantId = tenant2;
        using (var context = new BankingDBContext(_options, tenantProvider, actorProvider))
        {
            var items = await context.Customers.ToListAsync();

            // Assert
            Assert.Single(items);
            Assert.Equal("CUST-2", items[0].Number);
        }
    }

    public void Dispose()
    {
    }
}
