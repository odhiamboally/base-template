using BT.Domain.Features.HR.Departments.Entities;
using BT.Domain.Shared.Contracts.Common;
using BT.Persistence.Features.HR.DataContext;
using BT.Tests.Integration.TestFixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BT.Tests.Integration;

public class HrTenantIsolationTests_Postgres : HrTenantIsolationTests<PostgreSqlDbFixture>
{
    public HrTenantIsolationTests_Postgres(PostgreSqlDbFixture fixture) : base(fixture) { }
}

