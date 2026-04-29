using BT.SharedKernel.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Contracts.Interfaces.Services;

public interface ISmsService
{
    Task<AppResponse<bool>> TwilioSendAsync(string phoneNumber, string message);
}
