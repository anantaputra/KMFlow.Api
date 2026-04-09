using KMFlow.Application.DTOs.Departments;

namespace KMFlow.Application.Interfaces.Services;

public interface IDepartmentService
{
    Task<ResponseList<DepartmentResponseDto>> GetAllDepartmentAsync();

    Task<Response<DepartmentResponseDto>> GetDepartmentByIdAsync(Guid id);
    
    Task<Response<DepartmentResponseDto>> AddDepartmentAsync(Guid createdBy, CreateDepartmentDto dto);
    
    Task<Response<DepartmentResponseDto>> UpdateDepartmentAsync(Guid updatedBy, Guid id, UpdateDepartmentDto dto);
    
    Task<BaseResponse> DeleteDepartmentAsync(Guid id);
}
