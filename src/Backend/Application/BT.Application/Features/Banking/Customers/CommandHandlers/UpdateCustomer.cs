using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Extensions;
using BT.Application.Mappings;
using BT.Application.Utilities;
using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
using BT.Domain.Banking.Enums;
using BT.Domain.HR.Enums;
using BT.Domain.IAM.Enums;
using BT.Domain.Shared.Enums;
using BT.Domain.Banking.ValueObjects;
using BT.SharedKernel.Dtos.Banking.Customers;
using BT.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.Banking.Customers.CommandHandlers;


/// <summary>
/// Invalidation:
///   - Direct:  delete the entity entry so the next GetById call fetches fresh data.
///   - Version: bump the caller-scoped "customers" version to orphan list entries.
///
/// Both are necessary: without the direct deletion the entity detail page would
/// still show stale data even after the list refreshes.
/// </summary>
public record UpdateCustomerCommand(Guid Id, UpdateCustomerRequest UpdateCustomerRequest, string UserId) 
    : IRequest<AppResponse<CustomerResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("customers", Id.ToString())];
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("customers", UserId)];
        
}

internal sealed class UpdateCustomerCommandHandler(
    IBankingUnitOfWork _bankingUnitOfWork, 
    IHrUnitOfWork _hrUnitOfWork, 
    ILogger<UpdateCustomerCommandHandler> _logger)
    : IRequestHandler<UpdateCustomerCommand, AppResponse<CustomerResponse>>
{
    
    public async Task<AppResponse<CustomerResponse>> Handle(UpdateCustomerCommand command, CancellationToken ct)
    {
        try
        {
            var req = command.UpdateCustomerRequest;
            var customer = await _bankingUnitOfWork.CustomerRepository.FindByIdAsync(req.Id, ct).ConfigureAwait(false);
            if (customer is null)
                return AppResponse.Failure<CustomerResponse>($"Customer {req.Id} not found.");
                    
            // Verify new RM if it changed
            var rm = await _hrUnitOfWork.EmployeeRepository
                .FindByIdAsync(req.RelationshipManagerId, ct)
                .ConfigureAwait(false);

            if (rm is null)
                return AppResponse.Failure<CustomerResponse>("Selected Relationship Manager does not exist or is inactive.");
                    
            // Update via aggregate behaviours — never set properties directly
            customer.SetCorporateDetails(CorporateDetail.Create(
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
                
            ));

            customer.SetAddress(Address.Create(
                residentialAddress: req.ResidentialAddress,
                country: req.Country,
                region: req.Region,
                ward: req.Ward,
                district: req.District,
                mobile: req.Mobile,
                emailId: req.EmailId,
                businessAddress: req.BusinessAddress,
                officeAddress: req.OfficeAddress,
                mailingAddress: req.MailingAddress,
                street: req.Street,
                zipCode: req.ZipCode,
                phoneHome: req.PhoneHome,
                phoneWork: req.PhoneWork,
                faxNo: req.FaxNo,
                landMark: req.LandMark
            ));

            customer.SetCommunicationPreferences(CommunicationPreference.Create(
                canSendGreetings: req.CanSendGreetings,
                canSendAssociateSpecialOffer: req.CanSendAssociateSpecialOffer,
                canSendOurSpecialOffers: req.CanSendOurSpecialOffers,
                statementOnline: req.StatementOnline,
                mobileAlert: req.MobileAlert
            ));

            customer.AssignRelationshipManager(req.RelationshipManagerId);

            await _bankingUnitOfWork.CustomerRepository.UpdateAsync(customer).ConfigureAwait(false);
            await _bankingUnitOfWork.CompleteAsync(ct).ConfigureAwait(false);

            LogDefinitions.LogCustomerUpdated(_logger, customer.ClientNumber);

            return AppResponse.Success("Customer updated successfully.", customer.ToCustomerResponse());
        }
        catch (Exception ex)
        {
            LogDefinitions.LogCustomerUpdateFailed(_logger, command.UpdateCustomerRequest.Id, ex);
            throw;
        }
    }
}
