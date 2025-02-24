using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon.Types.Hammer;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(4939169559170921259)]
public class HammerBattleItemTemplate : DiscreteWeaponTemplate {
    public IEntity Create(IEntity tank, BattlePlayer battlePlayer) {
        const string configPath = "garage/weapon/hammer";
        IEntity entity = Create(configPath, tank, battlePlayer);

        entity.AddComponent(new HammerComponent());
        entity.AddComponent(new MagazineReadyStateComponent());
        entity.AddComponent(ConfigManager.GetComponent<MagazineWeaponComponent>(configPath));
        entity.AddComponent(ConfigManager.GetComponent<HammerPelletConeComponent>(configPath.Replace("garage", "battle")));
        return entity;
    }
}