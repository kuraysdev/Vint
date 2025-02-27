using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(-4669704207166218448)]
public class ExitBattleEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby) return;

        BattlePlayer battlePlayer = connection.BattlePlayer!;
        Battles.Battle battle = battlePlayer.Battle;

        if (battlePlayer.IsKicked) return;

        if (battlePlayer.IsSpectator || battlePlayer.InBattleAsTank)
            battle.RemovePlayer(battlePlayer);
        else
            battle.RemovePlayerFromLobby(battlePlayer);
    }
}