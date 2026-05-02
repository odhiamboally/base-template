using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Shared.Entities;
using BT.Persistence.Contracts.Implementations.Repositories;
using BT.Persistence.Features.Shared.DataContext;

namespace BT.Persistence.Features.Shared.EmailTemplates.Repositories;

internal sealed class SharedEmailTemplateRepository(SharedDBContext context) : Repository<EmailTemplate>(context), IEmailTemplateRepository { }
