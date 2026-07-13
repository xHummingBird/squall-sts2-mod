using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Powers;
using Squall.SquallCode.Relics;

namespace Squall.SquallCode.Cards.Uncommon;

public class PulseAmmo() : SquallCard(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FirepowerPower>(),
        HoverTipFactory.FromPower<VigorPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VigorPower>(5m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        AudioHelper.PlayRandomDefend();
        await PowerCmd.Apply<VigorPower>(choiceContext, base.Owner.Creature, DynamicVars["VigorPower"].BaseValue, base.Owner.Creature, this);
        var firepowerRelic = Owner.Relics
            .OfType<IFirepowerRelic>()
            .FirstOrDefault();
        if (Owner.Creature.HasPower<FirepowerPower>())
        {
            await PowerCmd.Apply<VigorPower>(choiceContext, base.Owner.Creature, DynamicVars["VigorPower"].BaseValue, base.Owner.Creature, this);
            await firepowerRelic?.ConsumeFirepower(choiceContext);
            await PowerCmd.Remove<FirepowerPower>(Owner.Creature);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
