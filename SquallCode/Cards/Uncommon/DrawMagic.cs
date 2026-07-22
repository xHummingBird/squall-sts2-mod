using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Squall.SquallCode.Mechanics.GF;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Uncommon;

public class DrawMagic() : SquallCard(2, CardType.Power,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DrawMagicPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        //Choose the GF when the card is played; the power remembers it.
        bool upgraded = GfRegistry.HasSummonUpgrade(base.Owner);

        var entry = await GfRegistry.ChooseJunctionedGf(
            choiceContext,
            base.Owner,
            e =>
            {
                var card = e.CreateDrawToken(base.CombatState, base.Owner);

                if (upgraded)
                    CardCmd.Upgrade(card);

                return card;
            },
            GfRegistry.DrawPrompt);

        if (entry == null)
            return;

        var power = await PowerCmd.Apply<DrawMagicPower>(
            choiceContext,
            base.Owner.Creature,
            1m,
            base.Owner.Creature,
            this);

        power?.SetChosenGf(entry);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
