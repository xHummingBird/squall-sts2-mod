using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Rare;

public class RevolverDrive() : SquallCard(2, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<FirepowerPower>();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(18, ValueProp.Move),
        new PowerVar<VulnerablePower>(2m)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        bool hasFirePower = false;
        if (base.Owner.HasPower<FirepowerPower>())
            hasFirePower = true;
        var ownerCreature = Owner?.Creature;
        var squall = Owner?.Character as Character.Squall;

        if (ownerCreature != null && squall != null)
        {
            CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
            AudioHelper.PlayRandomAttack();
            await squall.DashTo(ownerCreature, play.Target, distance: 300f, overrideAnim: "attack_explosion");
            SfxCmd.Play("res://Squall/sounds/attack_special_4.wav");
            SfxCmd.Play("res://Squall/sfx/swing_1.wav");
            SfxCmd.Play("res://Squall/sfx/gunblade_effect.wav");
            SquallExtensions.CombatHelpers.SquallFakeHit(play.Target);
            await Task.Delay(290);
            squall.PlayAnimation(ownerCreature, "upper_blues");
            SfxCmd.Play("res://Squall/sfx/swing_1.wav");
            SfxCmd.Play("res://Squall/sfx/gunblade_effect.wav");
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/hit_1.wav")
                .Execute(choiceContext);
            await Task.Delay(860);
            await squall.Retreat(ownerCreature);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        }
        else
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/hit_1.wav")
                .Execute(choiceContext);

        if (hasFirePower && play.Target != null)
            await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, DynamicVars.Vulnerable.BaseValue,
                base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
        DynamicVars.Vulnerable.UpgradeValueBy(1);
    }
}