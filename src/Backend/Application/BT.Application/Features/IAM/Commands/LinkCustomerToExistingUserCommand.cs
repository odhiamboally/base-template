using BT.SharedKernel.Dtos.Client;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.IAM.Commands;


public sealed record LinkCustomerToExistingUserCommand(
    string AppUserId,    // The employee's existing AppUser ID
    Guid CustomerId,     // The already-created Customer aggregate ID
    string UpdatedBy
) : IRequest<AppResponse<CustomerResponse>>;
