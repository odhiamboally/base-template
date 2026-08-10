using BT.Domain.Features.Shared.OrgSettings.Entities;
using BT.Domain.Shared.Contracts.Common;
using BT.Persistence.Features.Shared.DataContext;
using BT.Tests.Integration.TestFixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BT.Tests.Integration;

public class SharedTenantIsolationTests_Postgres : SharedTenantIsolationTests<PostgreSqlDbFixture>
{
    public SharedTenantIsolationTests_Postgres(PostgreSqlDbFixture fixture) : base(fixture) { }
}

