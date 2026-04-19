using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Common;

public record SendEmailRequest : IRequest<AppResponse<SendEmailResponse>>
{
    public string? To { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? MemberName { get; set; }
    public string? MemberNumber { get; set; }
}
