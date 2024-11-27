using Server.Data;

namespace Server.Services.RoomService;

public interface IRoomService
{
    public Task<Guid> GetRoomChatId(Guid roomId);
    public Task<List<Message>> GetRoomChatMessages(Guid roomId, Guid lastMessage, int amount = 50);
}