using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Server.Data;

namespace Server.Services;

public interface IUserService
{
    public Task<User> GetUserByToken();
    public Task<User> GetUserById(Guid userId);
}