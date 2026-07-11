using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Mechanics.Crisis;

namespace Squall.SquallCode.Cards.Ancient;

public class BlastingZone() : SquallCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy), IFinisherCard
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(30m, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var squall = Owner?.Character as Character.Squall;

        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && squall != null)
        {
            squall.PlayAnimation(ownerCreature, "blasting_zone");
            await Task.Delay((int)(0.13f * 1000f));
            SfxCmd.Play("res://Squall/sounds/attack_special_4.wav");
            SfxCmd.Play("res://Squall/sfx/hit_3.wav");
            SquallExtensions.CombatHelpers.SquallFakeHit(play.Target);
            await Task.Delay((int)(0.4f * 1000f));
            await squall.Retreat(ownerCreature, null, false, 0.73f);
            SfxCmd.Play("res://Squall/sounds/koredetodomeda.wav");
            await Task.Delay((int)(0.3f * 1000f));
            SfxCmd.Play("res://Squall/sfx/blasting_zone_charge.wav");
            await Task.Delay((int)(1.2f * 1000f));
            SfxCmd.Play("res://Squall/sfx/swing_1.wav");
            SfxCmd.Play("res://Squall/sfx/blasting_zone_hit.wav");
            await Task.Delay((int)(1.3f * 1000f));
            SfxCmd.Play("res://Squall/sfx/hit_1.wav");
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, play).TargetingAllOpponents(base.CombatState)
                .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/gunblade_effect.wav")
                .Execute(choiceContext);
            await Task.Delay(430);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
            
        }
        else
        {
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, play).TargetingAllOpponents(base.CombatState)
                .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/gunblade_effect.wav")
                .Execute(choiceContext);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8);
    }
}