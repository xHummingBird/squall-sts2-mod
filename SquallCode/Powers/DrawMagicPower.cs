using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Squall.SquallCode.Mechanics.GF;

namespace Squall.SquallCode.Powers;

/// <summary>
/// At the start of your turn, add the Magic token of the GF chosen when
/// Draw was played to your hand. Instanced (like ChannelingGfPower):
/// playing Draw again adds another instance bound to its own GF, so the
/// effect stacks. The chosen GF is stored as an index into GfRegistry.All
/// in the GfIndex DynamicVar.
/// Tokens are upgraded while holding the Summon-upgrading relic.
/// </summary>
public class DrawMagicPower : SquallPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

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

            return [entry.DrawTokenTip(GfRegistry.HasSummonUpgrade(base.Owner.Player))];
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

        var entry = ChosenGf ?? GfRegistry.Junctioned(player).FirstOrDefault();

        if (entry == null)
            return;

        Flash();

        var token = entry.CreateDrawToken(base.CombatState, player);

        if (GfRegistry.HasSummonUpgrade(player))
            CardCmd.Upgrade(token);

        await CardPileCmd.AddGeneratedCardToCombat(token, PileType.Hand, player);
    }
}
