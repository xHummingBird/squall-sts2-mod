using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Squall.SquallCode.Mechanics.Crisis;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Rare;

public class Aura() : SquallCard(0, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        base.EnergyHoverTip
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new CardsVar(2),
        new PowerVar<FinisherPower>(15m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, base.Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, base.Owner);
        CrisisManager.GainCrisis(base.Owner, DynamicVars["FinisherPower"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
        DynamicVars["FinisherPower"].UpgradeValueBy(5m);
    }
}
