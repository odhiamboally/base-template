using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Common;

public record LookupResponse(int Id, string Code, string? Description);
