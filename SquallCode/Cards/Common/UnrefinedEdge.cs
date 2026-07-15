using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Common;

public class UnrefinedEdge() : SquallCard(0, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Squall squall)
        {
            AudioHelper.PlayRandomAttack();
            float duration = squall.PlayAnimation(ownerCreature, "attack").total;
            if (duration > 0f)
                await Task.Delay((int)(0.13f * 1000f));
            SfxCmd.Play("res://Squall/sfx/swing_1.wav");
            await Task.Delay((int)(0.03f * 1000f));
            if (base.Owner.HasPower<FirepowerPower>())
            {
                SfxCmd.Play("res://Squall/sfx/gunblade_effect.wav");
                squall.PlayVfxOnTarget(
                    play.Target,
                    "res://Squall/scenes/vfx.tscn",
                    "explosion"
                );
            }
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/hit_1.wav")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
