using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Cards.Ancient;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Mechanics.Crisis;
using Squall.SquallCode.Mechanics.GF;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Relics;

public abstract class FirepowerRelicBase : SquallRelic, IFirepowerRelic
{
    private bool _isActivating;
    private int _firepowerProgress;
    private bool _firepowerPrimed;

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override bool ShowCounter => CombatManager.Instance.IsInProgress;

    protected virtual int CardsRequired => 3;

    protected abstract int FirepowerAmount { get; }

    public override int DisplayAmount
    {
        get
        {
            int cardsRequired = base.DynamicVars.Cards.IntValue;

            if (IsActivating || FirepowerPrimed)
            {
                return cardsRequired;
            }

            return FirepowerProgress;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(CardsRequired),
        new DynamicVar("AmountVar", FirepowerAmount)
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

    private int FirepowerProgress
    {
        get => _firepowerProgress;
        set
        {
            AssertMutable();
            _firepowerProgress = value;
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

    public void GainFirepowerProgress(int amount = 1)
    {
        if (FirepowerPrimed)
            return;

        int cardsRequired = base.DynamicVars.Cards.IntValue;

        FirepowerProgress += amount;

        if (FirepowerProgress >= cardsRequired)
        {
            FirepowerProgress = cardsRequired;
            FirepowerPrimed = true;
        }
    }

    public async Task ConsumeFirepower(PlayerChoiceContext? choiceContext)
    {
        FirepowerPrimed = false;
        FirepowerProgress = 0;

        Creature owner = base.Owner.Creature;

        decimal shivaAmount = owner.GetPowerAmount<ShivaPower>();
        decimal leviathanAmount = owner.GetPowerAmount<LeviathanPower>();
        decimal quezacoatlAmount = owner.GetPowerAmount<QuezacoatlPower>();
        decimal diabolosAmount = owner.GetPowerAmount<DiabolosPower>();

        if (shivaAmount > 0)
        {
            await CreatureCmd.GainBlock(
                owner,
                3m * shivaAmount,
                ValueProp.Unpowered,
                null
            );
        }

        if (leviathanAmount > 0)
        {
            await CreatureCmd.Heal(
                owner,
                2m * leviathanAmount,
                true
            );
        }

        if (quezacoatlAmount > 0)
        {
            await CardPileCmd.Draw(
                choiceContext,
                (int)quezacoatlAmount,
                base.Owner
            );
        }

        // Base Firepower consume gives 5 Crisis.
        // Each Diabolos stack adds another 5 Crisis.
        CrisisManager.GainCrisis(
            Owner,
            5 + ((int)diabolosAmount * 3)
        );

        if (owner.HasPower<GunbladeMasteryPower>())
        {
            decimal gunbladeMasteryAmount = owner.GetPowerAmount<GunbladeMasteryPower>();

            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                owner,
                gunbladeMasteryAmount,
                owner,
                null
            );
        }

        if (owner.HasPower<FirepowerPower>())
        {
            await PowerCmd.Remove<FirepowerPower>(owner);
        }
    }

    public int GetFirepowerProgressForUI()
    {
        return FirepowerProgress;
    }

    public bool IsFirepowerChargedForUI()
    {
        return FirepowerPrimed;
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
            int progress = FirepowerProgress;

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
        FirepowerProgress = 0;
        base.Status = RelicStatus.Normal;

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        CardModel card = cardPlay.Card;

        if (card.Owner != Owner)
            return;

        if (card is not IFinisherCard && card is not IGFCard && card.Type == CardType.Attack)
        {
            CrisisManager.GainCrisis(Owner, 5);
        }

        if (IsValidAttack(cardPlay))
        {
            if (FirepowerPrimed)
            {
                await ConsumeFirepower(choiceContext);
            }
            else
            {
                GainFirepowerProgress();

                if (FirepowerPrimed)
                {
                    TaskHelper.RunSafely(DoActivateVisuals());
                    await ApplyFirepower(choiceContext);
                }
            }
        }

        await Owner.Creature.CheckCrisisReady(
            choiceContext,
            Owner.Creature,
            card
        );
    }

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        /*
         * Intentionally do nothing.
         * Firepower counts attacks across the whole combat,
         * and only resets at the end of combat.
         */
        return Task.CompletedTask;
    }

    private bool IsValidAttack(CardPlay cardPlay)
    {
        // Finishers neither consume nor build Firepower.
        return cardPlay.Card.Owner == base.Owner &&
               CombatManager.Instance.IsInProgress &&
               cardPlay.Card.Type == CardType.Attack &&
               cardPlay.Card is not IFinisherCard &&
               cardPlay.Card is not IGFCard;
    }

    private async Task ApplyFirepower(PlayerChoiceContext choiceContext)
    {
        int amount = base.DynamicVars["AmountVar"].IntValue;
        int ifritAmount = Owner.Creature.GetPowerAmount<IfritPower>();

        if (ifritAmount > 0)
            amount += 2 * ifritAmount;

        await PowerCmd.Apply<FirepowerPower>(
            choiceContext,
            base.Owner.Creature,
            amount,
            null,
            null
        );
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
        FirepowerProgress = 0;

        return Task.CompletedTask;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
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
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != base.Owner.Creature.Side)
            return;

        if (combatState.RoundNumber <= 1)
        {
            int crisisGainFromHpLoss = base.Owner.Creature.MaxHp - base.Owner.Creature.CurrentHp;
            CrisisManager.GainCrisis(Owner, crisisGainFromHpLoss);
        }

        if (base.Owner.Creature.Player.GetRelic<MagicLamp>() != null)
            CrisisManager.GainCrisis(Owner, 10);
        else CrisisManager.GainCrisis(Owner, 5);

        await Owner.Creature.CheckCrisisReady(
            null,
            Owner.Creature,
            null
        );
    }

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (player == base.Owner.Creature.Player && combatState.RoundNumber <= 1)
        {
            await GfRegistry.JunctionNewGf(choiceContext, base.Owner);

            //Junction Ring grants one additional Junction.
            if (base.Owner.GetRelic<JunctionRing>() != null)
                await GfRegistry.JunctionNewGf(choiceContext, base.Owner);
            
            if (base.Owner.GetRelic<LeviathanScale>() != null)
            {
                FirepowerProgress = base.DynamicVars.Cards.IntValue;
                FirepowerPrimed = true;
                if (!base.Owner.Creature.HasPower<FirepowerPower>())
                {
                    await ApplyFirepower(choiceContext);
                }
                TaskHelper.RunSafely(DoActivateVisuals());
            }
        }
    }
}