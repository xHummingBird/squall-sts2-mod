using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Uncommon;

public class Ricochet() : SquallCard(
    1,
    CardType.Attack,
    CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MarkedPower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await using AttackContext context =
            await AttackCommand.CreateContextAsync(
                base.CombatState,
                choiceContext,
                cardPlay);

        Creature target = cardPlay.Target;
        Creature ownerCreature = base.Owner.Creature;

        bool targetWasMarked = target.HasPower<MarkedPower>();

        if (Owner?.Character is Character.Squall squall)
        {
            AudioHelper.PlayRandomPhrase();

            float duration = squall.PlayAnimation(
                ownerCreature,
                "shoot").total;

            if (duration > 0f)
            {
                await Task.Delay((int)(0.36f * 1000f));
            }

            SfxCmd.Play("res://Squall/sfx/gunblade_explosion.wav");
            SfxCmd.Play("res://Squall/sfx/gunblade_effect.wav");

            squall.PlayVfxOnTarget(
                target,
                "res://Squall/scenes/vfx.tscn",
                "explosion"
            );
        }

        List<DamageResult> results =
        (
            await CreatureCmd.Damage(
                choiceContext,
                target,
                base.DynamicVars.Damage.BaseValue,
                ValueProp.Move,
                this,
                cardPlay)
        ).ToList();

        context.AddHit(results);

        DamageResult? damageResult = results.FirstOrDefault();

        if (damageResult == null)
            return;

        if (!targetWasMarked)
            return;

        List<Creature> otherEnemies = base.CombatState
            .GetTeammatesOf(damageResult.Receiver)
            .Where(e =>
                e != damageResult.Receiver &&
                e.IsHittable)
            .ToList();

        if (otherEnemies.Count == 0)
            return;

        context.AddHit(
            await CreatureCmd.Damage(
                choiceContext,
                otherEnemies,
                damageResult.TotalDamage,
                ValueProp.Unpowered | ValueProp.Move,
                ownerCreature,
                this,
                cardPlay)
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
