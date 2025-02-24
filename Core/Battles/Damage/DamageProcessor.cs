using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Damage;
using Vint.Core.Server;

namespace Vint.Core.Battles.Damage;

public interface IDamageProcessor {
    public void Damage(BattleTank source, BattleTank target, IEntity weapon, CalculatedDamage damage);

    public DamageType Damage(BattleTank target, CalculatedDamage damage);

    public void Heal(BattleTank source, BattleTank target, CalculatedDamage heal);

    public void Heal(BattleTank target, CalculatedDamage heal);
}

public class DamageProcessor : IDamageProcessor {
    public void Damage(BattleTank source, BattleTank target, IEntity weapon, CalculatedDamage damage) {
        if (damage.Value <= 0) return;

        DamageType type = Damage(target, damage);
        IPlayerConnection sourcePlayerConnection = source.BattlePlayer.PlayerConnection;
        source.DealtDamage += damage.Value;

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (type) {
            case DamageType.Kill:
                if (source == target)
                    target.SelfDestruct();
                else
                    target.KillBy(source, weapon);
                break;

            case DamageType.Normal:
                if (!target.KillAssistants.TryAdd(source, damage.Value))
                    target.KillAssistants[source] += damage.Value;
                break;

            case DamageType.Critical:
                sourcePlayerConnection.Send(new CriticalDamageEvent(target.Tank, damage.HitPoint), source.Weapon);

                if (!target.KillAssistants.TryAdd(source, damage.Value))
                    target.KillAssistants[source] += damage.Value;
                break;
        }

        sourcePlayerConnection.Send(new DamageInfoEvent(damage.HitPoint,
                damage.Value,
                damage.IsCritical || damage.IsBackHit || damage.IsTurretHit),
            target.Tank);
    }

    public DamageType Damage(BattleTank target, CalculatedDamage damage) {
        if (damage.Value <= 0) return DamageType.Normal;

        target.SetHealth(target.Health - damage.Value);
        target.TakenDamage += damage.Value;

        return target.Health switch {
            <= 0 => DamageType.Kill,
            _ => damage.IsCritical ? DamageType.Critical : DamageType.Normal
        };
    }

    public void Heal(BattleTank source, BattleTank target, CalculatedDamage damage) {
        if (damage.Value <= 0) return;

        Heal(target, damage);
        source.BattlePlayer.PlayerConnection.Send(new DamageInfoEvent(damage.HitPoint,
                damage.Value,
                damage.IsCritical || damage.IsBackHit || damage.IsTurretHit,
                true),
            target.Tank);
    }

    public void Heal(BattleTank target, CalculatedDamage damage) {
        if (damage.Value <= 0) return;

        target.SetHealth(target.Health + damage.Value);
        target.BattlePlayer.PlayerConnection.Send(new DamageInfoEvent(damage.HitPoint,
                damage.Value,
                damage.IsCritical || damage.IsBackHit || damage.IsTurretHit,
                true),
            target.Tank);
    }
}