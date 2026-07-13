using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Squall.SquallCode.Powers;
using Squall.SquallCode.Relics;

namespace Squall.SquallCode.Cards.Rare;

public class Berserk() : SquallCard(2, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var firepowerRelic = Owner.Relics
            .OfType<IFirepowerRelic>()
            .FirstOrDefault();
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, DynamicVars.Strength.BaseValue, base.Owner.Creature, this);
        if (base.Owner.HasPower<FirepowerPower>())
        {
            await firepowerRelic?.ConsumeFirepower(choiceContext);
            await PowerCmd.Remove<FirepowerPower>(base.Owner.Creature);
            await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1m);
    }
}
