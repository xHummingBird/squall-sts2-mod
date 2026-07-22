using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Squall.SquallCode.Powers;

/// <summary>
/// The first time Marked is triggered each turn, reapply Amount Marked to
/// the creature it triggered on. Invoked by MarkedPower when its trigger
/// consumes a stack. Reapplies with no card source so KillSecuredPower
/// never triggers from it.
/// </summary>
public class TargetAcquiredPower : SquallPower
{
    private class Data
    {
        public bool triggeredThisTurn;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MarkedPower>()
    ];

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature == base.Owner)
            GetInternalData<Data>().triggeredThisTurn = false;

        return Task.CompletedTask;
    }

    public async Task TryReapplyMarked(
        PlayerChoiceContext choiceContext,
        Creature markedTarget)
    {
        Data data = GetInternalData<Data>();

        if (data.triggeredThisTurn)
            return;

        if (!markedTarget.IsAlive)
            return;

        data.triggeredThisTurn = true;

        Flash();

        await PowerCmd.Apply<MarkedPower>(
            choiceContext,
            markedTarget,
            base.Amount,
            base.Owner,
            null);
    }
}
