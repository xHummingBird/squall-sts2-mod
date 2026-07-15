using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Uncommon;

public class Scattershot() : SquallCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<FirepowerPower>();
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13m, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Squall squall)
        {
            AudioHelper.PlayRandomPhrase();
            var targets = base.CombatState.HittableEnemies;
            float duration = squall.PlayAnimation(ownerCreature, "shoot").total;
            if (duration > 0f)
                await Task.Delay((int)(0.36f * 1000f));
            SfxCmd.Play("res://Squall/sfx/gunblade_explosion.wav");
            SfxCmd.Play("res://Squall/sfx/gunblade_effect.wav");
            if (base.Owner.HasPower<FirepowerPower>() && play.Target != null)
                foreach (var target in targets)
                {
                    squall.PlayVfxOnTarget(
                        target,
                        "res://Squall/scenes/vfx.tscn",
                        "explosion"
                    );
                }
            else
                squall.PlayVfxOnTarget(
                    play.Target,
                    "res://Squall/scenes/vfx.tscn",
                    "explosion"
                );
        }
        if (base.Owner.HasPower<FirepowerPower>() && play.Target != null)
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, play)
                .TargetingAllOpponents(base.CombatState)
                .Execute(choiceContext);
        else await CommonActions.CardAttack(this, play.Target)
            .Execute(choiceContext);
        await Task.Delay((int)(0.36f * 1000f));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
