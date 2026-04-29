using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Banking.Customers;

public record DirectorResponse(
    Guid Id,
    string FullName,
    string RelationType,
    string IdentificationType,
    string IdentificationNumber,
    string? PhoneNumber,
    string? Email,
    decimal? SharePercentage,
    DateTimeOffset DateAdded
);

