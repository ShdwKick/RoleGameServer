using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Server.Data;
using Server.Data.Helpers;

namespace Server.Services;

public class UserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DataBaseConnection _dataBaseConnection;

    public UserService(IHttpContextAccessor httpContextAccessor, DataBaseConnection dataBaseConnection)
    {
        _httpContextAccessor = httpContextAccessor;
        _dataBaseConnection = dataBaseConnection;
    }


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

    public async Task<string> CreateUser(UserForCreate user, Guid roleGuid)
    {
        var usr = await _dataBaseConnection.Users.FirstOrDefaultAsync(q =>
            q.c_email == user.c_email || q.c_nickname == user.c_nickname);
        if (usr != null)
        {
            throw new ArgumentException("EMAIL_OR_NAME_EXIST_PROBLEM");
        }

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
        if (role == null)
            role = _dataBaseConnection.Roles.FirstOrDefault(q => q.c_dev_name == "User");

        if (role != null)
            usr.f_role = (Guid)role.id;

        var newToken = new AuthorizationToken();
        newToken.c_token = new JwtSecurityTokenHandler().WriteToken(Helpers.GenerateNewToken(usr.id.ToString()));
        newToken.c_hash = Helpers.ComputeHash(newToken.c_token);
        usr.f_authorization_token = (Guid)newToken.id;

        await _dataBaseConnection.Authorization.AddAsync(newToken);
        await _dataBaseConnection.Users.AddAsync(usr);

        await _dataBaseConnection.SaveChangesAsync();

        return newToken.c_token;
    }
    
    public async Task<string> LoginUser(string login, string password)
    {
        string passwordHash = Helpers.ComputeHash(password);

        var user = await _dataBaseConnection.Users.FirstOrDefaultAsync(q =>
            q.c_email == login && q.c_password == passwordHash);
        if (user == null)
            throw new ArgumentException("USER_NOT_FOUND_PROBLEM");

        return await GenerateNewTokenForUser(user);
    }
    
    private async Task<string> GenerateNewTokenForUser(UserData user)
    {
        var token = new JwtSecurityTokenHandler().WriteToken(Helpers.GenerateNewToken(user.id.ToString()));

        var authorizationToken =
            await _dataBaseConnection.Authorization.FirstOrDefaultAsync(q => q.id == user.f_authorization_token);
        if (authorizationToken == null)
            throw new ArgumentException("TOKEN_GENERATION_PROBLEM");

        authorizationToken.c_token = token;
        authorizationToken.c_hash = Helpers.ComputeHash(authorizationToken.c_token);
        await _dataBaseConnection.SaveChangesAsync();

        return token;
    }
}