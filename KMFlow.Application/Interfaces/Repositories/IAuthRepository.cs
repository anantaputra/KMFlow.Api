using KMFlow.Domain.Entities;

namespace KMFlow.Application.Interfaces.Repositories;

public interface IAuthRepository
{
    Task<User?> GetUserWithLoginDetailByEmail(string email);

    Task<User?> GetUserWithLoginDetailById(Guid id);
}
