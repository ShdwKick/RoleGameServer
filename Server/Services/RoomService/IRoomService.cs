using HotChocolate.Subscriptions;
using Server.Data;

namespace GraphQLServer.Services.RoomService;

public interface IRoomService
{
    Task RemoveRoom(Guid roomId);

    Task ChangeRoomUsersList(Guid userId, Guid roomId, bool IsNeedAdd,
        [Service] ITopicEventSender eventSender, CancellationToken cancellationToken);

    Task<Guid> CreateRoom(CreateRoom room);
}