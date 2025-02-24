using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Components.Battle.Weapon.Stream;

[ProtocolId(6803807621463709653)]
public class WeaponStreamShootingComponent : IComponent {
    public DateTimeOffset? StartShootingTime { get; private set; }
    public int Time { get; private set; }

    public void Added(IPlayerConnection connection, IEntity entity) {
        if (connection.BattlePlayer?.Tank?.WeaponHandler is VulcanWeaponHandler vulcan)
            vulcan.ShootingStartTime ??= DateTimeOffset.UtcNow;
    }
}