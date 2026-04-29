namespace BT.SharedKernel.Dtos.Auth;


public record UnlockRequest(string Password, string? Email = null, string? EmployeeNumber = null);