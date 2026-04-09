using KMFlow.Application.DTOs.Users;

namespace KMFlow.Application.Interfaces.Services;

public interface IUserService
{
    Task<ResponseList<UserResponseDto>> GetAllUserAsync();

    Task<Response<UserResponseDto>> GetUserByIdAsync(Guid id);
    
    Task<Response<UserResponseDto>> AddUserAsync(Guid createdBy, CreateUserDto dto);
    
    Task<Response<UserResponseDto>> UpdateUserAsync(Guid updatedBy, UpdateUserDto dto);
    
    Task<BaseResponse> DeleteUserAsync(Guid id);

    Task<ResponseList<UserResponseDto>> GetAllSmeAsync(Guid requestedBy);

    Task<Response<UserResponseDto>> PromoteToSmeAsync(Guid requestedBy, Guid targetUserId);

    Task<Response<UserResponseDto>> DemoteFromSmeAsync(Guid requestedBy, Guid targetUserId);

    Task<Response<UserResponseDto>> DisableSmeAsync(Guid requestedBy, Guid targetUserId);
}
