using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using Squall.SquallCode.Cards.Ancient;

namespace Squall.SquallCode.Powers;

public class FinisherPower : SquallPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        var player = Owner.Player;
        var playerState = player.PlayerCombatState;

        if (playerState == null)
            return;
        
        if (playerState.AllCards
            .OfType<Renzokuken>()
            .Any(c => c.Pile?.Type == PileType.Hand))
        {
            return;
        }
        var cards = playerState.AllCards
            .OfType<Renzokuken>()
            .Where(c => c.Pile == null || c.Pile.Type != PileType.Hand);
        await Task.Delay(500);
        await CardPileCmd.Add(cards, PileType.Hand);
    }

    public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner.Creature != base.Owner)
        {
            return false;
        }
        if (card is not Renzokuken)
        {
            return false;
        }
        bool flag;
        switch (card.Pile?.Type)
        {
            case PileType.Hand:
            case PileType.Play:
                flag = true;
                break;
            default:
                flag = false;
                break;
        }
        if (!flag)
        {
            return false;
        }
        modifiedCost = default(decimal);
        return true;
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == base.Owner && cardPlay.Card is Renzokuken)
        {
            bool flag;
            switch (cardPlay.Card.Pile?.Type)
            {
                case PileType.Hand:
                case PileType.Play:
                    flag = true;
                    break;
                default:
                    flag = false;
                    break;
            }
            if (flag)
            {
                await PowerCmd.Remove(this);
            }
        }
    }
}