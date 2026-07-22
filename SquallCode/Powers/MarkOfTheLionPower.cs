using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Squall.SquallCode.Powers;

/// <summary>
/// At the start of your turn, apply 1 Marked to a random enemy
/// (once per stack, each stack rolls its own target).
/// Applies with no card source so KillSecuredPower never triggers from it.
/// </summary>
public class MarkOfTheLionPower : SquallPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MarkedPower>()
    ];

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature != base.Owner)
            return;

        if (base.CombatState.HittableEnemies.Count == 0)
            return;

        Flash();

        var rng = player.RunState.Rng.CombatTargets;

        for (int i = 0; i < base.Amount; i++)
        {
            var target = rng.NextItem(base.CombatState.HittableEnemies);

            if (target == null)
                break;

            await PowerCmd.Apply<MarkedPower>(
                choiceContext,
                target,
                1m,
                base.Owner,
                null);
        }
    }
}
