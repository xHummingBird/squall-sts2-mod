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

namespace Squall.SquallCode.Cards.Rare;

public class Hypershot() : SquallCard(3, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<FirepowerPower>();
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FirepowerPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        decimal finalDamage = DynamicVars.Damage.BaseValue;
        if (ownerCreature != null && Owner?.Character is Character.Squall squall)
        {
            
            AudioHelper.PlayRandomPhrase();
            float duration = squall.PlayAnimation(ownerCreature, "shoot").total;
            if (duration > 0f)
                await Task.Delay((int)(0.36f * 1000f));
            SfxCmd.Play("res://Squall/sfx/gunblade_explosion.wav");
            SfxCmd.Play("res://Squall/sfx/gunblade_effect.wav");
            squall.PlayVfxOnTarget(
                play.Target,
                "res://Squall/scenes/vfx.tscn",
                "explosion"
            );
        }
        if (base.Owner.HasPower<FirepowerPower>())
            finalDamage = 3*DynamicVars.Damage.BaseValue;
            
        await DamageCmd.Attack(finalDamage).FromCard(this, play).Targeting(play.Target)
            .Execute(choiceContext);
        await Task.Delay((int)(0.36f * 1000f));
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
