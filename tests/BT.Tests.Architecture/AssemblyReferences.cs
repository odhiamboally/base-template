using BT.Domain.Shared.Entities;
using BT.Persistence.Features.Shared.DataContext;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Lookups.Dtos;
using BT.SharedKernel.Validation.Features.Shared.Lookups.Validators;
using System.IO;
using System.Reflection;

namespace BT.Tests.Architecture;

/// <summary>
/// Single source of truth for all assembly handles used in architecture tests.
/// </summary>
/// <remarks>
/// Using a concrete type from each assembly as an anchor is intentional —
/// if a project is renamed or restructured, the anchor type breaks at compile
/// time rather than silently passing tests against the wrong assembly.
/// Never use <c>Assembly.Load("BT.Domain")</c> — string-based loading fails
/// silently when the assembly name changes.
/// </remarks>
internal static class AssemblyReferences
{
    internal static readonly Assembly Domain = typeof(BaseEntity).Assembly;
    internal static readonly Assembly Application = Assembly.LoadFrom(
        Path.GetFullPath(Path.Combine(
            FindRepoRoot(),
            "src", "Backend", "Application", "BT.Application", "bin", BuildConfiguration, "net10.0", "BT.Application.dll")));
    internal static readonly Assembly Persistence = typeof(SharedDBContext).Assembly;
    internal static readonly Assembly SharedKernel = typeof(LookupResponse).Assembly;
    internal static readonly Assembly SharedKernelValidation = typeof(GetLookupRequestValidator).Assembly;
    internal static string RepoRoot => FindRepoRoot();

    private static string BuildConfiguration
    {
        get
        {
            var assemblyDirectory = new DirectoryInfo(Path.GetDirectoryName(typeof(SharedDBContext).Assembly.Location)!);
            return assemblyDirectory.Parent?.Name
                ?? throw new DirectoryNotFoundException("Could not determine build configuration from persistence assembly output path.");
        }
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate repository root from architecture test output directory.");
    }
}
