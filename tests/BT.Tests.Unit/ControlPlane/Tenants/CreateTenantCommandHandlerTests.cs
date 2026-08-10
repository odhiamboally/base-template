using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.ControlPlane.Tenants.Commands;
using BT.Application.Features.ControlPlane.Tenants.Contracts;
using BT.Domain.Features.ControlPlane.Contracts;
using BT.Domain.Features.ControlPlane.Tenants.Entities;
using BT.Domain.Features.ControlPlane.Tenants.Enums;
using BT.Domain.Shared.Contracts.Common;
using BT.Infrastructure.Features.ControlPlane.Tenants.CommandHandlers;
using BT.SharedKernel.Extensions;
using BT.SharedKernel.Features.ControlPlane.Tenants.Dtos;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace BT.Tests.Unit.ControlPlane.Tenants;

public sealed class CreateTenantCommandHandlerTests
{
    private readonly IControlPlaneUnitOfWork _unitOfWork;
    private readonly IStampProvisioner _provisioner;
    private readonly ILogger<CreateTenantCommandHandler> _logger;
    private readonly IEncryptionService _encryptionService;
    private readonly ICurrentActorProvider _actorProvider;
    private readonly CreateTenantCommandHandler _handler;

    public CreateTenantCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IControlPlaneUnitOfWork>();
        _provisioner = Substitute.For<IStampProvisioner>();
        _logger = Substitute.For<ILogger<CreateTenantCommandHandler>>();
        _encryptionService = Substitute.For<IEncryptionService>();
        _actorProvider = Substitute.For<ICurrentActorProvider>();
        _actorProvider.ActorId.Returns("test-actor-id");
        _handler = new CreateTenantCommandHandler(_unitOfWork, _logger, _encryptionService, _provisioner, _actorProvider);
    }

    [Fact]
    public async Task Handle_WithIsolatedStamp_SetsStatusToProvisioning_AndCallsProvisioner()
    {
        // Arrange
        var stampId = Guid.NewGuid();
        var request = new CreateTenantRequest
        {
            DisplayName = "Test Tenant",
            Identifier = "test-tenant",
            DeploymentStampId = stampId,
            SubscriptionTier = "Enterprise",
            ContactEmail = "test@example.com",
            DatabaseProvider = "SqlServer",
            HostName = "test.example.com"
        };
        var command = new CreateTenantCommand(request);

        var stamp = new DeploymentStamp
        {
            Id = stampId,
            Name = "Isolated Stamp",
            IsolationTier = IsolationTier.Isolated,
            TargetResourceGroup = "test-rg",
            CreatedBy = "test-user"
        };

        _unitOfWork.DeploymentStamps.FirstOrDefaultAsync(
            Arg.Any<Expression<Func<DeploymentStamp, bool>>>(), 
            Arg.Any<CancellationToken>())
            .Returns(stamp);

        _unitOfWork.Tenants.AnyAsync(
            Arg.Any<Expression<Func<Tenant, bool>>>(), 
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(TenantStatus.Provisioning.ToDisplayString(), result.Data!.Status);

        await _unitOfWork.Tenants.Received(1).CreateAsync(Arg.Is<Tenant>(t => t.Status == TenantStatus.Provisioning), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
        await _provisioner.Received(1).ProvisionIsolatedStampAsync(
            Arg.Any<string>(), stamp.Name, "test-rg", "SqlServer", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPooledStamp_SetsStatusToActive_AndDoesNotCallProvisioner()
    {
        // Arrange
        var stampId = Guid.NewGuid();
        var request = new CreateTenantRequest
        {
            DisplayName = "Test Tenant",
            Identifier = "test-tenant",
            DeploymentStampId = stampId,
            SubscriptionTier = "Enterprise",
            ContactEmail = "test@example.com",
            DatabaseProvider = "SqlServer",
            HostName = "test.example.com"
        };
        var command = new CreateTenantCommand(request);

        var stamp = new DeploymentStamp
        {
            Id = stampId,
            Name = "Pooled Stamp",
            IsolationTier = IsolationTier.Pooled,
            TargetResourceGroup = "test-rg",
            CreatedBy = "test-user"
        };

        _unitOfWork.DeploymentStamps.FirstOrDefaultAsync(
            Arg.Any<Expression<Func<DeploymentStamp, bool>>>(), 
            Arg.Any<CancellationToken>())
            .Returns(stamp);

        _unitOfWork.Tenants.AnyAsync(
            Arg.Any<Expression<Func<Tenant, bool>>>(), 
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(TenantStatus.Active.ToDisplayString(), result.Data!.Status);

        await _unitOfWork.Tenants.Received(1).CreateAsync(Arg.Is<Tenant>(t => t.Status == TenantStatus.Active), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
        await _provisioner.DidNotReceiveWithAnyArgs().ProvisionIsolatedStampAsync(
            default!, default!, default!, default!, default);
    }

    [Fact]
    public async Task Handle_WithIsolatedStamp_WhenProvisionerThrows_SetsStatusToProvisioningFailed_AndReturnsFailure()
    {
        // Arrange
        var stampId = Guid.NewGuid();
        var request = new CreateTenantRequest
        {
            DisplayName = "Test Tenant",
            Identifier = "test-tenant",
            DeploymentStampId = stampId,
            SubscriptionTier = "Enterprise",
            ContactEmail = "test@example.com",
            DatabaseProvider = "PostgreSql",
            HostName = "test.example.com"
        };
        var command = new CreateTenantCommand(request);

        var stamp = new DeploymentStamp
        {
            Id = stampId,
            Name = "Isolated Stamp",
            IsolationTier = IsolationTier.Isolated,
            TargetResourceGroup = "test-rg",
            CreatedBy = "test-user"
        };

        _unitOfWork.DeploymentStamps.FirstOrDefaultAsync(
            Arg.Any<Expression<Func<DeploymentStamp, bool>>>(),
            Arg.Any<CancellationToken>())
            .Returns(stamp);

        _unitOfWork.Tenants.AnyAsync(
            Arg.Any<Expression<Func<Tenant, bool>>>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        _provisioner.ProvisionIsolatedStampAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("GitHub API returned 401"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — failure returned, tenant status set to ProvisioningFailed, and CompleteAsync called twice
        Assert.False(result.IsSuccess);
        Assert.Contains("provisioning could not be started", result.Message, StringComparison.OrdinalIgnoreCase);

        // CreateAsync was called once (tenant initially saved as Provisioning, but later mutated)
        await _unitOfWork.Tenants.Received(1).CreateAsync(
            Arg.Is<Tenant>(t => t.Status == TenantStatus.ProvisioningFailed), Arg.Any<CancellationToken>());

        // UpdateAsync was called once to flip to ProvisioningFailed
        await _unitOfWork.Tenants.Received(1).UpdateAsync(
            Arg.Is<Tenant>(t => t.Status == TenantStatus.ProvisioningFailed), Arg.Any<CancellationToken>());

        // CompleteAsync must have been called twice: once after create, once after failure update
        await _unitOfWork.Received(2).CompleteAsync(Arg.Any<CancellationToken>());
    }
}
