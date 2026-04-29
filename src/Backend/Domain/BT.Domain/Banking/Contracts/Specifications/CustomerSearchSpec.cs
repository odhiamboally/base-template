using BT.Domain.Banking.Contracts.Specifications;
using BT.Domain.Shared.Contracts.Specifications;
using BT.Domain.Banking.Entities;
using BT.Domain.Banking.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Domain.Banking.Contracts.Specifications;

public class CustomerSearchSpec : Specification<Customer, Guid>, ICustomerSearchSpec
{
    public CustomerSearchSpec(
        string? globalSearch,
        CustomerType? clientType,
        SegmentType? segmentType,
        SubSegmentType? subSegmentType,
        IdentificationType? identificationType,
        LineOfBusiness? lineOfBusiness,
        CustomerStatus? status,
        Guid? relationshipManagerId,
        Guid? cursor,
        int pageSize

    )
    {
        AddCriteria(client =>
            (string.IsNullOrWhiteSpace(globalSearch) ||
                client.ClientNumber.Contains(globalSearch) ||
                client.CorporateDetail.CompanyName.Contains(globalSearch) ||
                client.CorporateDetail.RegistrationNumber.Contains(globalSearch) ||
                (client.CorporateDetail.TINNumber != null && client.CorporateDetail.TINNumber.Contains(globalSearch)) ||
                (client.Address.Mobile != null && client.Address.Mobile.Contains(globalSearch)) ||
                (client.Address.Email != null && client.Address.Email.Contains(globalSearch)))

            && (!clientType.HasValue || client.ClientType == clientType.Value)
            && (!segmentType.HasValue || client.SegmentType == segmentType.Value)
            && (!subSegmentType.HasValue || client.SubSegmentType == subSegmentType.Value)
            && (!identificationType.HasValue || client.CorporateDetail.IdentificationType == identificationType.Value)
            && (!lineOfBusiness.HasValue || client.CorporateDetail.LineOfBusiness == lineOfBusiness.Value)
            && (!status.HasValue || client.Status == status.Value)
            && (!relationshipManagerId.HasValue || client.RelationshipManagerId == relationshipManagerId.Value)
        );

        AddInclude(c => c.RelationshipManager!);
        AddOrderBy(c => c.Id);

        // Only set the cursor if it actually exists. If null, the Evaluator knows to start from the beginning.
        if (cursor.HasValue && cursor.Value != Guid.Empty)
        {
            SetCursor(cursor.Value, c => c.Id > cursor.Value);
            //SetCursor(cursor.Value, "Id");
        }

        SetTake(Math.Clamp(pageSize, 1, 50)); // defensive — spec shouldn't trust its caller
        EnableSplitQuery();
    }

}

