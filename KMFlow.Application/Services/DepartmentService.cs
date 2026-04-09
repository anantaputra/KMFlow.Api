using KMFlow.Application.DTOs.Departments;
using KMFlow.Application.Interfaces.Repositories;
using KMFlow.Application.Interfaces.Services;
using System.Text.RegularExpressions;

namespace KMFlow.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository; 
    }

    public async Task<ResponseList<DepartmentResponseDto>> GetAllDepartmentAsync()
    {
        return await _departmentRepository.GetAllDepartmentAsync();
    }

    public async Task<Response<DepartmentResponseDto>> GetDepartmentByIdAsync(Guid id)
    {
        return await _departmentRepository.GetDepartmentByIdAsync(id);
    }
    public async Task<Response<DepartmentResponseDto>> AddDepartmentAsync(Guid createdBy, CreateDepartmentDto dto)
    {
        try
        {
            // Business Logic 1: Validate input
            if (string.IsNullOrWhiteSpace(dto.Name))
                return new Response<DepartmentResponseDto>(false, "Department name is required", null);

            if (dto.Name.Length < 3 || dto.Name.Length > 100)
                return new Response<DepartmentResponseDto>(false, "Department name must be between 3 and 100 characters", null);

            // Business Logic 2: Check for duplicate name (case-insensitive)
            var existingDept = await _departmentRepository.GetByNameAsync(dto.Name);
            if (existingDept != null && existingDept.Data != null)
                return new Response<DepartmentResponseDto>(false, $"Department '{dto.Name}' already exists", null);

            return await _departmentRepository.AddDepartmentAsync(createdBy, dto);
        }
        catch (Exception ex)
        {
            return new Response<DepartmentResponseDto>(false, $"Error creating department: {ex.Message}", null);
        }
    }

    public async Task<Response<DepartmentResponseDto>> UpdateDepartmentAsync(Guid updatedBy, Guid id, UpdateDepartmentDto dto)
    {
        return await _departmentRepository.UpdateDepartmentAsync(updatedBy, id, dto);
    }

    public async Task<BaseResponse> DeleteDepartmentAsync(Guid id)
    {
        return await _departmentRepository.DeleteDepartmentAsync(id);
    }
}
