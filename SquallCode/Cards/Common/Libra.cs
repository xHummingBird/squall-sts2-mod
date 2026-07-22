using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Uncommon;

public class Libra() : SquallCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MarkedPower>()
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<MarkedPower>(1m),
        new CardsVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        if (cardPlay.Target.HasPower<MarkedPower>())
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, base.Owner);
        }
        await PowerCmd.Apply<MarkedPower>(choiceContext, cardPlay.Target, base.DynamicVars["MarkedPower"].BaseValue, base.Owner.Creature, this);
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}