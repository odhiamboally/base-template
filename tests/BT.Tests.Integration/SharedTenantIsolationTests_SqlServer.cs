using BT.Domain.Features.Shared.OrgSettings.Entities;
using BT.Domain.Shared.Contracts.Common;
using BT.Persistence.Features.Shared.DataContext;
using BT.Tests.Integration.TestFixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BT.Tests.Integration;

public class SharedTenantIsolationTests_SqlServer : SharedTenantIsolationTests<MsSqlDbFixture>
{
    public SharedTenantIsolationTests_SqlServer(MsSqlDbFixture fixture) : base(fixture) { }
}

