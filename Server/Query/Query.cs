using HotChocolate.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Server.Data;
using System.IdentityModel.Tokens.Jwt;
using Server.Data.Helpers;
using System.Net.Http;
using System.Text;

namespace GraphQLServer
{

    public class Query
    {

        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;
        private readonly DataBaseConnection _db;
        public Guid GlobalChatGuid { get; set; }

        public Query(IHttpContextAccessor httpContextAccessor, IMemoryCache cache)
        {
            _httpClient = new HttpClient();
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _db = new DataBaseConnection();
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
                var token = Helpers.GetTokenFromHeader(_httpContextAccessor);
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
                    FRole = await db.Roles.FirstOrDefaultAsync(q => q.id == userData.f_role),
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
                authorizationToken.c_hash = ServerSecretData.ComputeHash(authorizationToken.c_token);
                await db.SaveChangesAsync();

                return token;
            }
        }

        [Authorize]
        public List<Role> GetRoles()
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
        public async Task<Guid> GetRoomChatId(Guid roomId)
        {
            using (DataBaseConnection db = new DataBaseConnection())
            {
                var chat = await db.RoomChat.FirstOrDefaultAsync(q => q.f_room_id == roomId);
                return chat.id;
            }
        }

        [Authorize]
        public async Task<List<Message>> GetRoomChatMessages(Guid roomId, Guid lastMessage, int amount = 50)
        {
            using (DataBaseConnection db = new DataBaseConnection())
            {
                var room = await db.Room.FirstOrDefaultAsync(q => q.id == roomId);
                if (room == null)
                    throw new ArgumentException("ROOM_NOT_FOUND_PROBLEM");
                var chat = await db.RoomChat.FirstOrDefaultAsync(q => q.f_room_id == room.id);
                var lastMsg = await db.Message.FirstOrDefaultAsync(q => q.id == lastMessage);
                if (lastMsg == null)
                {
                    return db.Message.Where(q => q.f_chat == chat.id).TakeLast(amount).ToList();
                }
                else
                {
                    var messagesBeforeLast = await db.Message.Where(q => q.f_chat == chat.id).ToListAsync();
                    var index = messagesBeforeLast.IndexOf(lastMsg);

                    if (index == -1)
                        throw new ArgumentException("LAST_MESSAGE_NOT_FOUND_PROBLEM");

                    return messagesBeforeLast.SkipLast(index).TakeLast(amount).ToList();
                }
            }
        }


        public async Task<bool> SendRecoveryEmail(string address)
        {
            try
            {
                using (DataBaseConnection db = new DataBaseConnection())
                {
                    var user = await db.Users.FirstOrDefaultAsync(q => q.c_email == address);

                    if (user == null)
                        throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

                    Random random = new Random();
                    int code = random.Next(100000, 1000000);


                    await db.RecoveryCodes.AddAsync(new RecoveryCodes()
                    {
                        c_email = address,
                        n_code = code,
                        id = Guid.NewGuid(),
                        d_expiration_time = DateTime.UtcNow.AddMinutes(5),
                    });

                    string url = $"{ServerSecretData.GetBaseUrl()}:7111/EmailSender/SendRecoveryMail?address={Uri.EscapeDataString(address)}&code={Uri.EscapeDataString(code.ToString())}";
                    HttpContent content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await _httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Email sent successfully.");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to send email. Status code: {response.StatusCode}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred while sending email: {ex.Message}");
                return false;
            }
        }
    }
}