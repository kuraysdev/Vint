using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.User;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1458555246853)]
public class LoadUsersEvent : IServerEvent {
    public long RequestEntityId { get; private set; }
    public HashSet<long> UsersId { get; private set; } = null!;

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) { // bug: client crashes while scrolling friends list
        List<IPlayerConnection> playerConnections = connection.Server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .ToList();

        foreach (long userId in UsersId) {
            IEntity? user = playerConnections.SingleOrDefault(conn => conn.Player.Id == userId)?.User;

            if (EntityRegistry.TryGetTemp(userId, out IEntity? tempUser)) { // temp user exists..
                if (user != null) { // ..but player is online
                    foreach (IPlayerConnection shared in tempUser.SharedPlayers) {
                        shared.Unshare(tempUser);
                        shared.Share(user);
                    }

                    connection.ShareIfUnshared(user);
                    EntityRegistry.TryRemoveTemp(userId);
                } else { // ..and player is offline
                    connection.ShareIfUnshared(tempUser);
                }
            } else if (user != null) { // player is online
                connection.ShareIfUnshared(user);
            } else { // player is offline
                using DbConnection db = new();

                Player? player = db.Players.SingleOrDefault(player => player.Id == userId);

                if (player == null) continue;

                user = new UserTemplate().CreateFake(connection, player);
                connection.Share(user);
            }
        }

        connection.Send(new UsersLoadedEvent(RequestEntityId));
    }
}