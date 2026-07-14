using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Squall.SquallCode.Mechanics.GF;

namespace Squall.SquallCode.Cards.Uncommon;

public class CastMagic() : SquallCard(1, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var entry = await GfRegistry.ChooseJunctionedGf(
            choiceContext,
            base.Owner,
            e =>
            {
                var card = e.CreateCastMagic(base.CombatState, base.Owner);

                if (base.IsUpgraded)
                    CardCmd.Upgrade(card);

                return card;
            },
            GfRegistry.CastPrompt);

        if (entry == null)
            return;

        var magic = entry.CreateCastMagic(base.CombatState, base.Owner);

        if (base.IsUpgraded)
            CardCmd.Upgrade(magic);

        await CardCmd.AutoPlay(choiceContext, magic, play.Target);
    }

    protected override void OnUpgrade()
    {
        //Upgrading this card upgrades the Magic it casts instead.
    }
}
