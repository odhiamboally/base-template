using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Entities;
using BT.Persistence.DataContext;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Validation.Validators.Lookups;
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
    internal static readonly Assembly Application = typeof(IUnitOfWork).Assembly;  // public interface
    internal static readonly Assembly Persistence = typeof(DBContext).Assembly;
    internal static readonly Assembly SharedKernel = typeof(LookupResponse).Assembly;
    internal static readonly Assembly SharedKernelValidation = typeof(GetLookupRequestValidator).Assembly;
}
