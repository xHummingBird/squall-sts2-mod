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

        if (base.Owner.Creature.HasPower<ShivaPower>())
        {
            await CreatureCmd.GainBlock(
                base.Owner.Creature,
                3m,
                ValueProp.Unpowered,
                null
            );
        }
        
        if (base.Owner.Creature.HasPower<LeviathanPower>())
        {
            await CreatureCmd.Heal(
                base.Owner.Creature,
                2m, true
            );
        }

        if (base.Owner.Creature.HasPower<QuezacoatlPower>())
        {
            await CardPileCmd.Draw(choiceContext, 1, base.Owner);
        }

        if (base.Owner.Creature.HasPower<DiabolosPower>())
            CrisisManager.GainCrisis(Owner, 10);
        else CrisisManager.GainCrisis(Owner, 5);
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

        if (card is not IFinisherCard && card is not IGFCard)
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

        if (base.Owner.HasPower<IfritPower>())
            amount++;
        
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
            var choiceContext = new ThrowingPlayerChoiceContext();
            
            CardModel? selectedJunction = null;
            LeviathanScale? hasLeviathan = base.Owner?.GetRelic<LeviathanScale>();
            MagicLamp? hasDiabolos = base.Owner?.GetRelic<MagicLamp>();
        
            var ifrit = combatState.CreateCard<Ifrit>(base.Owner);
            var shiva = combatState.CreateCard<Shiva>(base.Owner);
            var quezacoatl = combatState.CreateCard<Quezacoatl>(base.Owner);
            var leviathan = combatState.CreateCard<Leviathan>(base.Owner);
            var diabolos = combatState.CreateCard<Diabolos>(base.Owner);
            
            List<CardModel> cards = [];

            if (!base.Owner.HasPower<IfritPower>())
                cards.Add(ifrit);

            if (!base.Owner.HasPower<ShivaPower>())
                cards.Add(shiva);

            if (!base.Owner.HasPower<QuezacoatlPower>())
                cards.Add(quezacoatl);

            if (hasLeviathan != null && !base.Owner.HasPower<LeviathanPower>())
                cards.Add(leviathan);

            if (hasDiabolos != null && !base.Owner.HasPower<DiabolosPower>())
                cards.Add(diabolos);
            
            selectedJunction = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards.ToList(), base.Owner, canSkip: false);

            if (selectedJunction == ifrit)
                await PowerCmd.Apply<IfritPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, null);
            if (selectedJunction == shiva)
                await PowerCmd.Apply<ShivaPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, null);
            if (selectedJunction == diabolos)
                await PowerCmd.Apply<DiabolosPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, null);
            if (selectedJunction == quezacoatl)
                await PowerCmd.Apply<QuezacoatlPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, null);
            if (selectedJunction == leviathan)
                await PowerCmd.Apply<LeviathanPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, null);
        }
        
        if (base.Owner.Creature.HasPower<DiabolosPower>())
            CrisisManager.GainCrisis(Owner, 5);
        
        await Owner.Creature.CheckCrisisReady(
            null,
            Owner.Creature,
            null
        );
    }
}