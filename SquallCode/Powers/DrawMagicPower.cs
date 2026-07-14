using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Squall.SquallCode.Mechanics.GF;

namespace Squall.SquallCode.Powers;

/// <summary>
/// At the start of your turn, add the Magic token of a Junctioned GF to
/// your hand (the player chooses when more than one GF is Junctioned).
/// Tokens are upgraded while holding the Summon-upgrading relic.
/// </summary>
public class DrawMagicPower : SquallPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature != base.Owner)
            return;

        bool upgraded = GfRegistry.HasSummonUpgrade(player);

        var entry = await GfRegistry.ChooseJunctionedGf(
            choiceContext,
            player,
            e =>
            {
                var card = e.CreateDrawToken(base.CombatState, player);

                if (upgraded)
                    CardCmd.Upgrade(card);

                return card;
            },
            GfRegistry.DrawPrompt);

        if (entry == null)
            return;

        Flash();

        var token = entry.CreateDrawToken(base.CombatState, player);

        if (upgraded)
            CardCmd.Upgrade(token);

        await CardPileCmd.AddGeneratedCardToCombat(token, PileType.Hand, player);
    }
}
