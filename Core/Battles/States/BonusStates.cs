using Vint.Core.Battles.Bonus;
using Vint.Core.Battles.Player;
using Vint.Core.StateMachine;

namespace Vint.Core.Battles.States;

public abstract class BonusState(
    BonusStateManager stateManager
) : State {
    public override BonusStateManager StateManager { get; } = stateManager;
    protected BonusBox Bonus => StateManager.Bonus;
    protected Battle Battle => Bonus.Battle;
}

public class None(
    BonusStateManager stateManager
) : BonusState(stateManager);

public class Cooldown(
    BonusStateManager stateManager,
    TimeSpan? duration = null
) : BonusState(stateManager) {
    TimeSpan Duration { get; } = duration ?? TimeSpan.FromMinutes(2);
    DateTimeOffset TimeToSpawn { get; set; }

    public override void Start() {
        base.Start();

        TimeToSpawn = DateTimeOffset.UtcNow + Duration;
    }

    public override void Tick() {
        if (DateTimeOffset.UtcNow >= TimeToSpawn)
            Bonus.Spawn();

        base.Tick();
    }
}

public class Spawned(
    BonusStateManager stateManager
) : BonusState(stateManager) {
    public DateTimeOffset SpawnTime { get; private set; }

    public override void Start() {
        base.Start();

        foreach (BattlePlayer battlePlayer in Battle.Players.Where(player => player.InBattle))
            battlePlayer.PlayerConnection.Share(Bonus.Entity!);

        SpawnTime = DateTimeOffset.UtcNow;
    }
}