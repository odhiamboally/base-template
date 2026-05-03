using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Features.Banking.Customers.Dtos;

public record RemoveDirectorRequest(Guid ClientId, Guid DirectorId);
