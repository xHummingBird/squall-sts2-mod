using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Squall.SquallCode.Powers;
using Squall.SquallCode.Relics;

namespace Squall.SquallCode.Cards.Uncommon;

public class IceArmor() : SquallCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<FirepowerPower>();
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<PlatingPower>(3m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PlatingPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromPower<FirepowerPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var firepowerRelic = Owner.Relics
            .OfType<IFirepowerRelic>()
            .FirstOrDefault();
        decimal platingAmount = base.DynamicVars["PlatingPower"].BaseValue;
        if (base.Owner.HasPower<FirepowerPower>())
        {
            platingAmount = base.DynamicVars["PlatingPower"].BaseValue*2;
            await firepowerRelic?.ConsumeFirepower(choiceContext);
            await PowerCmd.Remove<FirepowerPower>(Owner.Creature);
        }
        await PowerCmd.Apply<PlatingPower>(choiceContext, base.Owner.Creature, platingAmount, base.Owner.Creature, this);
    }
    
    protected override void OnUpgrade()
    {
        base.DynamicVars["PlatingPower"].UpgradeValueBy(1m);
    }
    
}