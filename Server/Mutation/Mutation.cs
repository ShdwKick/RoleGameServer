using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Helpers;
using System.IdentityModel.Tokens.Jwt;

namespace GraphQLServer
{
    public class Mutation
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Mutation(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> CreateUser(UserForCreate user, string roleName)
        {
            using (DataBaseConnection db = new DataBaseConnection())
            {
                var usr = await db.Users.FirstOrDefaultAsync(q => q.c_email == user.c_email || q.c_nickname == user.c_nickname);
                if (usr != null)
                {
                    throw new ArgumentException("EMAIL_OR_NAME_EXIST_PROBLEM");
                };

                usr = new UserData
                {
                    id = Guid.NewGuid(),
                    c_nickname = user.c_nickname,
                    c_email = user.c_email,
                    c_password = ServerSecretData.ComputeHash(user.c_password),
                    d_registrationdate = DateOnly.FromDateTime(DateTime.Today),
                };

                var role = db.Roles.FirstOrDefault(q => q.c_dev_name == roleName);
                if (role != null)
                    usr.f_role = (Guid)role.id;

                var newToken = new AuthorizationToken();
                newToken.c_token = new JwtSecurityTokenHandler().WriteToken(ServerSecretData.GenerateNewToken(usr.id.ToString()));
                newToken.c_hash = ServerSecretData.ComputeHash(newToken.c_token);
                usr.f_authorizationtoken = (Guid)newToken.id;

                await db.AuthorizationTokens.AddAsync(newToken);
                await db.Users.AddAsync(usr);

                await db.SaveChangesAsync();
                return newToken.c_token;
            }
        }

        public async Task<string> TryRefreshToken(string oldToken)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadToken(oldToken) as JwtSecurityToken;
            if (jwtToken == null) throw new ArgumentException("TOKEN_GENERATION_PROBLEM");

            var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (claim == null) throw new ArgumentException("TOKEN_GENERATION_PROBLEM");

            using (var db = new DataBaseConnection())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.id.ToString() == claim.Value);
                if (user == null) throw new ArgumentException("TOKEN_GENERATION_PROBLEM");

                var authorizationToken = await db.AuthorizationTokens.FirstOrDefaultAsync(q => q.id == user.f_authorizationtoken);
                if (authorizationToken == null) throw new ArgumentException("TOKEN_GENERATION_PROBLEM");

                var recivedTokenHash = ServerSecretData.ComputeHash(oldToken);
                if (recivedTokenHash != authorizationToken.c_hash)
                    throw new ArgumentException("CORRUPTED_TOKEN_DETECTED_PROBLEM");


                authorizationToken.c_token = new JwtSecurityTokenHandler().WriteToken(ServerSecretData.GenerateNewToken(user.id.ToString()));
                authorizationToken.c_hash = ServerSecretData.ComputeHash(authorizationToken.c_token);
                await db.SaveChangesAsync();
                return authorizationToken.c_token;
            }
        }

        public async Task<bool> PasswordRecovery(string email, string code, string newPassword)
        {
            using (var db = new DataBaseConnection())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.c_email == email);
                if (user == null)
                    throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

                var recoveryCode = await db.RecoveryCodes.FirstOrDefaultAsync(u => u.c_email == email);
                if (recoveryCode == null)
                    throw new ArgumentException("RECOVERY_CODE_NOT_FOUND_PROBLEM");

                if(!recoveryCode.n_code.ToString().Equals(code))
                    throw new ArgumentException("RECOVERY_CODE_NOT_CORRECT_PROBLEM");

                if(DateTime.UtcNow > recoveryCode.d_expiration_time)
                    throw new ArgumentException("RECOVERY_CODE_WAS_EXPIRED_PROBLEM");

                user.c_password = ServerSecretData.ComputeHash(newPassword);
                await db.SaveChangesAsync();
                return true;
            }
        }

        [Authorize]
        public async Task<bool> RefreshPassword(string oldPassword, string newPassword)
        {
            var token = Helpers.GetTokenFromHeader(_httpContextAccessor);
            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            if (jwtToken == null) 
                throw new ArgumentException("AUTH_TOKEN_PROBLEM");

            var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (claim == null) 
                throw new ArgumentException("AUTH_TOKEN_CLAIMS_PROBLEM");

            using (var db = new DataBaseConnection())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.id.ToString() == claim.Value);
                if (user == null)
                    throw new ArgumentException("USER_NOT_FOUND_PROBLEM");
                
                var oldPasswordHash = ServerSecretData.ComputeHash(oldPassword);
                if (oldPasswordHash != user.c_password)
                    throw new ArgumentException("OLD_PASSWORD_NOT_CORRECT_PROBLEM");


                user.c_password = ServerSecretData.ComputeHash(newPassword);
                await db.SaveChangesAsync();
                return true;
            }
        }

        [Authorize]
        public async Task<Guid> CreatePrivateChat(Guid firstuserId, Guid seconduserId)
        {
            if (firstuserId == Guid.Empty || seconduserId == Guid.Empty)
            {
                throw new ArgumentException("EMPTY_MESSAGE_SENDER_OR_CHAT_GUID_PROBLEM");
            }
            using (var db = new DataBaseConnection())
            {
                var chat = await db.PrivateChat.FirstOrDefaultAsync(q => q.f_firstuser == firstuserId && q.f_seconduser == seconduserId);

                if (chat == null)
                {
                    chat = new PrivateChat
                    {
                        id = Guid.NewGuid(),
                        f_firstuser = firstuserId,
                        f_seconduser = seconduserId
                    };
                    db.PrivateChat.Add(chat);
                    await db.SaveChangesAsync();
                }
                return (Guid)chat.id;
            };
        }
        [Authorize]
        public async Task<Guid> CreateRoom(CreateRoom room)
        {
            if (room.f_owner_id == Guid.Empty)
            {
                throw new ArgumentException("EMPTY_OWNER_ID_PROBLEM");
            }
            using (var db = new DataBaseConnection())
            {
                var user = await db.Users.FirstOrDefaultAsync(q => q.id == room.f_owner_id);
                if(user == null)
                    throw new ArgumentException("EMPTY_USER_NOT_EXIST");

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
                await db.Room.AddAsync(newRoom);
                await db.RoomUsers.AddAsync(roomUsers);
                await db.SaveChangesAsync();

                return newRoom.id;
            };
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

            using (var db = new DataBaseConnection())
            {
                db.Message.Add(msg);
                await db.SaveChangesAsync(cancellationToken);
            };
            await eventSender.SendAsync($"Chat_{chatId}", msg, cancellationToken);
            return msg;
        }
    }
}
