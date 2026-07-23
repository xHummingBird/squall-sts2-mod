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

public class LionheartCard() : SquallCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(25m),
        new ExtraDamageVar(0.50m),
        new DynamicVar("HpPercent", 50),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier(static (card, _) =>
            {
                var owner = card.Owner?.Creature;

                if (owner == null)
                    return 0m;

                return owner.MaxHp - owner.CurrentHp;
            })
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var squall = Owner?.Character as Character.Squall;

        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);

        Func<string, int, Task> fakeHit = async (sfx, delay) =>
        {
            SfxCmd.Play(sfx);
            SquallExtensions.CombatHelpers.SquallFakeHit(play.Target);
            await Task.Delay(delay);
        };
        
        if (ownerCreature != null && squall != null)
        {
            SfxCmd.Play("res://Squall/sounds/owaraseruzo.wav");
            SfxCmd.Play("res://Squall/sfx/lionheart_charge.wav");
            squall.PlayAnimation(ownerCreature, "lionheart");
            await Task.Delay((int)(0.533f * 1000f));
            AudioHelper.PlayRandomAttack();
            await fakeHit("res://Squall/sfx/hit_1.wav", 600);
            AudioHelper.PlayRandomAttack();
            await fakeHit("res://Squall/sfx/hit_2.wav", 267);
            await fakeHit("res://Squall/sfx/hit_3.wav", 267);
            AudioHelper.PlayRandomAttack();
            await fakeHit("res://Squall/sfx/hit_1.wav", 67);
            SfxCmd.Play("res://Squall/sfx/gunblade_effect.wav");
            await Task.Delay((int)(0.133f * 1000f));
            AudioHelper.PlayRandomAttack();
            await fakeHit("res://Squall/sfx/hit_2.wav", 267);
            await fakeHit("res://Squall/sfx/hit_3.wav", 267);
            SfxCmd.Play("res://Squall/sounds/koredetodomeda.wav");
            await fakeHit("res://Squall/sfx/hit_1.wav", 600);
            SfxCmd.Play("res://Squall/sfx/lionheart_pre.wav");
            await Task.Delay((int)(0.733f * 1000f));
            SfxCmd.Play("res://Squall/sounds/lionheart_final.wav");
            await Task.Delay((int)(0.200f * 1000f));
            SfxCmd.Play("res://Squall/sfx/swing_1.wav");
            SfxCmd.Play("res://Squall/sfx/lionheart_hit.wav");
            await DamageCmd.Attack(base.DynamicVars.CalculatedDamage).FromCard(this, play).Targeting(play.Target)
                .WithHitFx(null, "res://Squall/sfx/gunblade_effect.wav") 
                .Execute(choiceContext);
            await Task.Delay((int)(0.733f * 1000f));
            await squall.Retreat(ownerCreature);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        }
        else
        {
            await DamageCmd.Attack(base.DynamicVars.CalculatedDamage).FromCard(this, play).Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/gunblade_effect.wav")
                .Execute(choiceContext);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        }
    }
    protected override void OnUpgrade()
    {
        base.DynamicVars.CalculationBase.UpgradeValueBy(5m);
        base.DynamicVars.ExtraDamage.UpgradeValueBy(0.16m);
        DynamicVars["HpPercent"].UpgradeValueBy(16m);
    }
}