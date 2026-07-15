using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;

namespace Squall.SquallCode.Cards.Uncommon;

public class Cactuar() : SquallCard(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<ThornsPower>()
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ThornsPower>(3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Squall squall)
        {
            AudioHelper.PlayRandomDefend();
            float duration = squall.PlayAnimation(ownerCreature, "cactuar").total;
            if (duration > 0f)
                await Task.Delay((int)(0.2f * 1000f));
        }
        await PowerCmd.Apply<ThornsPower>(choiceContext, base.Owner.Creature, DynamicVars["ThornsPower"].BaseValue, base.Owner.Creature, this);
        await Task.Delay((int)(0.5f * 1000f));
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ThornsPower"].UpgradeValueBy(1m);
    }
}
