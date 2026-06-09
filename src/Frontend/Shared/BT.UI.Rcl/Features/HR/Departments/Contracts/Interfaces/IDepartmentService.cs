using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.HR.Departments.Dtos;

namespace BT.UI.Rcl.Features.HR.Departments.Contracts.Interfaces;

public interface IDepartmentService
{
    Task<AppResponse<IReadOnlyList<DepartmentResponse>>> GetAsync();
    Task<AppResponse<PagedResponse<DepartmentResponse, Guid>>> SearchAsync(DepartmentSearchRequest request);
    Task<AppResponse<DepartmentResponse>> GetByIdAsync(Guid id);
    Task<AppResponse<DepartmentResponse>> CreateAsync(CreateDepartmentRequest request);
    Task<AppResponse<DepartmentResponse>> UpdateAsync(Guid id, UpdateDepartmentRequest request);
    Task<AppResponse<bool>> DeleteAsync(Guid id);
}
