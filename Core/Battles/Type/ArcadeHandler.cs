using ConcurrentCollections;
using Vint.Core.Battles.ArcadeMode;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Matchmaking;
using Vint.Core.Server;

namespace Vint.Core.Battles.Type;

public class ArcadeHandler : TypeHandler {
    public ArcadeHandler(Battle battle, ArcadeModeType mode) : base(battle) {
        Maps = ConfigManager.MapInfos.Where(map => map.MatchMaking && map.HasSpawnPoints(BattleMode)).ToList();
        Mode = mode;
        ModeHandler = GetHandlerByType(Battle, Mode);
    }

    public BattleMode BattleMode { get; } = GetRandomMode();
    public ArcadeModeType Mode { get; }
    public ArcadeModeHandler ModeHandler { get; }
    public IReadOnlyList<MapInfo> Maps { get; }
    ConcurrentHashSet<BattlePlayer> WaitingPlayers { get; } = [];

    public override void Setup() => ModeHandler.Setup();

    public override void Tick() {
        foreach (BattlePlayer battlePlayer in WaitingPlayers.Where(player => DateTimeOffset.UtcNow >= player.BattleJoinTime)) {
            battlePlayer.Init();
            WaitingPlayers.TryRemove(battlePlayer);
        }
    }

    public override void PlayerEntered(BattlePlayer battlePlayer) {
        ModeHandler.PlayerEntered(battlePlayer);

        IPlayerConnection connection = battlePlayer.PlayerConnection;
        IEntity user = connection.User;

        user.AddComponent(new MatchMakingUserComponent());

        if (Battle.StateManager.CurrentState is not Running) return;

        connection.Send(new MatchMakingLobbyStartTimeEvent(battlePlayer.BattleJoinTime), user);
        WaitingPlayers.Add(battlePlayer);
    }

    public override void PlayerExited(BattlePlayer battlePlayer) {
        ModeHandler.PlayerExited(battlePlayer);

        WaitingPlayers.TryRemove(battlePlayer);
        battlePlayer.PlayerConnection.User.RemoveComponentIfPresent<MatchMakingUserComponent>();
    }

    static ArcadeModeHandler GetHandlerByType(Battle battle, ArcadeModeType modeType) => modeType switch {
        ArcadeModeType.FullRandom => new FullRandomHandler(battle),
        ArcadeModeType.QuickPlay => new QuickPlayHandler(battle),
        ArcadeModeType.CosmicBattle => new CosmicHandler(battle),
        ArcadeModeType.WithoutDamage => new WithoutDamageHandler(battle),
        _ => null!
    };
}

public enum ArcadeModeType {
    CosmicBattle,
    DeathMatch,
    TeamDeathMatch,
    CaptureTheFlag,
    QuickPlay,
    WithoutDamage,
    FullRandom
}