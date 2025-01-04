using HotChocolate.Subscriptions;
using Server.Data;

namespace GraphQLServer.Services.ChatService;

public interface IChatService
{
    Guid GlobalChatGuid { get; set; }
    Task<Guid> GetRoomChatId(Guid roomId);
    Task<List<Message>> GetRoomChatMessages(Guid roomId, Guid lastMessage, int amount = 50);

    Task<Message> SendMessageAsync(Message msg, Guid senderId, Guid chatId, [Service] ITopicEventSender eventSender,
        CancellationToken cancellationToken);

    Task<Guid> CreatePrivateChat(Guid firstuserId, Guid seconduserId);

}