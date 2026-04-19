using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Directors;

public record AddDirectorRequest(
    Guid ClientId,
    string FullName,
    string RelationType,
    string IdentificationType,
    string IdentificationNumber,
    string? PhoneNumber,
    string? Email,
    decimal? SharePercentage
);
