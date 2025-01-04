using System.IdentityModel.Tokens.Jwt;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Server.Data;
using Server.Data.Helpers;

namespace GraphQLServer.Services.RoomService;

public class RoomService : IRoomService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _cache;
    private readonly DataBaseConnection _dataBaseConnection;


    public RoomService(IHttpContextAccessor httpContextAccessor, IMemoryCache cache,
        IHttpClientFactory httpClientFactory, DataBaseConnection dataBaseConnection)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
        _dataBaseConnection = dataBaseConnection;
    }

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

        var roomUsers = _dataBaseConnection.RoomUsers.FirstOrDefault(q => q.f_room_id == roomId);
        if (roomUsers != null)
            _dataBaseConnection.RoomUsers.Remove(roomUsers);
        
        await _dataBaseConnection.SaveChangesAsync();
    }
    
    public async Task ChangeRoomUsersList(Guid userId, Guid roomId, bool IsNeedAdd,
        [Service] ITopicEventSender eventSender, CancellationToken cancellationToken)
    {
        var userData = await Helpers.GetUserFromHeader(_dataBaseConnection, _httpContextAccessor);

        var room = await _dataBaseConnection.Room.FirstOrDefaultAsync(q => q.id == roomId);
        if (room == null)
            throw new ArgumentException("ROOM_NOT_EXIST_PROBLEM");
        
        
        if (IsNeedAdd && userData.id != userId)
            throw new ArgumentException("USER_CANT_BE_ADDED_TO_ROOM_PROBLEM");

        if (!IsNeedAdd && userData.id != userId || userData.id != room.f_owner_id)
            throw new ArgumentException("USER_CANT_BE_REMOVED_FROM_ROOM_PROBLEM");
        
        
        if (IsNeedAdd)
        {
            await AddUserToRoom(userId, roomId);
        }
        else
        {
            RemoveUserFromRoom(userId, roomId);
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

    private async Task AddUserToRoom(Guid userId, Guid roomId)
    {
        var roomUsers = new RoomUsers()
        {
            id = Guid.NewGuid(),
            f_room_id = roomId,
            f_user_id = userId,
            b_is_master = false,
        };

        await _dataBaseConnection.RoomUsers.AddAsync(roomUsers);
    }

    private void RemoveUserFromRoom(Guid userId, Guid roomId)
    {
        var roomUsers =
            _dataBaseConnection.RoomUsers.FirstOrDefault(q => q.id == userId && q.f_room_id == roomId);

        if (roomUsers == null)
            throw new ArgumentException("ROOM_USER_NOT_EXIST_PROBLEM");

        _dataBaseConnection.RoomUsers.Remove(roomUsers);
    }
    
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
}