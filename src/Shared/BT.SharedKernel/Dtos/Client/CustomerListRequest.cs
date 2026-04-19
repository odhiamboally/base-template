using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Dtos.Client;

public record CustomerListRequest(

    Guid? Cursor = null,
    int PageSize = 50
);
