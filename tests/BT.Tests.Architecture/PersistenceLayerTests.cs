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
    public void EntityConfigurations_Should_Reside_In_Feature_Configuration_Namespaces()
    {
        var configurationTypes = Types.InAssembly(AssemblyReferences.Persistence)
            .That()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .GetTypes();

        var misplacedTypes = configurationTypes
            .Where(t => t.Namespace is null ||
                        !t.Namespace.StartsWith("BT.Persistence.Features.", StringComparison.Ordinal) ||
                        !t.Namespace.Contains(".EntityConfigurations", StringComparison.Ordinal))
            .Select(t => t.FullName)
            .ToList();

        misplacedTypes.Should().BeEmpty(
            because: "entity configurations must live under their owning Persistence feature folder. Found: {0}",
            string.Join(", ", misplacedTypes));
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
            "BT.Persistence.Features.Banking.DataContext",
            "BT.Persistence.Features.HR.DataContext",
            "BT.Persistence.Features.IAM.DataContext",
            "BT.Persistence.Features.Shared.DataContext"
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
            "SharedDBContext",
            "IamDBContext",
            "HrDBContext",
            "BankingDBContext"
        ], because: "the modular monolith requires one DbContext per bounded context.");
    }
}
