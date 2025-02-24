using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Weapon.Hit;

[ProtocolId(8070042425022831807)]
public class SelfShaftAimingHitEvent : SelfHitEvent {
    public float HitPower { get; set; }

    [ProtocolIgnore] protected override RemoteShaftAimingHitEvent RemoteEvent => new() {
        HitPower = HitPower,
        Targets = Targets,
        StaticHit = StaticHit,
        ShotId = ShotId,
        ClientTime = ClientTime
    };

    public override void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        base.Execute(connection, entities);

        if (connection.BattlePlayer?.Tank?.WeaponHandler is not ShaftWeaponHandler shaft) return;

        shaft.Reset();
    }
}