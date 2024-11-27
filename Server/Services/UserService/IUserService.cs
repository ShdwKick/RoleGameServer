using Server.Data;

namespace Server.Services.UserService;

public interface IUserService
{
    public Task<User> GetUserByToken();
    public Task<User> GetUserById(Guid userId);
    public List<Role> GetRoles();
}