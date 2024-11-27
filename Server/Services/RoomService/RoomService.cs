using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Server.Data;

namespace Server.Services.RoomService;

public class RoomService : IRoomService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _httpClient;
    private readonly DataBaseConnection _dataBaseConnection;
    private readonly IMemoryCache _cache;
    public RoomService(IHttpContextAccessor httpContextAccessor,DataBaseConnection dataBaseConnection, HttpClient httpClient, IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _dataBaseConnection = dataBaseConnection;
        _httpClient = httpClient;
        _cache = cache;
    }
    
    public async Task<Guid> GetRoomChatId(Guid roomId)
    {
        if (_cache.TryGetValue($"RoomChatId{roomId}", out Guid guid))
        {
            var chat = await _dataBaseConnection.RoomChat.FirstOrDefaultAsync(q => q.f_room_id == roomId);
            _cache.Set($"RoomChatId{roomId}", chat.id, 
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            
            return chat.id;
        }
        else
        {
            return guid;
        }
    }

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
}