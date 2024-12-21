using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Server.Data;

namespace GraphQLServer.Services.RoleService;

public class RoleService : IRoleService
{
    private readonly IMemoryCache _cache;
    private readonly DataBaseConnection _dataBaseConnection;

    public List<Role> Roles { get; set; }

    public RoleService(IMemoryCache cache, DataBaseConnection dataBaseConnection)
    {
        _cache = cache;
        _dataBaseConnection = dataBaseConnection;
    }


    public async Task<List<Role>> GetRoles()
    {
        if (_cache.TryGetValue("Roles", out List<Role> roles))
        {
            return roles!;
        }
        else
        {
            roles = await _dataBaseConnection.Roles.ToListAsync();
            _cache.Set("Roles", roles,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
                });
            return roles;
        }
    }
}