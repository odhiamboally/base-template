using FluentAssertions;
using FluentValidation;
using NetArchTest.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Tests.Architecture;

/// <summary>
/// Enforces naming conventions that the compiler and editorconfig cannot catch —
/// specifically, conventions on types across assembly and namespace boundaries.
/// </summary>
public sealed class NamingConventionTests
{
    // ── Interfaces ────────────────────────────────────────────────────────────

    [Fact]
    public void All_Interfaces_Should_Start_With_I()
    {
        var assemblies = new[]
        {
            AssemblyReferences.Domain,
            AssemblyReferences.Application,
            AssemblyReferences.Persistence,
        };

        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: $"All interfaces in {assembly.GetName().Name} must start with 'I'. " +
                         "Failing types: {0}", string.Join(", ",
                    result.FailingTypes?.Select(t => t.FullName) ?? []));
        }
    }

    // ── Async methods — checked via Roslyn/editorconfig, not NetArchTest ──────
    // NetArchTest reflects on types, not method signatures. Async method naming
    // (must end in Async) is enforced by the dotnet_naming_rule in .editorconfig.

    // ── Exception classes ─────────────────────────────────────────────────────

    [Fact]
    public void Exceptions_Should_End_With_Exception()
    {
        var assemblies = new[]
        {
            AssemblyReferences.Domain,
            AssemblyReferences.Application,
        };

        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .Inherit(typeof(Exception))
                .Should()
                .HaveNameEndingWith("Exception")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: $"All Exception subclasses in {assembly.GetName().Name} must end in 'Exception'. " +
                         "Failing types: {0}", string.Join(", ",
                    result.FailingTypes?.Select(t => t.Name) ?? []));
        }
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Dtos_Should_Reside_In_Shared_Project()
    {
        // Nothing in Domain or Persistence should be named *Dto —
        // DTOs belong exclusively in BT.Shared.
        var domainDtos = Types.InAssembly(AssemblyReferences.Domain)
            .That()
            .HaveNameEndingWith("Dto")
            .GetTypes();

        var persistenceDtos = Types.InAssembly(AssemblyReferences.Persistence)
            .That()
            .HaveNameEndingWith("Dto")
            .GetTypes();

        domainDtos.Should().BeEmpty(
            because: "DTOs must live in BT.Shared, not BT.Domain. " +
                     "Found in Domain: {0}", string.Join(", ", domainDtos.Select(t => t.Name)));

        persistenceDtos.Should().BeEmpty(
            because: "DTOs must live in BT.Shared, not BT.Persistence. " +
                     "Found in Persistence: {0}", string.Join(", ", persistenceDtos.Select(t => t.Name)));
    }

    [Fact]
    public void SharedKernel_FeatureOwned_Types_Should_Reside_In_Features_Namespace()
    {
        var misplacedTypes = Types.InAssembly(AssemblyReferences.SharedKernel)
            .That()
            .ResideInNamespaceStartingWith("BT.SharedKernel.Dtos.Auth")
            .Or()
            .ResideInNamespaceStartingWith("BT.SharedKernel.Dtos.Employees")
            .Or()
            .ResideInNamespaceStartingWith("BT.SharedKernel.Dtos.Directors")
            .Or()
            .ResideInNamespaceStartingWith("BT.SharedKernel.Dtos.Dashboard")
            .Or()
            .ResideInNamespaceStartingWith("BT.SharedKernel.Dtos.Lookups")
            .Or()
            .ResideInNamespaceStartingWith("BT.SharedKernel.Dtos.Banking")
            .Or()
            .ResideInNamespaceStartingWith("BT.SharedKernel.Enums")
            .GetTypes();

        misplacedTypes.Should().BeEmpty(
            because: "feature-owned SharedKernel DTOs and enums must live under BT.SharedKernel.Features.*. Found: {0}",
            string.Join(", ", misplacedTypes.Select(t => t.FullName)));
    }

    [Fact]
    public void SharedKernel_Validators_Should_Reside_In_Features_Or_Common_Namespace()
    {
        var validatorTypes = Types.InAssembly(AssemblyReferences.SharedKernelValidation)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .GetTypes();

        var misplacedTypes = validatorTypes
            .Where(t => t.Namespace is null ||
                        (!t.Namespace.StartsWith("BT.SharedKernel.Validation.Features.", StringComparison.Ordinal) &&
                         !t.Namespace.StartsWith("BT.SharedKernel.Validation.Validators.Common", StringComparison.Ordinal)))
            .Select(t => t.FullName)
            .ToList();

        misplacedTypes.Should().BeEmpty(
            because: "feature-owned validators must live under BT.SharedKernel.Validation.Features.*; only generic validator bases stay in Common. Found: {0}",
            string.Join(", ", misplacedTypes));
    }

    [Fact]
    public void Api_Controllers_Should_Reside_In_Features_Namespace()
    {
        var apiRoot = Path.Combine(
            AssemblyReferences.RepoRoot,
            "src",
            "Backend",
            "Api",
            "BT.Api");

        var misplacedControllers = Directory
            .EnumerateFiles(apiRoot, "*Controller.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}Common{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
                           !path.Contains($"{Path.DirectorySeparatorChar}Features{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(apiRoot, path))
            .ToList();

        misplacedControllers.Should().BeEmpty(
            because: "feature-owned API controllers must live under BT.Api.Features by bounded context and feature. Found: {0}",
            string.Join(", ", misplacedControllers));
    }
}

