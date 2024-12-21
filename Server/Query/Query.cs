using GraphQLServer.Services.ChatService;
using GraphQLServer.Services.RoleService;
using HotChocolate.Authorization;
using Server.Data;
using Server.Services;

namespace GraphQLServer
{

    public class Query
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IChatService _chatService;

        public Query(IUserService userService, IRoleService roleService, IChatService chatService)
        {
            _userService = userService;
            _roleService = roleService;
            _chatService = chatService;
        }


        public DateTime GetServerCurrentDateTime()
        {
            return DateTime.Now;
        }

        public DateTime GetServerCurrentUTCDateTime()
        {
            return DateTime.UtcNow;
        }


        [Authorize]
        public async Task<User> GetUserByToken()
        {
            return await _userService?.GetUserByToken();
        }

        [Authorize]
        public async Task<User> GetUserById(Guid userId)
        {
            return await _userService?.GetUserById(userId);
        }

        [Authorize]
        public async Task<List<Role>> GetRoles()
        {
            return await _roleService?.GetRoles();
        }

        [Authorize]
        public async Task<Guid> GetRoomChatId(Guid roomId)
        {
            return await _chatService?.GetRoomChatId(roomId);
        }

        [Authorize]
        public async Task<List<Message>> GetRoomChatMessages(Guid roomId, Guid lastMessage, int amount = 50)
        {
            return await _chatService?.GetRoomChatMessages(roomId, lastMessage, amount);
        }
    }
}