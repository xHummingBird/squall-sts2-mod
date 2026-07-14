using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Squall.SquallCode.Mechanics.GF;

namespace Squall.SquallCode.Powers;

/// <summary>
/// "Summoning [gold]Junctioned[/gold] GF in {Amount} turns."
/// One power for every GF: the chosen GF is stored as an index into
/// GfRegistry.All (in a DynamicVar, following TheBombPower's pattern).
/// Ticks down at the start of the owner's turn and autoplays the GF card.
/// </summary>
public class ChannelingGfPower : SquallPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("GfIndex", -1)
    ];

    private GfEntry? ChosenGf
    {
        get
        {
            int index = DynamicVars["GfIndex"].IntValue;

            if (index < 0 || index >= GfRegistry.All.Count)
                return null;

            return GfRegistry.All[index];
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            var entry = ChosenGf;

            if (entry == null)
                return [];

            return [entry.GfCardTip(GfRegistry.HasSummonUpgrade(base.Owner.Player))];
        }
    }

    public void SetChosenGf(GfEntry entry)
    {
        AssertMutable();
        DynamicVars["GfIndex"].BaseValue = GfRegistry.IndexOf(entry);
    }

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature != base.Owner)
            return;

        if (base.Amount > 1)
        {
            await PowerCmd.Decrement(this);
            return;
        }

        Flash();

        var entry = ChosenGf ?? GfRegistry.Junctioned(player).FirstOrDefault();

        if (entry != null)
        {
            var gfCard = entry.CreateGfCard(base.CombatState, player);

            if (GfRegistry.HasSummonUpgrade(player))
                CardCmd.Upgrade(gfCard);

            await CardCmd.AutoPlay(choiceContext, gfCard, null);
        }

        await PowerCmd.Remove(this);
    }
}
