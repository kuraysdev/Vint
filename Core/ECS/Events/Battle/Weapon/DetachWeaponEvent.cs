using System.Numerics;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Weapon;

[ProtocolId(1447917521601)]
public class DetachWeaponEvent : IServerEvent {
    public Vector3 AngularVelocity { get; private set; }
    public Vector3 Velocity { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IEntity tank = entities.Single();

        if (!connection.InLobby ||
            !connection.BattlePlayer!.InBattleAsTank ||
            connection.BattlePlayer.Tank!.Tank != tank) return;

        foreach (BattlePlayer battlePlayer in connection.BattlePlayer.Battle.Players)
            battlePlayer.PlayerConnection.Send(this, tank);
    }
}