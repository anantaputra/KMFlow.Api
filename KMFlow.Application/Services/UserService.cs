using KMFlow.Application.DTOs.Users;
using KMFlow.Application.Interfaces.Repositories;
using KMFlow.Application.Interfaces.Services;
using System.Text.RegularExpressions;

namespace KMFlow.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _UserRepository;
    private readonly IKnowledgeRepository _knowledgeRepository;

    public UserService(IUserRepository UserRepository, IKnowledgeRepository knowledgeRepository)
    {
        _UserRepository = UserRepository;
        _knowledgeRepository = knowledgeRepository;
    }

    private async Task<string?> GetAdminValidationErrorAsync(Guid requestedBy)
    {
        var me = await _UserRepository.GetUserByIdAsync(requestedBy);
        if (!me.Status || me.Data == null)
        {
            return "User tidak ditemukan";
        }

        if (!string.Equals(me.Data.RoleName, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return "Hanya Admin yang bisa mengakses fitur ini";
        }

        return null;
    }

    public async Task<ResponseList<UserResponseDto>> GetAllUserAsync()
    {
        return await _UserRepository.GetAllUserAsync();
    }

    public async Task<Response<UserResponseDto>> GetUserByIdAsync(Guid id)
    {
        return await _UserRepository.GetUserByIdAsync(id);
    }
    public async Task<Response<UserResponseDto>> AddUserAsync(Guid createdBy, CreateUserDto dto)
    {
        try
        {
            // Business Logic 1: Validate input
            if (string.IsNullOrWhiteSpace(dto.Name))
                return new Response<UserResponseDto>(false, "User name is required", null);

            if (dto.Name.Length < 3 || dto.Name.Length > 100)
                return new Response<UserResponseDto>(false, "User name must be between 3 and 100 characters", null);

            // Business Logic 2: Check for duplicate name (case-insensitive)
            //var existingDept = await _UserRepository.GetByNameAsync(dto.Name);
            //if (existingDept != null && existingDept.Data != null)
            //    return new Response<UserResponseDto>(false, $"User '{dto.Name}' already exists", null);

            return await _UserRepository.AddUserAsync(createdBy, dto);
        }
        catch (Exception ex)
        {
            return new Response<UserResponseDto>(false, $"Error creating User: {ex.Message}", null);
        }
    }

    public async Task<Response<UserResponseDto>> UpdateUserAsync(Guid updatedBy, UpdateUserDto dto)
    {
        return await _UserRepository.UpdateUserAsync(updatedBy, dto);
    }

    public async Task<BaseResponse> DeleteUserAsync(Guid id)
    {
        return await _UserRepository.DeleteUserAsync(id);
    }

    public async Task<ResponseList<UserResponseDto>> GetAllSmeAsync(Guid requestedBy)
    {
        var adminError = await GetAdminValidationErrorAsync(requestedBy);
        if (!string.IsNullOrWhiteSpace(adminError))
        {
            return new ResponseList<UserResponseDto>(false, adminError, new List<UserResponseDto>());
        }

        return await _UserRepository.GetUsersByRoleAsync("SME");
    }

    public async Task<Response<UserResponseDto>> PromoteToSmeAsync(Guid requestedBy, Guid targetUserId)
    {
        var adminError = await GetAdminValidationErrorAsync(requestedBy);
        if (!string.IsNullOrWhiteSpace(adminError))
        {
            return new Response<UserResponseDto>(false, adminError, null);
        }

        return await _UserRepository.SetUserRoleAsync(requestedBy, targetUserId, "SME");
    }

    public async Task<Response<UserResponseDto>> DemoteFromSmeAsync(Guid requestedBy, Guid targetUserId)
    {
        var adminError = await GetAdminValidationErrorAsync(requestedBy);
        if (!string.IsNullOrWhiteSpace(adminError))
        {
            return new Response<UserResponseDto>(false, adminError, null);
        }

        var targetUser = await _UserRepository.GetUserByIdAsync(targetUserId);
        if (!targetUser.Status || targetUser.Data == null)
        {
            return new Response<UserResponseDto>(false, "User tidak ditemukan", null);
        }

        if (string.Equals(targetUser.Data.RoleName, "SME", StringComparison.OrdinalIgnoreCase))
        {
            var validation = await _knowledgeRepository.ValidateSmeCanBeDisabledAsync(targetUserId);
            if (!validation.Status)
            {
                return new Response<UserResponseDto>(false, validation.Message, null);
            }
        }

        return await _UserRepository.SetUserRoleAsync(requestedBy, targetUserId, "User");
    }

    public async Task<Response<UserResponseDto>> DisableSmeAsync(Guid requestedBy, Guid targetUserId)
    {
        return await DemoteFromSmeAsync(requestedBy, targetUserId);
    }
}
