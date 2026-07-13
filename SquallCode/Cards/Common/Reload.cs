using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Powers;
using Squall.SquallCode.Relics;

namespace Squall.SquallCode.Cards.Common;

public class Reload() : SquallCard(1, CardType.Skill,
    CardRarity.Common, TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<FirepowerPower>();
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(6, ValueProp.Move),
        new CardsVar(1)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FirepowerPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var firepowerRelic = Owner.Relics
            .OfType<IFirepowerRelic>()
            .FirstOrDefault();
        AudioHelper.PlayRandomDefend();
        await CommonActions.CardBlock(this, play);

        if (Owner.Creature.HasPower<FirepowerPower>())
        {
            await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.IntValue, base.Owner);
            await firepowerRelic?.ConsumeFirepower(choiceContext);
            await PowerCmd.Remove<FirepowerPower>(Owner.Creature);
        }
    }
    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(3m);
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}