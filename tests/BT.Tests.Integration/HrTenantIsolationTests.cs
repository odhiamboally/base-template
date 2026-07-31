using BT.Domain.Features.HR.Departments.Entities;
using BT.Domain.Shared.Contracts.Common;
using BT.Persistence.Features.HR.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BT.Tests.Integration;

public class HrTenantIsolationTests : IDisposable
{
    private readonly DbContextOptions<HrDBContext> _options;

    public HrTenantIsolationTests()
    {
        _options = new DbContextOptionsBuilder<HrDBContext>()
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
        using (var context = new HrDBContext(_options, tenantProvider, actorProvider))
        {
            await context.Database.EnsureCreatedAsync();

            var dept1 = Department.Create("DEPT1", "Department 1", "Desc 1", "test");
            context.Departments.Add(dept1);
            await context.SaveChangesAsync();
        }

        // Seed data acting as tenant 2
        tenantProvider.TenantId = tenant2;
        using (var context = new HrDBContext(_options, tenantProvider, actorProvider))
        {
            var dept2 = Department.Create("DEPT2", "Department 2", "Desc 2", "test");
            context.Departments.Add(dept2);
            await context.SaveChangesAsync();
        }

        // Act - Query as tenant 1
        tenantProvider.TenantId = tenant1;
        using (var context = new HrDBContext(_options, tenantProvider, actorProvider))
        {
            var items = await context.Departments.ToListAsync();

            // Assert
            Assert.Single(items);
            Assert.Equal("DEPT1", items[0].Code);
        }
        
        // Act - Query as tenant 2
        tenantProvider.TenantId = tenant2;
        using (var context = new HrDBContext(_options, tenantProvider, actorProvider))
        {
            var items = await context.Departments.ToListAsync();

            // Assert
            Assert.Single(items);
            Assert.Equal("DEPT2", items[0].Code);
        }
    }

    public void Dispose()
    {
    }
}
