using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Cards.Basic;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Mechanics.Crisis;

namespace Squall.SquallCode.Powers;

public class FirepowerPower : SquallPower
{
    private class Data
    {
        public AttackCommand? commandToModify;

        public int amountWhenAttackStarted;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task BeforeAttack(AttackCommand command)
    {
        if (command.Attacker != base.Owner)
        {
            return Task.CompletedTask;
        }
        if (!command.DamageProps.IsPoweredAttack())
        {
            return Task.CompletedTask;
        }
        
        Data internalData = GetInternalData<Data>();
        if (internalData.commandToModify != null)
        {
            return Task.CompletedTask;
        }
        if (command.ModelSource != null && !(command.ModelSource is CardModel))
        {
            return Task.CompletedTask;
        }
        if (!command.DamageProps.IsPoweredAttack())
        {
            return Task.CompletedTask;
        }
        internalData.commandToModify = command;
        internalData.amountWhenAttackStarted = base.Amount;
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (base.Owner != dealer)
        {
            return 0m;
        }
        if (!props.IsPoweredAttack())
        {
            return 0m;
        }
        Data internalData = GetInternalData<Data>();
        if (internalData.commandToModify != null && cardSource != null && cardSource != internalData.commandToModify.ModelSource)
        {
            return 0m;
        }
        if (internalData.commandToModify != null && internalData.commandToModify.Attacker != dealer)
        {
            return 0m;
        }
        return base.Amount;
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        Data internalData = GetInternalData<Data>();
        
        CrisisManager.GainCrisis(base.Owner.Player, 5);
        
        if (command == internalData.commandToModify)
        {
            await PowerCmd.ModifyAmount(choiceContext, this, -internalData.amountWhenAttackStarted, null, null);
        }
        
        await Owner.Player.Creature.CheckCrisisReady(
            choiceContext,
            Owner.Player.Creature,
            null
        );
    }
}