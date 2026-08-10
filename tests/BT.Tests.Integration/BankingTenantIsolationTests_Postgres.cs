using BT.Domain.Features.Banking.Customers.Entities;
using BT.Domain.Features.Banking.Customers.Enums;
using BT.Domain.Features.Banking.Customers.ValueObjects;
using BT.Domain.Shared.Contracts.Common;
using BT.Persistence.Features.Banking.DataContext;
using BT.Tests.Integration.TestFixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BT.Tests.Integration;

public class BankingTenantIsolationTests_Postgres : BankingTenantIsolationTests<PostgreSqlDbFixture>
{
    public BankingTenantIsolationTests_Postgres(PostgreSqlDbFixture fixture) : base(fixture) { }
}

