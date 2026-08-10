using BT.Domain.Features.IAM.Menus.Entities;
using BT.Domain.Shared.Contracts.Common;
using BT.Persistence.Features.IAM.DataContext;
using BT.Tests.Integration.TestFixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BT.Tests.Integration;

public class TenantIsolationMutationTests_PostgreSql : TenantIsolationMutationTests<PostgreSqlDbFixture>
{
    public TenantIsolationMutationTests_PostgreSql(PostgreSqlDbFixture fixture) : base(fixture) { }
}

