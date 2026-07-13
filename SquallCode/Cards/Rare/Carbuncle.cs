using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Squall.SquallCode.Cards.Rare;

public class Carbuncle() : SquallCard(1, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<BufferPower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<BufferPower>(1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<BufferPower>(choiceContext, base.Owner.Creature, DynamicVars["BufferPower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BufferPower"].UpgradeValueBy(1m);
    }
}
