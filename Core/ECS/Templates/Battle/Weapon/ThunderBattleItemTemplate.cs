using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon.Splash;
using Vint.Core.ECS.Components.Battle.Weapon.Types;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(-8770103861152493981)]
public class ThunderBattleItemTemplate : DiscreteWeaponTemplate {
    public IEntity Create(IEntity tank, BattlePlayer battlePlayer) {
        const string configPath = "garage/weapon/thunder";
        IEntity entity = Create(configPath, tank, battlePlayer);

        entity.AddComponent(new ThunderComponent());
        entity.AddComponent(ConfigManager.GetComponent<SplashImpactComponent>(configPath));
        entity.AddComponent(ConfigManager.GetComponent<SplashWeaponComponent>(configPath.Replace("garage", "battle")));
        return entity;
    }
}