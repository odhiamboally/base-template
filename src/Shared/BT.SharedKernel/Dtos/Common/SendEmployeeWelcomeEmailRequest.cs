using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Common;


public record SendEmployeeWelcomeEmailRequest(Guid EmployeeId, string EmployeeNumber, string EmployeeName, string EmployeeEmail, string EmployeeType);
