using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Xml.Linq;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Helpers;

namespace GraphQLServer
{
    public class Mutation
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DataBaseConnection _dataBaseConnection;

        public Mutation(IHttpContextAccessor httpContextAccessor, DataBaseConnection dataBaseConnection)
        {
            _httpContextAccessor = httpContextAccessor;
            _dataBaseConnection = dataBaseConnection;
        }

        public async Task<string> CreateUser(UserForCreate user, Guid roleGuid)
        {
            var usr = await _dataBaseConnection.Users.FirstOrDefaultAsync(q => q.c_email == user.c_email || q.c_nickname == user.c_nickname);
            if (usr != null)
            {
                throw new ArgumentException("EMAIL_OR_NAME_EXIST_PROBLEM");
            };

            usr = new UserData
            {
                id = Guid.NewGuid(),
                c_nickname = user.c_nickname,
                c_email = user.c_email,
                c_password = Helpers.ComputeHash(user.c_password),
                d_registration_date = DateTime.UtcNow,
                c_vk_token = user.c_vk_token,
                c_yandex_token = user.c_yandex_token,
                c_google_token = user.c_google_token,
            };

            var role = _dataBaseConnection.Roles.FirstOrDefault(q => q.id == roleGuid);
            if(role == null)
                role = _dataBaseConnection.Roles.FirstOrDefault(q => q.c_dev_name == "User");

            if (role != null)
                usr.f_role = (Guid)role.id;

            var newToken = new AuthorizationToken();
            newToken.c_token = new JwtSecurityTokenHandler().WriteToken(Helpers.GenerateNewToken(usr.id.ToString()));
            newToken.c_hash = Helpers.ComputeHash(newToken.c_token);
            usr.f_authorization_token = (Guid)newToken.id;

            await _dataBaseConnection.Authorization.AddAsync(newToken);
            //await _dataBaseConnection.SaveChangesAsync();
            await _dataBaseConnection.Users.AddAsync(usr);

            try
            {
                await _dataBaseConnection.SaveChangesAsync();
            }
            catch (Exception EX_NAME)
            {
                Debug.WriteLine(EX_NAME.Message);
            }

            
            return newToken.c_token;
        }


        public async Task<string> LoginUser(string login, string password)
        {
            string passwordHash = Helpers.ComputeHash(password);

            var user = await _dataBaseConnection.Users.FirstOrDefaultAsync(q => q.c_email == login && q.c_password == passwordHash);
            if (user == null)
                throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

            return await GenerateNewTokenForUser(user);
        }


        public async Task<string> LoginViaVK(string vk_token)
        {
            var userData = await _dataBaseConnection.Users.FirstOrDefaultAsync(q => q.c_vk_token == vk_token);

            if (userData == null)
                throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

            return await GenerateNewTokenForUser(userData);
        }

        public async Task<string> LoginViaYandex(string yandex_token)
        {
            var userData = await _dataBaseConnection.Users.FirstOrDefaultAsync(q => q.c_vk_token == yandex_token);

            if (userData == null)
                throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

            return await GenerateNewTokenForUser(userData);
        }

        public async Task<string> LoginViaGoogle(string google_token)
        {
            var userData = await _dataBaseConnection.Users.FirstOrDefaultAsync(q => q.c_vk_token == google_token);

            if (userData == null)
                throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

            return await GenerateNewTokenForUser(userData);
        }

        [GraphQLIgnore]
        public async Task<string> GenerateNewTokenForUser(UserData user)
        {
            var token = new JwtSecurityTokenHandler().WriteToken(Helpers.GenerateNewToken(user.id.ToString()));

            var authorizationToken = await _dataBaseConnection.Authorization.FirstOrDefaultAsync(q => q.id == user.f_authorization_token);
            if (authorizationToken == null)
                throw new ArgumentException("TOKEN_GENERATION_PROBLEM");

            authorizationToken.c_token = token;
            authorizationToken.c_hash = Helpers.ComputeHash(authorizationToken.c_token);
            await _dataBaseConnection.SaveChangesAsync();

            return token;
        }

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

            var authorizationToken = await _dataBaseConnection.Authorization.FirstOrDefaultAsync(q => q.id == user.f_authorization_token);
            if (authorizationToken == null) throw new ArgumentException("TOKEN_GENERATION_PROBLEM");

            var recivedTokenHash = Helpers.ComputeHash(oldToken);
            if (recivedTokenHash != authorizationToken.c_hash)
                throw new ArgumentException("CORRUPTED_TOKEN_DETECTED_PROBLEM");


            authorizationToken.c_token = new JwtSecurityTokenHandler().WriteToken(Helpers.GenerateNewToken(user.id.ToString()));
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

            if (!recoveryCode.n_code.ToString().Equals(code))
                throw new ArgumentException("RECOVERY_CODE_NOT_CORRECT_PROBLEM");

            if (DateTime.UtcNow > recoveryCode.d_expiration_time)
                throw new ArgumentException("RECOVERY_CODE_WAS_EXPIRED_PROBLEM");

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
            if (firstuserId == Guid.Empty || seconduserId == Guid.Empty)
            {
                throw new ArgumentException("EMPTY_MESSAGE_SENDER_OR_CHAT_GUID_PROBLEM");
            }

            var chat = await _dataBaseConnection.PrivateChat.FirstOrDefaultAsync(q => q.f_firstuser == firstuserId && q.f_seconduser == seconduserId);

