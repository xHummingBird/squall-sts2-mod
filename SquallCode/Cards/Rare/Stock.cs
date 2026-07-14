using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Squall.SquallCode.Mechanics.GF;

namespace Squall.SquallCode.Cards.Rare;

public class Stock() : SquallCard(2, CardType.Power,
    CardRarity.Rare, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await GfRegistry.JunctionNewGf(choiceContext, base.Owner, this);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
