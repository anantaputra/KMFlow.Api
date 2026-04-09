
using KMFlow.Application.DTOs.Users;

namespace KMFlow.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<ResponseList<UserResponseDto>> GetAllUserAsync();

    Task<Response<UserResponseDto>> GetUserByIdAsync(Guid id);

    Task<Response<UserResponseDto>> AddUserAsync(Guid createdBy, CreateUserDto dto);

    Task<Response<UserResponseDto>> UpdateUserAsync(Guid updatedBy, UpdateUserDto dto);

    Task<BaseResponse> DeleteUserAsync(Guid id);

    Task<ResponseList<UserResponseDto>> GetUsersByRoleAsync(string roleName);

    Task<Response<UserResponseDto>> SetUserRoleAsync(Guid updatedBy, Guid targetUserId, string roleName);

    //Task<Response<UserResponseDto>> GetByNameAsync(string name);
}
