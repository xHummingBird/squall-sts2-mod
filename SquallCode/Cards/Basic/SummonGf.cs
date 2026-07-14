using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Mechanics.GF;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Basic;

public class SummonGf() : SquallCard(2, CardType.Skill,
    CardRarity.Basic, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(8m, ValueProp.Move),
        new PowerVar<ChannelingGfPower>(2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<ChannelingGfPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(
            base.Owner.Creature,
            DynamicVars.Block.BaseValue,
            ValueProp.Move,
            play);

        bool upgraded = GfRegistry.HasSummonUpgrade(base.Owner);

        var entry = await GfRegistry.ChooseJunctionedGf(
            choiceContext,
            base.Owner,
            e =>
            {
                var card = e.CreateGfCard(base.CombatState, base.Owner);

                if (upgraded)
                    CardCmd.Upgrade(card);

                return card;
            },
            GfRegistry.SummonPrompt);

        if (entry == null)
            return;

        var power = await PowerCmd.Apply<ChannelingGfPower>(
            choiceContext,
            base.Owner.Creature,
            DynamicVars["ChannelingGfPower"].BaseValue,
            base.Owner.Creature,
            this);

        power?.SetChosenGf(entry);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}
