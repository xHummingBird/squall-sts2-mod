
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Mechanics.Crisis;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Relics;

public class Revolver() : SquallRelic
{
    private bool _isActivating;
    private int _attacksPlayedThisCombat;
    private bool _firepowerPrimed;

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override bool ShowCounter => CombatManager.Instance.IsInProgress;

    public override int DisplayAmount
    {
        get
        {
            int cardsRequired = base.DynamicVars.Cards.IntValue;

            if (IsActivating || FirepowerPrimed)
            {
                return cardsRequired;
            }

            return AttacksPlayedThisCombat % cardsRequired;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3),
        new DynamicVar("AmountVar", 2)
    ];

    private bool IsActivating
    {
        get => _isActivating;
        set
        {
            AssertMutable();
            _isActivating = value;
            UpdateDisplay();
        }
    }

    private int AttacksPlayedThisCombat
    {
        get => _attacksPlayedThisCombat;
        set
        {
            AssertMutable();
            _attacksPlayedThisCombat = value;
            UpdateDisplay();
        }
    }

    private bool FirepowerPrimed
    {
        get => _firepowerPrimed;
        set
        {
            AssertMutable();
            _firepowerPrimed = value;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        int cardsRequired = base.DynamicVars.Cards.IntValue;

        if (IsActivating || FirepowerPrimed)
        {
            base.Status = RelicStatus.Active;
        }
        else
        {
            int progress = AttacksPlayedThisCombat % cardsRequired;

            base.Status = progress == cardsRequired - 1
                ? RelicStatus.Active
                : RelicStatus.Normal;
        }

        InvokeDisplayAmountChanged();
    }

    public override Task BeforeCombatStart()
    {
        IsActivating = false;
        FirepowerPrimed = false;
        AttacksPlayedThisCombat = 0;
        base.Status = RelicStatus.Normal;

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Owner != Owner) return;
        var player = Owner;
        bool isCrisis  = card is ICrisisCard;
        
        if (card is not ICrisisCard)
            CrisisManager.GainCrisis(player, 5);
        
        await Owner.Creature.CheckCrisisReady(
            choiceContext,
            Owner.Creature,
            cardPlay.Card
        );
        
        if (!IsValidAttack(cardPlay))
        {
            return;
        }
        
        if (FirepowerPrimed)
        {
            FirepowerPrimed = false;
            UpdateDisplay();
            return;
        }

        AttacksPlayedThisCombat++;

        int cardsRequired = base.DynamicVars.Cards.IntValue;

        if (AttacksPlayedThisCombat % cardsRequired != 0)
        {
            return;
        }

        FirepowerPrimed = true;

        TaskHelper.RunSafely(DoActivateVisuals());

        await ApplyFirepower(choiceContext);
    }

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        /*
         * Intentionally do nothing.
         * Revolver counts attacks across the whole combat,
         * and only resets at the end of combat.
         */
        return Task.CompletedTask;
    }

    private bool IsValidAttack(CardPlay cardPlay)
    {
        return cardPlay.Card.Owner == base.Owner &&
               CombatManager.Instance.IsInProgress &&
               cardPlay.Card.Type == CardType.Attack;
    }

    private async Task ApplyFirepower(PlayerChoiceContext choiceContext)
    {
        int amount = base.DynamicVars["AmountVar"].IntValue;

        await PowerCmd.Apply<FirepowerPower>(
            choiceContext,
            base.Owner.Creature,
            amount,
            null,
            null);
    }

    private async Task DoActivateVisuals()
    {
        IsActivating = true;
        Flash();

        await Cmd.Wait(1f);

        IsActivating = false;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        base.Status = RelicStatus.Normal;
        IsActivating = false;
        FirepowerPrimed = false;
        AttacksPlayedThisCombat = 0;

        return Task.CompletedTask;
    }
    
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner.Creature)
            return;

        if (dealer == base.Owner.Creature)
            return;
    
        if (!props.IsPoweredAttack())
            return;
    
        int gain = 0;
        
        if (result.UnblockedDamage > 0)
        {
            gain += result.UnblockedDamage;
        }
        CrisisManager.GainCrisis(Owner, gain);
        await Owner.Creature.CheckCrisisReady(
            choiceContext,
            Owner.Creature,
            null
        );
    }
}
