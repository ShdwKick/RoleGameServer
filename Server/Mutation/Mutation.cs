using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Server.Data;
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

                var role = db.Roles.FirstOrDefault(q => q.c_devname == roleName);
                if (role != null)
                    usr.f_role = (Guid)role.id;

                var newToken = new AuthorizationTokens();
                newToken.c_token = new JwtSecurityTokenHandler().WriteToken(ServerSecretData.GenerateNewToken(usr.id.ToString()));
                newToken.c_hashsum = ServerSecretData.ComputeHash(newToken.c_token);
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
                if (recivedTokenHash != authorizationToken.c_hashsum)
                    throw new ArgumentException("CORRUPTED_TOKEN_DETECTED_PROBLEM");


                authorizationToken.c_token = new JwtSecurityTokenHandler().WriteToken(ServerSecretData.GenerateNewToken(user.id.ToString()));
                authorizationToken.c_hashsum = ServerSecretData.ComputeHash(authorizationToken.c_token);
                await db.SaveChangesAsync();
                return authorizationToken.c_token;
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
                var chat = await db.Chats.FirstOrDefaultAsync(q => q.f_firstuser == firstuserId && q.f_seconduser == seconduserId);

                if (chat == null)
                {
                    chat = new Chats
                    {
                        id = Guid.NewGuid(),
                        f_firstuser = firstuserId,
                        f_seconduser = seconduserId
                    };
                    db.Chats.Add(chat);
                    await db.SaveChangesAsync();
                }
                return (Guid)chat.id;
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
