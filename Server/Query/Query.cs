using Server.Services.EmailService;
using Server.Services.RoomService;
using HotChocolate.Authorization;
using Server.Data;
using Server.Services.UserService;

namespace GraphQLServer
{

    public class Query
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IRoomService _roomService;
        
        public Guid GlobalChatGuid { get; set; }

        public Query(IUserService userService, IEmailService emailService, RoomService roomService)
        {
            _userService = userService;
            _emailService = emailService;
            _roomService = roomService;
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
            return await _userService.GetUserByToken();
        }

        [Authorize]
        public async Task<User> GetUserById(Guid userId)
        {
            return await _userService.GetUserById(userId);
        }

        [Authorize]
        public List<Role> GetRoles()
        {
            return _userService.GetRoles();
        }

        [Authorize]
        public async Task<Guid> GetRoomChatId(Guid roomId)
        {
            return await _roomService.GetRoomChatId(roomId);
        }

        [Authorize]
        public async Task<List<Message>> GetRoomChatMessages(Guid roomId, Guid lastMessage, int amount = 50)
        {
            return await _roomService.GetRoomChatMessages(roomId, lastMessage, amount);
        }

        public async Task SendRecoveryEmail(string address)
        {
            await _emailService.SendRecoveryEmail(address);
        }

        public async Task SendEmailConfirmationEMail()
        {
            await _emailService.SendEmailConfirmationEMail();
        }

        [Authorize]
        public async Task SendNewsEMail(Guid newsGuid)
        {
            await _emailService.SendNewsEMail(newsGuid);
        }
    }
}