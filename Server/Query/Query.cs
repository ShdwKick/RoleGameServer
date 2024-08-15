using HotChocolate.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Server.Data;
using System.IdentityModel.Tokens.Jwt;

namespace GraphQLServer
{

    public class Query
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;
        private readonly DataBaseConnection _db;
        public Guid GlobalChatGuid { get; set; }

        public Query(IHttpContextAccessor httpContextAccessor, IMemoryCache cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _db = new DataBaseConnection();
        }

        private string GetTokenFromHeader()
        {
            if (_httpContextAccessor == null)
                throw new ArgumentException("ERROR_OCCURRED");

            var httpContext = _httpContextAccessor.HttpContext;
            string authorizationHeader = httpContext.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                throw new ArgumentException("INVALID_AUTHORIZATION_HEADER_PROBLEM");
            }

            return authorizationHeader.Substring("Bearer ".Length).Trim();
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
            using (DataBaseConnection db = new DataBaseConnection())
            {
                var token = GetTokenFromHeader();
                if (token == null || token == "")
                    throw new ArgumentException("INVALID_AUTHORIZATION_HEADER_PROBLEM");

                var dbToken = await db.AuthorizationTokens.FirstOrDefaultAsync(q => q.c_token == token);

                if (dbToken == null)
                    throw new ArgumentException("TOKEN_NOT_FOUND_PROBLEM");

                UserData userData = await db.Users.FirstOrDefaultAsync(q => q.f_authorizationtoken == dbToken.id);
                if (userData == null)
                    throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

                User user = new User
                {
                    id = userData.id,
                    c_nickname = userData.c_nickname,
                    c_email = userData.c_email,
                    b_emailconfirmed = userData.b_emailconfirmed,
                    d_registrationdate = userData.d_registrationdate,
                    f_role = await db.Roles.FirstOrDefaultAsync(q => q.id == userData.f_role),
                };
                return user;
            }
        }

        public async Task<string> LoginUser(string login, string password)
        {
            string passwordHash = ServerSecretData.ComputeHash(password);
            using (DataBaseConnection db = new DataBaseConnection())
            {
                var user = await db.Users.FirstOrDefaultAsync(q => q.c_email == login && q.c_password == passwordHash);
                if (user == null)
                    throw new ArgumentException("USER_NOT_FOUND_PROBLEM");
                var token = new JwtSecurityTokenHandler().WriteToken(
                    ServerSecretData.GenerateNewToken(user.id.ToString()));

                var authorizationToken =
                    await db.AuthorizationTokens.FirstOrDefaultAsync(q => q.id == user.f_authorizationtoken);
                if (authorizationToken == null) throw new ArgumentException("TOKEN_GENERATION_PROBLEM");

                authorizationToken.c_token = token;
                authorizationToken.c_hashsum = ServerSecretData.ComputeHash(authorizationToken.c_token);
                await db.SaveChangesAsync();

                return token;
            }
        }

        [Authorize]
        public List<Roles> GetRoles()
        {
            using (DataBaseConnection db = new DataBaseConnection())
            {
                try
                {
                    return db.Roles.ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
        
        [Authorize]
        public async Task<Guid> GetGlobalChatId()
        {
            if (GlobalChatGuid != Guid.Empty)
                return GlobalChatGuid;
            using (DataBaseConnection db = new DataBaseConnection())
            {
                var chat = await db.Chats.FirstOrDefaultAsync(q => q.b_isglobalchat == true);
                GlobalChatGuid = (Guid)chat.id;
                return GlobalChatGuid;
            }
        }
    }
}