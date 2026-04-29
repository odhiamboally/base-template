using BT.SharedKernel.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BT.SharedKernel.Dtos.Auth;


public sealed record SendEmailOtpRequest
{
    [Required]
    public string UserId { get; init; } = string.Empty;
    public string Purpose { get; init; } = "Login";
}