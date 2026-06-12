using System;
using System.Linq;
using System.Threading.Tasks;
using BT.Persistence.Common.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BT.Tests.Unit.Persistence
{
    public class RepositoryUpdateTests
    {
        private sealed class TestEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
            {
            }

            public DbSet<TestEntity> TestEntities { get; set; } = null!;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TestEntity>().HasKey(e => e.Id);
                base.OnModelCreating(modelBuilder);
            }
        }

        [Fact]
        public async Task UpdateAsync_MergesIntoTrackedInstance_WhenDuplicateKeyExists()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Seed a tracked instance
            using (var seedContext = new TestDbContext(options))
            {
                var existing = new TestEntity { Id = Guid.NewGuid(), Name = "Original" };
                seedContext.Add(existing);
                await seedContext.SaveChangesAsync();
            }

            // Act: create a new context to get the tracked instance into ChangeTracker, then call UpdateAsync
            using (var context = new TestDbContext(options))
            {
                // Attach/load the existing instance so the context is tracking it
                var tracked = await context.TestEntities.FirstAsync();

                var repository = new Repository<TestEntity>(context);

                // Create a different instance with the same key but modified data (simulating a detached entity)
                var detached = new TestEntity { Id = tracked.Id, Name = "Updated" };

                var result = await repository.UpdateAsync(detached);

                // There should only be one tracked entry for TestEntity
                var entries = context.ChangeTracker.Entries<TestEntity>().ToList();
                Assert.Single(entries);

                var trackedEntity = entries.Single().Entity;

                // The tracked instance must have been updated with the detached instance values
                Assert.Equal("Updated", trackedEntity.Name);

                // The repository should return the tracked entity instance
                Assert.Equal(trackedEntity.Id, result.Id);
                Assert.Equal(trackedEntity.Name, result.Name);
            }
        }

        [Fact]
        public async Task UpdateRangeAsync_StagesChanges_WithoutSavingImmediately()
        {
            var entityId = Guid.NewGuid();
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using (var seedContext = new TestDbContext(options))
            {
                seedContext.Add(new TestEntity { Id = entityId, Name = "Original" });
                await seedContext.SaveChangesAsync();
            }

            await using (var context = new TestDbContext(options))
            {
                var entity = await context.TestEntities.FirstAsync();
                entity.Name = "Staged";

                var repository = new Repository<TestEntity>(context);
                var stagedCount = await repository.UpdateRangeAsync([entity]);

                Assert.Equal(1, stagedCount);
            }

            await using (var verifyContext = new TestDbContext(options))
            {
                var persisted = await verifyContext.TestEntities.SingleAsync();
                Assert.Equal("Original", persisted.Name);
            }
        }
    }
}
