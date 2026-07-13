using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;

namespace Squall.SquallCode.Cards.Uncommon;

public class TriggerHappy() : SquallCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.RandomEnemy)
{
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(3m, ValueProp.Move),
        new RepeatVar(5),
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
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this, play)
                .TargetingRandomOpponents(base.CombatState)
                .WithHitCount(base.DynamicVars.Repeat.IntValue)
                .WithHitFx(null, "res://Squall/sfx/gunblade_effect.wav")
                .WithHitVfxNode(target =>
                {
                    squall.PlayVfxOnTarget(
                        target,
                        "res://Squall/scenes/vfx.tscn",
                        "explosion"
                    );

                    return null;
                })
                .Execute(choiceContext);
            await Task.Delay((int)(0.25f * 1000f));
        }
        else await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingRandomOpponents(base.CombatState)
            .WithHitCount(base.DynamicVars.Repeat.IntValue)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
    }
}