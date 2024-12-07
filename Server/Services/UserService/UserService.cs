﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Server.Data;
using Server.Data.Helpers;

namespace Server.Services.UserService;

public class UserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DataBaseConnection _dataBaseConnection;
    private readonly IMemoryCache _cache;

    public UserService(IHttpContextAccessor httpContextAccessor, DataBaseConnection dataBaseConnection, 
        IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _dataBaseConnection = dataBaseConnection;
        _cache = cache;
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

    public List<Role> GetRoles()
    {
        try
        {
            if(_cache.TryGetValue("roles", out List<Role> roles))
            {
                var rolesList = _dataBaseConnection.Roles.ToList();
                _cache.Set("roles", rolesList, 
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(15)));
                
                return rolesList;
            }
            else
            {
                return roles;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}