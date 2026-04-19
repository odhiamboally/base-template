using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Directors;

public record RemoveDirectorRequest(Guid ClientId, Guid DirectorId);
