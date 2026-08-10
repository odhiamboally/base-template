namespace BT.Domain.Features.ControlPlane.Tenants.Enums;

public enum TenantStatus
{
    Active = 0,
    Suspended = 1,
    Provisioning = 2,
    ProvisioningFailed = 3
}
