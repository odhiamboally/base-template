using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Features.Banking.Customers.Dtos;

public record CustomerListRequest(

    Guid? Cursor = null,
    int PageSize = 50
);
