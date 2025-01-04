using System.Diagnostics;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Xml.Linq;
using GraphQLServer.Services.ChatService;
using GraphQLServer.Services.RecoveryService;
using GraphQLServer.Services.RoomService;
using Server.Services;

namespace GraphQLServer
{
    public class Mutation
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DataBaseConnection _dataBaseConnection;
        private readonly IEmailService _emailService;
        private readonly IChatService _chatService;
        private readonly IRoomService _roomService;
        private readonly IUserService _userService;


        public Mutation(IHttpContextAccessor httpContextAccessor, DataBaseConnection dataBaseConnection,
            IEmailService emailService, IChatService chatService, IRoomService roomService, IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _dataBaseConnection = dataBaseConnection;
            _emailService = emailService;
            _chatService = chatService;
            _roomService = roomService;
            _userService = userService;
        }

        public async Task<bool> SendRecoveryEmail(string address)
        {
            return await _emailService?.SendRecoveryEmail(address);
        }

        public async Task<bool> SendEmailConfirmationEMail()
        {
            return await _emailService?.SendEmailConfirmationEMail();
        }

        public async Task<string> CreateUser(UserForCreate user, Guid roleGuid)
        {
            return await _userService.CreateUser(user, roleGuid);
        }
        
        public async Task<string> LoginUser(string login, string password)
        {
            return await _userService.LoginUser(login, password);
        }

        // public async Task<string> LoginViaVK(string vk_token)
        // {
        //     var userData = await _dataBaseConnection.Users.FirstOrDefaultAsync(q => q.c_vk_token == vk_token);
        //
        //     if (userData == null)
        //         throw new ArgumentException("USER_NOT_FOUND_PROBLEM");
        //
        //     return await GenerateNewTokenForUser(userData);
        // }
        //
        // public async Task<string> LoginViaYandex(string yandex_token)
        // {
        //     var userData = await _dataBaseConnection.Users.FirstOrDefaultAsync(q => q.c_vk_token == yandex_token);
        //
        //     if (userData == null)
        //         throw new ArgumentException("USER_NOT_FOUND_PROBLEM");
        //
        //     return await GenerateNewTokenForUser(userData);
        // }
        //
        // public async Task<string> LoginViaGoogle(string google_token)
        // {
        //     var userData = await _dataBaseConnection.Users.FirstOrDefaultAsync(q => q.c_vk_token == google_token);
        //
        //     if (userData == null)
        //         throw new ArgumentException("USER_NOT_FOUND_PROBLEM");
        //
        //     return await GenerateNewTokenForUser(userData);
        // }
        

        //TODO: убрать после тестирования регистрации
        public async Task DeleteAllUsers()
        {
            _dataBaseConnection.Users.ExecuteDelete();
            await _dataBaseConnection.SaveChangesAsync();
        }

        public async Task<string> TryRefreshToken(string oldToken)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadToken(oldToken) as JwtSecurityToken;
            if (jwtToken == null) throw new ArgumentException("TOKEN_GENERATION_PROBLEM");

            var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (claim == null) throw new ArgumentException("TOKEN_GENERATION_PROBLEM");


            var user = await _dataBaseConnection.Users.FirstOrDefaultAsync(u => u.id.ToString() == claim.Value);
            if (user == null) throw new ArgumentException("TOKEN_GENERATION_PROBLEM");

            var authorizationToken =
                await _dataBaseConnection.Authorization.FirstOrDefaultAsync(q => q.id == user.f_authorization_token);
            if (authorizationToken == null) throw new ArgumentException("TOKEN_GENERATION_PROBLEM");

            var recivedTokenHash = Helpers.ComputeHash(oldToken);
            if (recivedTokenHash != authorizationToken.c_hash)
                throw new ArgumentException("CORRUPTED_TOKEN_DETECTED_PROBLEM");


            authorizationToken.c_token =
                new JwtSecurityTokenHandler().WriteToken(Helpers.GenerateNewToken(user.id.ToString()));
            authorizationToken.c_hash = Helpers.ComputeHash(authorizationToken.c_token);
            await _dataBaseConnection.SaveChangesAsync();
            return authorizationToken.c_token;
        }

        public async Task<bool> PasswordRecovery(string email, string code, string newPassword)
        {
            var user = await _dataBaseConnection.Users.FirstOrDefaultAsync(u => u.c_email == email);
            if (user == null)
                throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

            var recoveryCode = await _dataBaseConnection.RecoveryCodes.FirstOrDefaultAsync(u => u.c_email == email);
            if (recoveryCode == null)
                throw new ArgumentException("RECOVERY_CODE_NOT_FOUND_PROBLEM");

            if (DateTime.UtcNow > recoveryCode.d_expiration_time)
                throw new ArgumentException("RECOVERY_CODE_WAS_EXPIRED_PROBLEM");

            if (!recoveryCode.n_code.ToString().Equals(code))
                throw new ArgumentException("RECOVERY_CODE_NOT_CORRECT_PROBLEM");

            user.c_password = Helpers.ComputeHash(newPassword);
            await _dataBaseConnection.SaveChangesAsync();
            return true;
        }

        [Authorize]
        public async Task<bool> RefreshPassword(string oldPassword, string newPassword)
        {
            var user = await Helpers.GetUserFromHeader(_dataBaseConnection, _httpContextAccessor);

            var oldPasswordHash = Helpers.ComputeHash(oldPassword);
            if (oldPasswordHash != user.c_password)
                throw new ArgumentException("OLD_PASSWORD_NOT_CORRECT_PROBLEM");


            user.c_password = Helpers.ComputeHash(newPassword);
            await _dataBaseConnection.SaveChangesAsync();
            return true;
        }

        [Authorize]
        public async Task<Guid> CreatePrivateChat(Guid firstuserId, Guid seconduserId)
        {
            return await _chatService.CreatePrivateChat(firstuserId, seconduserId);
        }

        [Authorize]
        public async Task<Guid> CreateRoom(CreateRoom room)
        {
            return await _roomService.CreateRoom(room);
        }

        [Authorize]
        public async Task RemoveRoom(Guid roomId)
        {
            await _roomService.RemoveRoom(roomId);
        }

        [Authorize]
        public async Task ChangeRoomUsersList(Guid userId, Guid roomId, bool IsNeedAdd,
            [Service] ITopicEventSender eventSender, CancellationToken cancellationToken)
        {
            await _roomService.ChangeRoomUsersList(userId, roomId, IsNeedAdd, eventSender, cancellationToken);
        }

        [Authorize]
        public async Task<Message> SendMessageAsync(Message msg, Guid senderId, Guid chatId,
            [Service] ITopicEventSender eventSender, CancellationToken cancellationToken)
        {
            return await _chatService.SendMessageAsync(msg, senderId, chatId, eventSender, cancellationToken);
        }
    }
}