using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT.SharedKernel.Dtos.Directors;

public record UpdateDirectorRequest(
    Guid DirectorId,
    Guid ClientId,
    string FullName,
    string RelationType,
    string IdentificationType,
    string IdentificationNumber,
    string? PhoneNumber,
    string? Email,
    decimal? SharePercentage
);
