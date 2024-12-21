using Server.Data;

namespace GraphQLServer.Services.ChatService;

public interface IChatService
{
    Guid GlobalChatGuid { get; set; }
    Task<Guid> GetRoomChatId(Guid roomId);
    Task<List<Message>> GetRoomChatMessages(Guid roomId, Guid lastMessage, int amount = 50);
}