            if (chat == null)
            {
                chat = new PrivateChat
                {
                    id = Guid.NewGuid(),
                    f_firstuser = firstuserId,
                    f_seconduser = seconduserId
                };
                _dataBaseConnection.PrivateChat.Add(chat);
                await _dataBaseConnection.SaveChangesAsync();
            }
            return (Guid)chat.id;

        }
        [Authorize]
        public async Task<Guid> CreateRoom(CreateRoom room)
        {
            if (room.f_owner_id == Guid.Empty)
            {
                throw new ArgumentException("EMPTY_OWNER_ID_PROBLEM");
            }
            var user = await _dataBaseConnection.Users.FirstOrDefaultAsync(q => q.id == room.f_owner_id);
            if (user == null)
                throw new ArgumentException("ROOM_NOT_EXIST_PROBLEM");

            var newRoom = new Room()
            {
                id = Guid.NewGuid(),
                c_name = room.c_name,
                c_description = room.c_description,
                f_owner_id = room.f_owner_id,
                n_size = 5,
                f_game = null,
            };

            var roomUsers = new RoomUsers()
            {
                id = Guid.NewGuid(),
                f_room_id = newRoom.id,
                f_user_id = room.f_owner_id,
                b_is_master = true,
            };
            await _dataBaseConnection.Room.AddAsync(newRoom);
            await _dataBaseConnection.RoomUsers.AddAsync(roomUsers);
            await _dataBaseConnection.SaveChangesAsync();

            return newRoom.id;
        }

        [Authorize]
        public async Task RemoveRoom(Guid roomId)
        {
            var token = Helpers.GetTokenFromHeader(_httpContextAccessor);
            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            if (jwtToken == null)
                throw new ArgumentException("AUTH_TOKEN_PROBLEM");

            var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (claim == null)
                throw new ArgumentException("AUTH_TOKEN_CLAIMS_PROBLEM");

            if (roomId == Guid.Empty)
            {
                throw new ArgumentException("EMPTY_ROOM_ID_PROBLEM");
            }


            var room = await _dataBaseConnection.Room.FirstOrDefaultAsync(q => q.id == roomId);
            if (room == null)
                throw new ArgumentException("ROOM_NOT_FOUND_PROBLEM");

            

            if (Guid.Parse(claim.Value) != room.f_owner_id)
                throw new ArgumentException("USER_NOT_OWNER_PROBLEM");

            _dataBaseConnection.Room.Remove(room);

            var roomUsers = _dataBaseConnection.RoomUsers.Where(q => q.f_room_id == roomId);
            if (roomUsers != null || roomUsers.Any())
                _dataBaseConnection.RoomUsers.RemoveRange(roomUsers);
            await _dataBaseConnection.SaveChangesAsync();

        }

        [Authorize]
        public async Task ChangeRoomUsersList(Guid userId, Guid roomId, bool IsNeedAdd, [Service] ITopicEventSender eventSender, CancellationToken cancellationToken)
        {
            var userData = await Helpers.GetUserFromHeader(_dataBaseConnection, _httpContextAccessor);

            var room = await _dataBaseConnection.Room.FirstOrDefaultAsync(q => q.id == roomId);
            if (room == null)
                throw new ArgumentException("ROOM_NOT_EXIST_PROBLEM");

            if (IsNeedAdd)
            {
                if (userData.id != userId)
                    throw new ArgumentException("USER_CANT_BE_ADDED_TO_ROOM_PROBLEM");

                var roomUsers = new RoomUsers()
                {
                    id = Guid.NewGuid(),
                    f_room_id = roomId,
                    f_user_id = userId,
                    b_is_master = false,
                };

                await _dataBaseConnection.RoomUsers.AddAsync(roomUsers);
            }
            else
            {
                if (userData.id != userId || userData.id != room.f_owner_id)
                    throw new ArgumentException("USER_CANT_BE_REMOVED_FROM_ROOM_PROBLEM");

                var roomUsers = _dataBaseConnection.RoomUsers.FirstOrDefault(q => q.id == userId && q.f_room_id == roomId);

                if(roomUsers == null)
                    throw new ArgumentException("ROOM_USER_NOT_EXIST_PROBLEM");

                _dataBaseConnection.RoomUsers.Remove(roomUsers);
            }

            var change = new RoomUserListChange()
            {
                userId = userId,
                roomId = roomId,
                ChangeType = IsNeedAdd ? RoomUserChangeType.Add : RoomUserChangeType.Remove,
            };

            await eventSender.SendAsync($"Chat_{roomId}", change, cancellationToken);
            await _dataBaseConnection.SaveChangesAsync();

        }

        [Authorize]
        public async Task<Message> SendMessageAsync(Message msg, Guid senderId, Guid chatId, [Service] ITopicEventSender eventSender, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(msg.c_content))
            {
                throw new ArgumentException("EMPTY_MESSAGE_CONTENT_PROBLEM");
            }
            if (senderId == Guid.Empty || chatId == Guid.Empty)
            {
                throw new ArgumentException("EMPTY_MESSAGE_SENDER_OR_CHAT_GUID_PROBLEM");
            }
            _dataBaseConnection.Message.Add(msg);
            await _dataBaseConnection.SaveChangesAsync(cancellationToken);

            await eventSender.SendAsync($"Chat_{chatId}", msg, cancellationToken);
            return msg;
        }
    }
}
