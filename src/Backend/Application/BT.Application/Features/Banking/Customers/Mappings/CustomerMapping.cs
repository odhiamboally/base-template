using BT.SharedKernel.Extensions;
using BT.Domain.Features.Banking.Customers.Entities;
using BT.SharedKernel.Dtos.Banking.Customers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace BT.Application.Features.Banking.Customers.Mappings;

internal static class CustomerMapping
{
    public static CustomerResponse ToCustomerResponse(this Customer client) =>
        new(
            // Identity & Classification
            client.Id,
            client.ClientNumber,
            client.ClientType.ToDisplayString(),
            client.SegmentType.ToDisplayString(),
            client.SubSegmentType.ToDisplayString(),
            client.Status.ToDisplayString(),
            client.OpenedOn,

            // Corporate Details
            client.CorporateDetail.CompanyName,
            client.CorporateDetail.LineOfBusiness.ToDisplayString(),
            client.CorporateDetail.LineOfBusinessMoreInfo,
            client.CorporateDetail.NatureOfBusiness,
            client.CorporateDetail.IdentificationType.ToDisplayString(),
            client.CorporateDetail.RegistrationNumber,
            client.CorporateDetail.DateOfRegistration,
            client.CorporateDetail.RegisteredAt,
            client.CorporateDetail.RegisteredOffice,
            client.CorporateDetail.BusinessStartedYear,
            client.CorporateDetail.NumberOfEmployees,
            client.CorporateDetail.Comments,
            client.CorporateDetail.Website,
            client.CorporateDetail.TINNumber,

            // Relationship Manager
            client.RelationshipManagerId,
            $"{client.RelationshipManager?.FirstName} {client.RelationshipManager?.LastName}" ?? "—",

            // Address
            client.Address.ResidentialAddress,
            client.Address.Country,
            client.Address.Region,
            client.Address.Ward,
            client.Address.District,
            client.Address.BusinessAddress,
            client.Address.OfficeAddress,
            client.Address.MailingAddress,
            client.Address.Street,
            client.Address.ZipCode,
            client.Address.PhoneHome,
            client.Address.PhoneWork,
            client.Address.Mobile,
            client.Address.FaxNo,
            client.Address.LandMark,
            client.Address.Email,

            // Communication Prefs
            client.CommunicationPreference.CanSendGreetings,
            client.CommunicationPreference.CanSendAssociateSpecialOffer,
            client.CommunicationPreference.CanSendOurSpecialOffers,
            client.CommunicationPreference.StatementOnline,
            client.CommunicationPreference.MobileAlert,

            // Directors
            [.. client.Directors.Select(d => d.ToDirectorResponse())]
        );

    public static DirectorResponse ToDirectorResponse(this Director director) =>
        new(
            director.Id,
            director.FullName,
            director.RelationType.ToDisplayString(),
            director.IdentificationType.ToDisplayString(),
            director.IdentificationNumber,
            director.PhoneNumber,
            director.Email,
            director.SharePercentage,
            director.CreatedAt
        );

    public static Expression<Func<Customer, CustomerResponse>> AsResponse => client => new CustomerResponse(
        client.Id,
        client.ClientNumber,
        client.ClientType.ToDisplayString(), // EF can translate simple ToString() or use a helper
        client.SegmentType.ToDisplayString(),
        client.SubSegmentType.ToDisplayString(),
        client.Status.ToDisplayString(),
        client.OpenedOn,

        // Corporate Details
        client.CorporateDetail.CompanyName,
        client.CorporateDetail.LineOfBusiness.ToDisplayString(),
        client.CorporateDetail.LineOfBusinessMoreInfo,
        client.CorporateDetail.NatureOfBusiness,
        client.CorporateDetail.IdentificationType.ToDisplayString(),
        client.CorporateDetail.RegistrationNumber,
        client.CorporateDetail.DateOfRegistration,
        client.CorporateDetail.RegisteredAt,
        client.CorporateDetail.RegisteredOffice,
        client.CorporateDetail.BusinessStartedYear,
        client.CorporateDetail.NumberOfEmployees,
        client.CorporateDetail.Comments,
        client.CorporateDetail.Website,
        client.CorporateDetail.TINNumber,
        client.RelationshipManagerId,
        client.RelationshipManager != null ? $"{client.RelationshipManager.FirstName} {client.RelationshipManager.LastName}" : "—",

        // Address
        client.Address.ResidentialAddress,
        client.Address.Country,
        client.Address.Region,
        client.Address.Ward,
        client.Address.District,
        client.Address.BusinessAddress,
        client.Address.OfficeAddress,
        client.Address.MailingAddress,
        client.Address.Street,
        client.Address.ZipCode,
        client.Address.PhoneHome,
        client.Address.PhoneWork,
        client.Address.Mobile,
        client.Address.FaxNo,
        client.Address.LandMark,
        client.Address.Email,

        // Communication Prefs
        client.CommunicationPreference.CanSendGreetings,
        client.CommunicationPreference.CanSendAssociateSpecialOffer,
        client.CommunicationPreference.CanSendOurSpecialOffers,
        client.CommunicationPreference.StatementOnline,
        client.CommunicationPreference.MobileAlert,

        client.Directors.Select(d => new DirectorResponse(
            d.Id,
            d.FullName,
            d.RelationType.ToDisplayString(),
            d.IdentificationType.ToDisplayString(),
            d.IdentificationNumber,
            d.PhoneNumber,
            d.Email,
            d.SharePercentage,
            d.CreatedAt))
        
        .ToList()
    );

    
}
