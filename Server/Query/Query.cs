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
        private readonly DataBaseConnection _dataBaseConnection;
        public Guid GlobalChatGuid { get; set; }

        public Query(IHttpContextAccessor httpContextAccessor, IMemoryCache cache)
        {
            _httpClient = new HttpClient();
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _dataBaseConnection = new DataBaseConnection();
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
            var userData = await Helpers.GetUserFromHeader(_dataBaseConnection, _httpContextAccessor);

            User user = new User
            {
                id = userData.id,
                c_nickname = userData.c_nickname,
                c_email = userData.c_email,
                b_is_mail_confirmed = userData.b_is_mail_confirmed,
                d_registration_date = userData.d_registration_date,
                f_role = await _dataBaseConnection.Roles.FirstOrDefaultAsync(q => q.id == userData.f_role),
            };
            return user;
        }

        [Authorize]
        public async Task<User> GetUserById(Guid userId)
        {
            var userData = await _dataBaseConnection.Users.FirstOrDefaultAsync(q => q.id == userId);
            if (userData == null)
                throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

            User user = new User
            {
                id = userData.id,
                c_nickname = userData.c_nickname,
                c_email = userData.c_email,
                b_is_mail_confirmed = userData.b_is_mail_confirmed,
                d_registration_date = userData.d_registration_date,
                f_role = await _dataBaseConnection.Roles.FirstOrDefaultAsync(q => q.id == userData.f_role),
            };
            return user;
        }

        public async Task<string> LoginUser(string login, string password)
        {
            string passwordHash = Helpers.ComputeHash(password);

            var user = await _dataBaseConnection.Users.FirstOrDefaultAsync(q => q.c_email == login && q.c_password == passwordHash);
            if (user == null)
                throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

            var token = new JwtSecurityTokenHandler().WriteToken(Helpers.GenerateNewToken(user.id.ToString()));

            var authorizationToken = await _dataBaseConnection.Authorization.FirstOrDefaultAsync(q => q.id == user.f_authorization_token);
            if (authorizationToken == null) 
                throw new ArgumentException("TOKEN_GENERATION_PROBLEM");

            authorizationToken.c_token = token;
            authorizationToken.c_hash = Helpers.ComputeHash(authorizationToken.c_token);
            await _dataBaseConnection.SaveChangesAsync();

            return token;
        }

        [Authorize]
        public List<Role> GetRoles()
        {
            try
            {
                return _dataBaseConnection.Roles.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        [Authorize]
        public async Task<Guid> GetRoomChatId(Guid roomId)
        {

            var chat = await _dataBaseConnection.RoomChat.FirstOrDefaultAsync(q => q.f_room_id == roomId);
            return chat.id;

        }

        [Authorize]
        public async Task<List<Message>> GetRoomChatMessages(Guid roomId, Guid lastMessage, int amount = 50)
        {

            var room = await _dataBaseConnection.Room.FirstOrDefaultAsync(q => q.id == roomId);
            if (room == null)
                throw new ArgumentException("ROOM_NOT_FOUND_PROBLEM");
            var chat = await _dataBaseConnection.RoomChat.FirstOrDefaultAsync(q => q.f_room_id == room.id);
            var lastMsg = await _dataBaseConnection.Message.FirstOrDefaultAsync(q => q.id == lastMessage);
            if (lastMsg == null)
            {
                return _dataBaseConnection.Message.Where(q => q.f_chat == chat.id).TakeLast(amount).ToList();
            }
            else
            {
                var messagesBeforeLast = await _dataBaseConnection.Message.Where(q => q.f_chat == chat.id).ToListAsync();
                var index = messagesBeforeLast.IndexOf(lastMsg);

                if (index == -1)
                    throw new ArgumentException("LAST_MESSAGE_NOT_FOUND_PROBLEM");

                return messagesBeforeLast.SkipLast(index).TakeLast(amount).ToList();
            }

        }

        public async Task<bool> SendRecoveryEmail(string address)
        {
            try
            {
                var user = await _dataBaseConnection.Users.FirstOrDefaultAsync(q => q.c_email == address);

                if (user == null)
                    throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

                Random random = new Random();
                int code = random.Next(100000, 1000000);


                await _dataBaseConnection.RecoveryCodes.AddAsync(new RecoveryCodes()
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
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred while sending email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendEmailConfirmationEMail()
        {
            try
            {
                var user = await Helpers.GetUserFromHeader(_dataBaseConnection, _httpContextAccessor);

                if (user == null)
                    throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

                Random random = new Random();
                int code = random.Next(100000, 1000000);


                await _dataBaseConnection.RecoveryCodes.AddAsync(new RecoveryCodes()
                {
                    c_email = user.c_email,
                    n_code = code,
                    id = Guid.NewGuid(),
                    d_expiration_time = DateTime.UtcNow.AddMinutes(5),
                });

                string url = $"{ServerSecretData.GetBaseUrl()}:7111/EmailSender/SendConfirmationMail?address={Uri.EscapeDataString(user.c_email)}";
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
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred while sending email: {ex.Message}");
                return false;
            }
        }
    }
}