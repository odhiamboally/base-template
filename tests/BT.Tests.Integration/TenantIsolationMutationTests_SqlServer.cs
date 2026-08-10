using BT.Domain.Features.IAM.Menus.Entities;
using BT.Domain.Shared.Contracts.Common;
using BT.Persistence.Features.IAM.DataContext;
using BT.Tests.Integration.TestFixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BT.Tests.Integration;

public class TenantIsolationMutationTests_SqlServer : TenantIsolationMutationTests<MsSqlDbFixture>
{
    public TenantIsolationMutationTests_SqlServer(MsSqlDbFixture fixture) : base(fixture) { }
}

