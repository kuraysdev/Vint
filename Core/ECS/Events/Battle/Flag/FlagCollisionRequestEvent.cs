using System.Numerics;
using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Flag;

[ProtocolId(1463741053998)]
public class FlagCollisionRequestEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby) return;

        BattlePlayer battlePlayer = connection.BattlePlayer!;
        Battles.Battle battle = battlePlayer.Battle;

        if (!battlePlayer.InBattleAsTank ||
            battlePlayer.Tank!.StateManager.CurrentState is not Active ||
            battle.StateManager.CurrentState is not Running ||
            battle.ModeHandler is not CTFHandler ctf) return;

        entities = entities.ToList();
        IEntity tankEntity = entities.ElementAt(0);
        IEntity flagEntity = entities.ElementAt(1);

        Battles.Flags.Flag collisionFlag = ctf.Flags.Values.Single(flag => flag.Entity == flagEntity);
        Battles.Flags.Flag oppositeFlag = ctf.Flags.Values.Single(flag => flag != collisionFlag);

        TeamColor tankTeamColor = battlePlayer.PlayerConnection.User.GetComponent<TeamColorComponent>().TeamColor;
        TeamColor flagTeamColor = collisionFlag.TeamColor;
        bool isAllyFlag = tankTeamColor == flagTeamColor;

        switch (collisionFlag.StateManager.CurrentState) {
            case OnPedestal: {
                if (Vector3.Distance(battlePlayer.Tank!.Position, collisionFlag.PedestalPosition) > 10) return;

                if (isAllyFlag) {
                    if (oppositeFlag.StateManager.CurrentState is not Captured ||
                        !oppositeFlag.Entity.HasComponent<TankGroupComponent>() ||
                        oppositeFlag.Entity.GetComponent<TankGroupComponent>().Key != tankEntity.Id) return;

                    oppositeFlag.Deliver(battlePlayer);
                } else collisionFlag.Capture(battlePlayer);

                break;
            }

            case OnGround: {
                if (Vector3.Distance(battlePlayer.Tank!.Position, collisionFlag.Position) > 10) return;

                if (isAllyFlag) collisionFlag.Return(battlePlayer);
                else collisionFlag.Pickup(battlePlayer);
                break;
            }
        }
    }
}