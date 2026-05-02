using BT.Domain.Features.Shared.EmailTemplates.Entities;
using BT.Domain.Shared.Contracts.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Features.Shared.EmailTemplates.Contracts.Repositories;

public interface IEmailTemplateRepository : IRepository<EmailTemplate>
{
}
