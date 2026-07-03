using BT.SharedKernel.Features.Shared.Reporting.Dtos;

namespace BT.Application.Features.Shared.Reporting.Contracts.Interfaces;

public interface IPdfReportService
{
    PdfReportResponse Generate(PdfReportRequest request);
}
