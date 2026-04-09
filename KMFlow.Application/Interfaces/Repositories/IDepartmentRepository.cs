using KMFlow.Application.DTOs;
using KMFlow.Application.DTOs.Departments;

namespace KMFlow.Application.Interfaces.Repositories;

public interface IDepartmentRepository
{
    Task<ResponseList<DepartmentResponseDto>> GetAllDepartmentAsync();

    Task<Response<DepartmentResponseDto>> GetDepartmentByIdAsync(Guid id);

    Task<Response<DepartmentResponseDto>> AddDepartmentAsync(Guid createdBy, CreateDepartmentDto dto);

    Task<Response<DepartmentResponseDto>> UpdateDepartmentAsync(Guid updatedBy, Guid id, UpdateDepartmentDto dto);

    Task<BaseResponse> DeleteDepartmentAsync(Guid id);

    Task<Response<DepartmentResponseDto>> GetByNameAsync(string name);
}
