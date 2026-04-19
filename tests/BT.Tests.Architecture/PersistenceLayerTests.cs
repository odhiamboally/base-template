using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;
using System;
using System.Collections.Generic;
using System.Text;

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
        // Configurations are persistence implementation details.
        // They must never be referenced from Application or Domain.
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

    // ── DBContext ─────────────────────────────────────────────────────────────

    [Fact]
    public void DBContext_Should_Reside_In_DataContext_Namespace()
    {
        var result = Types.InAssembly(AssemblyReferences.Persistence)
            .That()
            .Inherit(typeof(DbContext))
            .Should()
            .ResideInNamespace("BT.Persistence.DataContext")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "DbContext subclasses must live in BT.Persistence.DataContext. " +
                     "Failing types: {0}", string.Join(", ",
                result.FailingTypes?.Select(t => t.Name) ?? []));
    }

    [Fact]
    public void Only_One_DbContext_Should_Exist()
    {
        var dbContextTypes = Types.InAssembly(AssemblyReferences.Persistence)
            .That()
            .Inherit(typeof(DbContext))
            .GetTypes()
            .ToList();

        dbContextTypes.Should().HaveCount(1,
            because: "A single-tenant application should have exactly one DbContext. " +
                     "Found: {0}", string.Join(", ", dbContextTypes.Select(t => t.Name)));
    }
}
