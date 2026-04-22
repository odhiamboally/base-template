using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;

namespace BT.Tests.Architecture;

/// <summary>
/// Enforces structural rules within the Persistence layer.
/// </summary>
public sealed class PersistenceLayerTests
{
    // ── Entity Configurations ─────────────────────────────────────────────────

    [Fact]
    public void EntityConfigurations_Should_Be_Named_With_Configuration_Suffix()
    {
        var result = Types.InAssembly(AssemblyReferences.Persistence)
            .That()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .And().AreNotAbstract()
            .And().AreNotGeneric()
            .Should()
            .HaveNameEndingWith("Configuration")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "All IEntityTypeConfiguration<T> implementations must end in 'Configuration'. " +
                     "Failing types: {0}", string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? []));
    }

    [Fact]
    public void EntityConfigurations_Should_Be_Internal()
    {
        var result = Types.InAssembly(AssemblyReferences.Persistence)
            .That()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Entity configurations are internal persistence details " +
                     "and must not be public. Failing types: {0}", string.Join(", ",
                result.FailingTypes?.Select(t => t.Name) ?? []));
    }

    [Fact]
    public void EntityConfigurations_Should_Reside_In_Configurations_Namespace()
    {
        var result = Types.InAssembly(AssemblyReferences.Persistence)
            .That()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .Should()
            .ResideInNamespaceStartingWith("BT.Persistence.EntityConfigurations")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "All entity configurations must live under BT.Persistence.EntityConfigurations. " +
                     "Failing types: {0}", string.Join(", ",
                result.FailingTypes?.Select(t => t.Name) ?? []));
    }

    // ── DbContexts ────────────────────────────────────────────────────────────

    [Fact]
    public void DbContexts_Should_Reside_In_Known_DataContext_Namespaces()
    {
        var dbContextTypes = Types.InAssembly(AssemblyReferences.Persistence)
            .That()
            .Inherit(typeof(DbContext))
            .GetTypes()
            .ToList();

        dbContextTypes.Should().NotBeEmpty("Persistence must define EF Core DbContext types.");

        var allowedNamespaces = new[]
        {
            "BT.Persistence.Banking.DataContext",
            "BT.Persistence.HR.DataContext",
            "BT.Persistence.IAM.DataContext",
            "BT.Persistence.Shared.DataContext",
            "BT.Persistence.DataContext"
        };

        var misplacedTypes = dbContextTypes
            .Where(t => t.Namespace is null || !allowedNamespaces.Contains(t.Namespace))
            .Select(t => t.FullName)
            .ToList();

        misplacedTypes.Should().BeEmpty(
            because: "DbContext subclasses must live in one of the bounded-context DataContext namespaces. Found: {0}",
            string.Join(", ", misplacedTypes));
    }

    [Fact]
    public void Required_Bounded_Context_DbContexts_Should_Exist()
    {
        var dbContextTypeNames = Types.InAssembly(AssemblyReferences.Persistence)
            .That()
            .Inherit(typeof(DbContext))
            .GetTypes()
            .Select(t => t.Name)
            .ToList();

        dbContextTypeNames.Should().Contain([
            "SharedDbContext",
            "IamDbContext",
            "HrDbContext",
            "BankingDbContext"
        ], because: "the modular monolith requires one DbContext per bounded context.");
    }
}
