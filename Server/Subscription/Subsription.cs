using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using Server.Data;

namespace GraphQLServer
{
    public class Subsription
    {
        [Subscribe(With = nameof(SubscribeToMessagesByChatId))]
        [Topic("Chat_{chatId}")]
        public Task<Message> OnMessageReceived([EventMessage] Message Message)
        {
            using (DataBaseConnection db = new DataBaseConnection())
            {
                return Task.FromResult(Message);
            }
        }
        
        public ValueTask<ISourceStream<Message>> SubscribeToMessagesByChatId(Guid chatId, [Service] ITopicEventReceiver eventReceiver)
        {
            return eventReceiver.SubscribeAsync<Message>($"Chat_{chatId}");
        }
    }


}
