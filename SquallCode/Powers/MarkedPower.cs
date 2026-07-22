using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Relics;

namespace Squall.SquallCode.Powers;

public class MarkedPower : SquallPower
{
    private class Data
    {
        public AttackCommand? commandToTrack;
        public bool tookUnblockedDamage;
    }

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task BeforeAttack(AttackCommand command)
    {
        if (!command.DamageProps.IsPoweredAttack())
            return Task.CompletedTask;

        Data data = GetInternalData<Data>();

        // Already tracking an attack sequence
        if (data.commandToTrack != null)
            return Task.CompletedTask;

        data.commandToTrack = command;
        data.tookUnblockedDamage = false;

        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != base.Owner)
            return 1m;

        if (!props.IsPoweredAttack())
            return 1m;

        bool hasHyperion = base.Owner.CombatState?.Players
            .Any(p => p.GetRelic<Hyperion>() != null) == true;

        return hasHyperion
            ? 1.75m
            : 1.50m;
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != base.Owner)
            return Task.CompletedTask;

        if (!props.IsPoweredAttack())
            return Task.CompletedTask;

        if (result.UnblockedDamage <= 0)
            return Task.CompletedTask;

        if (dealer == null)
            return Task.CompletedTask;

        Data data = GetInternalData<Data>();

        // Mark that this Armor Break was actually used during the attack sequence.
        data.tookUnblockedDamage = true;

        return Task.CompletedTask;
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        Data data = GetInternalData<Data>();

        if (command != data.commandToTrack)
            return;

        bool shouldRemove = data.tookUnblockedDamage;

        data.commandToTrack = null;
        data.tookUnblockedDamage = false;

        if (shouldRemove)
        {
            await PowerCmd.Decrement(this);

            //Target Acquired: the first time Marked is triggered each turn,
            //the attacker reapplies Marked to this creature.
            var targetAcquired = command.Attacker?.GetPower<TargetAcquiredPower>();

            if (targetAcquired != null)
                await targetAcquired.TryReapplyMarked(choiceContext, base.Owner);
        }
    }
}