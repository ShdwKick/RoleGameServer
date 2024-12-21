using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Server.Data;

namespace GraphQLServer.Services.ChatService;

public class ChatService : IChatService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _cache;
    private readonly DataBaseConnection _dataBaseConnection;
    
    public Guid GlobalChatGuid { get; set; }

    public ChatService(IHttpContextAccessor httpContextAccessor, IMemoryCache cache,
        IHttpClientFactory httpClientFactory, DataBaseConnection dataBaseConnection)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
        _dataBaseConnection = dataBaseConnection;
    }
    
    public async Task<Guid> GetRoomChatId(Guid roomId)
    {
        if (_cache.TryGetValue($"roomChatId{roomId.ToString()}", out Guid roomChatId))
        {
            return roomChatId;
        }
        else
        {
            var room = await _dataBaseConnection.RoomChat.FirstOrDefaultAsync(q => q.f_room_id == roomId);
            roomChatId = room.id;
            _cache.Set($"roomChatId{roomId.ToString()}", roomChatId,
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));
            return roomChatId;
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