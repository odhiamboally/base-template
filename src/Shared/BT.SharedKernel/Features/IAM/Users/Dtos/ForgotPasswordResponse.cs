namespace BT.SharedKernel.Features.IAM.Users.Dtos;

public sealed record ForgotPasswordResponse(string DeliveryMethod, bool RequiresCode);
