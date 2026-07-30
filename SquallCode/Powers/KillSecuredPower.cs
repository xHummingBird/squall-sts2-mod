using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Squall.SquallCode.Powers;

/// <summary>
/// The first time you apply Marked from a card each turn, gain Amount
/// Energy. Requires a card source, so Marked applied by powers
/// (Mark of the Lion, Target Acquired) never triggers this.
/// </summary>
public class KillSecuredPower : SquallPower
{
    private class Data
    {
        public bool triggeredThisTurn;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MarkPower>()
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

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power is not MarkPower)
            return;

        if (amount <= 0)
            return;

        if (applier != base.Owner)
            return;

        //Only Marked applied from a card counts.
        if (cardSource == null)
            return;

        Data data = GetInternalData<Data>();

        if (data.triggeredThisTurn)
            return;

        var player = base.Owner.Player;

        if (player == null)
            return;

        data.triggeredThisTurn = true;

        Flash();

        await PlayerCmd.GainEnergy(base.Amount, player);
    }
}
