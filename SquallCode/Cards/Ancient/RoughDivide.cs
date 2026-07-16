using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Mechanics.Crisis;

namespace Squall.SquallCode.Cards.Ancient;

public class RoughDivide() : SquallCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy), IFinisherCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(27m, ValueProp.Move),
        new PowerVar<VulnerablePower>(2m)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var squall = Owner?.Character as Character.Squall;

        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && squall != null)
        {
            squall.PlayAnimation(ownerCreature, "rough_divide");
            SfxCmd.Play("res://Squall/sounds/kimeru.wav");
            
            await Task.Delay((int)(0.1f * 1000f));
            SfxCmd.Play("res://Squall/sfx/hit_1.wav");
            SquallExtensions.CombatHelpers.SquallFakeHit(play.Target);
            
            await Task.Delay((int)(0.567f * 1000f));
            SfxCmd.Play("res://Squall/sfx/swing_1.wav");
            SfxCmd.Play("res://Squall/sfx/hit_1.wav");
            CommonActions.CardAttack(this, play.Target)
                .WithHitFx(null, "res://Squall/sfx/gunblade_effect.wav")
                .Execute(choiceContext);
            await Task.Delay((int)(0.434f * 1000f));
            squall.Retreat(ownerCreature, null, false, 0.01f);
            await Task.Delay((int)(0.567f * 1000f));
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
        }
        else
        {
            
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/gunblade_effect.wav")
                .Execute(choiceContext);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8m);
        DynamicVars.Vulnerable.UpgradeValueBy(1m);
    }
}