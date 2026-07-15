using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Powers;
using Squall.SquallCode.Relics;

namespace Squall.SquallCode.Cards.Uncommon;

public class Rearm() : SquallCard(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<FirepowerPower>();
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(2),
        new EnergyVar(1)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FirepowerPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var firepowerRelic = Owner.Relics
            .OfType<IFirepowerRelic>()
            .FirstOrDefault();
        int num = Math.Min(base.DynamicVars.Cards.IntValue, CardPile.MaxCardsInHand - PileType.Hand.GetPile(base.Owner).Cards.Count);
        if (num > 0)
        {
            await CardPileCmd.Add(await CardSelectCmd.FromCombatPile(choiceContext, PileType.Discard.GetPile(base.Owner), base.Owner, new CardSelectorPrefs(base.SelectionScreenPrompt, num)), PileType.Hand);
        }

        if (Owner.Creature.HasPower<FirepowerPower>())
        {
            await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
            await firepowerRelic?.ConsumeFirepower(choiceContext);
            await PowerCmd.Remove<FirepowerPower>(Owner.Creature);
        }
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}