using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.Banking.Customers.Contracts.Interfaces;
using BT.Application.Features.Banking.Customers.IntegrationEvents;
using BT.Application.Features.HR.Employees.IntegrationEvents;
using BT.Application.Features.IAM.Users.IntegrationEvents;
using BT.SharedKernel.Extensions;
using BT.Application.Features.Banking.Customers.Mappings;
using BT.Application.Features.HR.Employees.Mappings;
using BT.Application.Features.IAM.Users.Mappings;
using BT.Application.Features.Shared.EmailTemplates.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.Banking.Customers.Entities;
using BT.Domain.Features.Banking.Customers.Enums;
using BT.Domain.Features.Banking.Customers.ValueObjects;
using BT.SharedKernel.Dtos.Banking.Customers;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BT.Application.Features.Banking.Customers.CommandHandlers;

/// <summary>
/// Invalidation: bump the version token for "customers" in the current user scope.
/// No entity key to delete (the entity does not exist in cache yet).
/// The caller's versioned list entries are orphaned in O(1).
/// </summary>
public sealed record CreateCustomerCommand(CreateCustomerRequest CreateCustomerRequest, string UserId) 
    : IRequest<AppResponse<CustomerResponse>>, ICacheInvalidatorRequest
{
    // No direct keys — new entity, nothing cached yet.
    public IReadOnlyList<string> DirectInvalidationKeys => [];

    // Bump the scoped "customers" version so the caller sees the new entry on next list load.
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("customers", UserId)];
        

}
    
internal sealed class CreateCustomerCommandHandler(
    IBankingUnitOfWork _bankingUnitOfWork,
    ILogger<CreateCustomerCommandHandler> _logger,
    ICustomerNumberGenerator _clientNumberGenerator

) : IRequestHandler<CreateCustomerCommand, AppResponse<CustomerResponse>>
{
    public async Task<AppResponse<CustomerResponse>> Handle(CreateCustomerCommand command, CancellationToken ct)
    {
        try
        {
            var req = command.CreateCustomerRequest;
            var clientNumber = await _clientNumberGenerator.GenerateAsync(ct).ConfigureAwait(false);

            // Build owned entities
            var corporateDetails = CorporateDetail.Create(
                req.CompanyName,
                req.LineOfBusiness.ToEnum<LineOfBusiness>(),
                req.NatureOfBusiness,
                req.IdentificationType.ToEnum<IdentificationType>(),
                req.RegistrationNumber,
                req.DateOfRegistration,
                req.LineOfBusinessMoreInfo,
                req.RegisteredAt,
                req.RegisteredOffice,
                req.BusinessStartedYear,
                req.NumberOfEmployees,
                req.Website,
                req.TINNumber,
                req.ClientClassification,
                req.Comments
                
            );

            var address = Address.Create(
                req.ResidentialAddress,
                req.Country,
                req.Region,
                req.Ward,
                req.District,
                req.Mobile,
                req.Email,
                req.BusinessAddress,
                req.OfficeAddress,
                req.MailingAddress,
                req.Street,
                req.ZipCode,
                req.PhoneHome,
                req.PhoneWork,
                req.FaxNo,
                req.LandMark
            );

            var communicationPrefs = CommunicationPreference.Create(
                canSendGreetings: req.CanSendGreetings,
                canSendAssociateSpecialOffer: req.CanSendAssociateSpecialOffer,
                canSendOurSpecialOffers: req.CanSendOurSpecialOffers,
                statementOnline: req.StatementOnline,
                mobileAlert: req.MobileAlert
            );

            // Create aggregate root — domain rules enforced inside
            var client = Customer.Create(
                clientNumber,
                req.CompanyName,
                req.ClientType.ToEnum<CustomerType>(),
                req.SegmentType.ToEnum<SegmentType>(),
                req.SubSegmentType.ToEnum<SubSegmentType>(),
                req.RelationshipManagerId,
                req.OpenedOn,
                corporateDetails,
                address,
                communicationPrefs,
                string.Empty // ToDo: Should get currentuser
            );

            await _bankingUnitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _bankingUnitOfWork.CustomerRepository.CreateAsync(client, ct).ConfigureAwait(false);
                return true;
            }, ct).ConfigureAwait(false);

            LogDefinitions.LogCustomerCreated(_logger, client.ClientNumber, client.CorporateDetail.CompanyName);
            return AppResponse.Success($"Customer {client.ClientNumber} created successfully.", client.ToCustomerResponse());

        }
        catch (ArgumentException ex)
        {
            LogDefinitions.LogCustomerCreateValidationFailed(_logger, ex);
            return AppResponse.Failure<CustomerResponse>(ex.Message);
        }
        catch (Exception ex)
        {
            LogDefinitions.LogCustomerCreateFailed(_logger, ex);
            throw;
        }
    }
}
