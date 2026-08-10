using System;
using System.Threading.Tasks;
using BT.Domain.Features.IAM.Menus.Entities;
using BT.Domain.Shared.Contracts.Common;
using BT.Persistence.Features.IAM.DataContext;
using BT.Tests.Integration.TestFixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BT.Tests.Integration;

public class IamTenantIsolationTests_Postgres : IamTenantIsolationTests<PostgreSqlDbFixture>
{
    public IamTenantIsolationTests_Postgres(PostgreSqlDbFixture fixture) : base(fixture) { }
}

