using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using Server.Data;

namespace GraphQLServer
{
    public class Subsription
    {
        private readonly DataBaseConnection _dataBaseConnection;

        public Subsription(DataBaseConnection dataBaseConnection)
        {
            _dataBaseConnection = dataBaseConnection;
        }

        [Subscribe(With = nameof(SubscribeToMessagesByChatId))]
        [Topic("Chat_{chatId}")]
        public Task<Message> OnMessageReceived([EventMessage] Message Message)
        {
            return Task.FromResult(Message);
        }
        
        public ValueTask<ISourceStream<Message>> SubscribeToMessagesByChatId(Guid chatId, [Service] ITopicEventReceiver eventReceiver)
        {
            return eventReceiver.SubscribeAsync<Message>($"Chat_{chatId}");
        }

        [Subscribe(With = nameof(SubscribeToRoomUsersListChanged))]
        [Topic("Room_{chatId}")]
        public Task<Message> OnRoomUserListChangeReceived([EventMessage] Message Message)
        {
            return Task.FromResult(Message);
        }

        public ValueTask<ISourceStream<RoomUserListChange>> SubscribeToRoomUsersListChanged(Guid roomId, [Service] ITopicEventReceiver eventReceiver)
        {
            return eventReceiver.SubscribeAsync<RoomUserListChange>($"Room_{roomId}");
        }
    }


}
