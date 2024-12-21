using System.Collections.ObjectModel;
using Server.Data;

namespace GraphQLServer.Services.RoleService;

public interface IRoleService
{
    List<Role> Roles { get; set; }
    Task<List<Role>> GetRoles();
}