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

public class FatedCircle() : SquallCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy), IFinisherCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(12m, ValueProp.Move),
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
            squall.PlayAnimation(ownerCreature, "fated_circle");
            SfxCmd.Play("res://Squall/sounds/fated_circle.wav");
            SfxCmd.Play("res://Squall/sfx/fated_circle.wav");
            await Task.Delay((int)(0.56f * 1000f));
            SfxCmd.Play("res://Squall/sfx/gunblade_explosion.wav");
            var targets = base.CombatState.HittableEnemies;
            foreach (var target in targets)
            {
                squall.PlayVfxOnTarget(
                    target,
                    "res://Squall/scenes/vfx.tscn",
                    "explosion"
                );
            }

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, play)
                .TargetingAllOpponents(base.CombatState)
                .WithHitFx(null, "res://Squall/sfx/gunblade_effect.wav")
                .Execute(choiceContext);
            await Task.Delay(330);
            await CreatureCmd.Stun(play.Target);
            await squall.Retreat(ownerCreature);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
            

        }
        else
        {
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, play)
                .TargetingAllOpponents(base.CombatState)
                .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/gunblade_effect.wav")
                .Execute(choiceContext);
            await CreatureCmd.Stun(play.Target);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6);
    }
}