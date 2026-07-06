using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Common;

public class BlizzardBarret() : SquallCard(1, CardType.Attack,
    CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<FirepowerPower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(7m, ValueProp.Move),
        new PowerVar<WeakPower>(1)
        
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Squall squall)
        {
            AudioHelper.PlayRandomPhrase();
            float duration = squall.PlayAnimation(ownerCreature, "shoot").total;
            if (duration > 0f)
                await Task.Delay((int)(0.36f * 1000f));
            SfxCmd.Play("res://Squall/sfx/gunblade_explosion.wav");
            squall.PlayVfxOnTarget(
                play.Target,
                "res://Squall/scenes/vfx.tscn",
                "ice_1"
            );
        }

        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Squall/sfx/ice.wav")
            .Execute(choiceContext);
        if (base.Owner.HasPower<FirepowerPower>() && play.Target != null)
            await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, DynamicVars.Weak.BaseValue, base.Owner.Creature, this);
        await Task.Delay((int)(0.36f * 1000f));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}