using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Squall.SquallCode.Mechanics.GF;

namespace Squall.SquallCode.Cards.Rare;

public class QuickSummon() : SquallCard(2, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        bool upgraded = GfRegistry.HasSummonUpgrade(base.Owner);

        var entry = await GfRegistry.ChooseJunctionedGf(
            choiceContext,
            base.Owner,
            e =>
            {
                var card = e.CreateGfCard(base.CombatState, base.Owner);

                if (upgraded)
                    CardCmd.Upgrade(card);

                return card;
            },
            GfRegistry.SummonPrompt);

        if (entry == null)
            return;

        var gfCard = entry.CreateGfCard(base.CombatState, base.Owner);

        if (upgraded)
            CardCmd.Upgrade(gfCard);

        await CardCmd.AutoPlay(choiceContext, gfCard, null);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
