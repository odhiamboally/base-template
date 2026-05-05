using FluentAssertions;
using MediatR;
using NetArchTest.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Tests.Architecture;

/// <summary>
/// Enforces structural rules within the Application layer — CQRS conventions,
/// handler registration, and feature slice structure.
/// </summary>
public sealed class ApplicationLayerTests
{
    // ── Commands ──────────────────────────────────────────────────────────────

    [Fact]
    public void Commands_Should_Be_Named_With_Command_Suffix()
    {
        var result = Types.InAssembly(AssemblyReferences.Application)
            .That()
            .ImplementInterface(typeof(IRequest<>))
            .And()
            .HaveNameEndingWith("Command")
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult();

        // Inverse — find IRequest<T> implementors that aren't Query or Command
        var unnamedRequests = Types.InAssembly(AssemblyReferences.Application)
            .That()
            .ImplementInterface(typeof(IRequest<>))
            .And()
            .DoNotHaveNameEndingWith("Command")
            .And()
            .DoNotHaveNameEndingWith("Query")
            .GetTypes();

        unnamedRequests.Should().BeEmpty(
            because: "Every IRequest<T> in Application must be named *Command or *Query " +
                     "to make CQRS intent explicit. Unnamed types: {0}",
            string.Join(", ", unnamedRequests.Select(t => t.Name)));
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    [Fact]
    public void Queries_Should_Be_Named_With_Query_Suffix()
    {
        var unnamedQueries = Types.InAssembly(AssemblyReferences.Application)
            .That()
            .ImplementInterface(typeof(IRequest<>))
            .And()
            .DoNotHaveNameEndingWith("Query")
            .And()
            .DoNotHaveNameEndingWith("Command")
            .GetTypes();

        unnamedQueries.Should().BeEmpty(
            because: "Every IRequest<T> must end in Query or Command. " +
                     "Violators: {0}",
            string.Join(", ", unnamedQueries.Select(t => t.Name)));
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    [Fact]
    public void Handlers_Should_Be_Named_With_Handler_Suffix()
    {
        var result = Types.InAssembly(AssemblyReferences.Application)
            .That()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "All IRequestHandler<,> implementations must end in 'Handler'. " +
                     "Failing types: {0}", string.Join(", ",
                result.FailingTypes?.Select(t => t.Name) ?? []));
    }

    [Fact]
    public void Handlers_Should_Be_Internal()
    {
        // Handlers are implementation details — they must never be referenced
        // directly by outer layers. MediatR discovers them via DI registration.
        var result = Types.InAssembly(AssemblyReferences.Application)
            .That()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Handlers are internal implementation details. " +
                     "They must be internal or private, never public. " +
                     "Failing types: {0}", string.Join(", ",
                result.FailingTypes?.Select(t => t.Name) ?? []));
    }

    // ── Validators ────────────────────────────────────────────────────────────

    [Fact]
    public void Validators_Should_Be_Named_With_Validator_Suffix()
    {
        var result = Types.InAssembly(AssemblyReferences.Application)
            .That()
            .Inherit(typeof(FluentValidation.AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "All AbstractValidator<T> subclasses must end in 'Validator'. " +
                     "Failing types: {0}", string.Join(", ",
                result.FailingTypes?.Select(t => t.Name) ?? []));
    }

    // ── Feature slice structure ───────────────────────────────────────────────

    [Fact]
    public void Handlers_Should_Reside_In_Features_Namespace()
    {
        var result = Types.InAssembly(AssemblyReferences.Application)
            .That()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Should()
            .ResideInNamespaceStartingWith("BT.Application.Features")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "All handlers must live inside BT.Application.Features.* " +
                     "to enforce VSA feature-slice structure. " +
                     "Failing types: {0}", string.Join(", ",
                result.FailingTypes?.Select(t => t.Name) ?? []));
    }

    [Fact]
    public void Iam_Application_Artifacts_Should_Reside_In_Users_Feature()
    {
        var applicationRoot = Path.Combine(
            AssemblyReferences.RepoRoot,
            "src",
            "Backend",
            "Application",
            "BT.Application");

        var iamRoot = Path.Combine(applicationRoot, "Features", "IAM");
        var misplacedArtifacts = Directory
            .EnumerateFiles(iamRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path =>
                !path.Contains($"{Path.DirectorySeparatorChar}Users{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
                (path.Contains($"{Path.DirectorySeparatorChar}Commands{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
                 path.Contains($"{Path.DirectorySeparatorChar}EventHandlers{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
                 path.Contains($"{Path.DirectorySeparatorChar}Mappings{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
                 path.Contains($"{Path.DirectorySeparatorChar}IntegrationEvents{Path.DirectorySeparatorChar}", StringComparison.Ordinal)))
            .Select(path => Path.GetRelativePath(applicationRoot, path))
            .ToList();

        misplacedArtifacts.Should().BeEmpty(
            because: "IAM application artifacts belong under Features/IAM/Users unless a separate IAM feature is introduced. Found: {0}",
            string.Join(", ", misplacedArtifacts));
    }
}
