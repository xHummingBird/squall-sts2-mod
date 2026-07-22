using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using Squall.SquallCode.Mechanics.Firepower;

namespace Squall.SquallCode.Powers;

/// <summary>
/// While Firepower is full (primed and not yet consumed), the owner's
/// Attacks cost 0.
/// </summary>
public class FullBurstPower : SquallPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (card.Type != CardType.Attack)
            return false;

        var player = card.Owner;

        if (player == null || player.Creature != base.Owner)
            return false;

        if (!FirepowerHelper.IsFirepowerCharged(player))
            return false;

        modifiedCost = 0m;
        return true;
    }
}
