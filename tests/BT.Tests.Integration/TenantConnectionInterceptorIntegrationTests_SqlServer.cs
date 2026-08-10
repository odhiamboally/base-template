using BT.Domain.Shared.Contracts.Common;
using BT.Persistence.Common.Interceptors;
using BT.Persistence.Features.Shared.DataContext;
using BT.Tests.Integration.TestFixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BT.Tests.Integration;

public class TenantConnectionInterceptorIntegrationTests_SqlServer : TenantConnectionInterceptorIntegrationTests<MsSqlDbFixture>
{
    public TenantConnectionInterceptorIntegrationTests_SqlServer(MsSqlDbFixture fixture) : base(fixture) { }
}

