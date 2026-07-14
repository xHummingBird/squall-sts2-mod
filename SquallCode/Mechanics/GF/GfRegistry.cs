using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Squall.SquallCode.Cards.Ancient;
using Squall.SquallCode.Cards.Token;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Powers;
using Squall.SquallCode.Relics;

namespace Squall.SquallCode.Mechanics.GF;

public sealed class GfEntry
{
    public required string Name { get; init; }

    //Junction state.
    public required Func<Creature, bool> IsJunctioned { get; init; }
    public required Func<PlayerChoiceContext, Player, CardModel?, Task> ApplyJunctionPower { get; init; }

    //Availability gate (base GFs are always available, Leviathan/Diabolos are relic gated).
    public required Func<Player, bool> IsUnlocked { get; init; }

    //Card factories.
    public required Func<ICombatState, Player, CardModel> CreateGfCard { get; init; }
    public required Func<ICombatState, Player, CardModel> CreateDrawToken { get; init; }
    public required Func<ICombatState, Player, CardModel> CreateCastMagic { get; init; }

    //Hover tips (upgraded variant when true).
    public required Func<bool, IHoverTip> GfCardTip { get; init; }
    public required Func<bool, IHoverTip> DrawTokenTip { get; init; }
    public required Func<bool, IHoverTip> CastMagicTip { get; init; }

    //Icon shown on junction-related cards.
    public required string IconPath { get; init; }
}

public static class GfRegistry
{
    public static LocString JunctionPrompt => new("cards", "SQUALL-GF_SELECT.junctionPrompt");
    public static LocString SummonPrompt => new("cards", "SQUALL-GF_SELECT.summonPrompt");
    public static LocString DrawPrompt => new("cards", "SQUALL-GF_SELECT.drawPrompt");
    public static LocString CastPrompt => new("cards", "SQUALL-GF_SELECT.castPrompt");

    public static readonly IReadOnlyList<GfEntry> All =
    [
        new GfEntry
        {
            Name = "Ifrit",
            IsJunctioned = c => c.HasPower<IfritPower>(),
            ApplyJunctionPower = async (ctx, p, src) =>
                await PowerCmd.Apply<IfritPower>(ctx, p.Creature, 1, p.Creature, src),
            IsUnlocked = _ => true,
            CreateGfCard = (cs, p) => cs.CreateCard<Ifrit>(p),
            CreateDrawToken = (cs, p) => cs.CreateCard<Fire>(p),
            CreateCastMagic = (cs, p) => cs.CreateCard<Firaga>(p),
            GfCardTip = up => HoverTipFactory.FromCard<Ifrit>(up),
            DrawTokenTip = up => HoverTipFactory.FromCard<Fire>(up),
            CastMagicTip = up => HoverTipFactory.FromCard<Firaga>(up),
            IconPath = "ifrit_power.png".BigPowerImagePath()
        },

        new GfEntry
        {
            Name = "Shiva",
            IsJunctioned = c => c.HasPower<ShivaPower>(),
            ApplyJunctionPower = async (ctx, p, src) =>
                await PowerCmd.Apply<ShivaPower>(ctx, p.Creature, 1, p.Creature, src),
            IsUnlocked = _ => true,
            CreateGfCard = (cs, p) => cs.CreateCard<Shiva>(p),
            CreateDrawToken = (cs, p) => cs.CreateCard<Blizzard>(p),
            CreateCastMagic = (cs, p) => cs.CreateCard<Blizzaga>(p),
            GfCardTip = up => HoverTipFactory.FromCard<Shiva>(up),
            DrawTokenTip = up => HoverTipFactory.FromCard<Blizzard>(up),
            CastMagicTip = up => HoverTipFactory.FromCard<Blizzaga>(up),
            IconPath = "shiva_power.png".BigPowerImagePath()
        },

        new GfEntry
        {
            Name = "Quezacoatl",
            IsJunctioned = c => c.HasPower<QuezacoatlPower>(),
            ApplyJunctionPower = async (ctx, p, src) =>
                await PowerCmd.Apply<QuezacoatlPower>(ctx, p.Creature, 1, p.Creature, src),
            IsUnlocked = _ => true,
            CreateGfCard = (cs, p) => cs.CreateCard<Quezacoatl>(p),
            CreateDrawToken = (cs, p) => cs.CreateCard<Thunder>(p),
            CreateCastMagic = (cs, p) => cs.CreateCard<Thundaga>(p),
            GfCardTip = up => HoverTipFactory.FromCard<Quezacoatl>(up),
            DrawTokenTip = up => HoverTipFactory.FromCard<Thunder>(up),
            CastMagicTip = up => HoverTipFactory.FromCard<Thundaga>(up),
            IconPath = "quezacoatl_power.png".BigPowerImagePath()
        },

        new GfEntry
        {
            Name = "Leviathan",
            IsJunctioned = c => c.HasPower<LeviathanPower>(),
            ApplyJunctionPower = async (ctx, p, src) =>
                await PowerCmd.Apply<LeviathanPower>(ctx, p.Creature, 1, p.Creature, src),
            IsUnlocked = p => p.GetRelic<LeviathanScale>() != null,
            CreateGfCard = (cs, p) => cs.CreateCard<Leviathan>(p),
            CreateDrawToken = (cs, p) => cs.CreateCard<Water>(p),
            CreateCastMagic = (cs, p) => cs.CreateCard<Flood>(p),
            GfCardTip = up => HoverTipFactory.FromCard<Leviathan>(up),
            DrawTokenTip = up => HoverTipFactory.FromCard<Water>(up),
            CastMagicTip = up => HoverTipFactory.FromCard<Flood>(up),
            IconPath = "leviathan_power.png".BigPowerImagePath()
        },

        new GfEntry
        {
            Name = "Diabolos",
            IsJunctioned = c => c.HasPower<DiabolosPower>(),
            ApplyJunctionPower = async (ctx, p, src) =>
                await PowerCmd.Apply<DiabolosPower>(ctx, p.Creature, 1, p.Creature, src),
            IsUnlocked = p => p.GetRelic<MagicLamp>() != null,
            CreateGfCard = (cs, p) => cs.CreateCard<Diabolos>(p),
            CreateDrawToken = (cs, p) => cs.CreateCard<Demi>(p),
            CreateCastMagic = (cs, p) => cs.CreateCard<Graviga>(p),
            GfCardTip = up => HoverTipFactory.FromCard<Diabolos>(up),
            DrawTokenTip = up => HoverTipFactory.FromCard<Demi>(up),
            CastMagicTip = up => HoverTipFactory.FromCard<Graviga>(up),
            IconPath = "diabolos_power.png".BigPowerImagePath()
        }
    ];

    public static int IndexOf(GfEntry entry)
    {
        for (int i = 0; i < All.Count; i++)
        {
            if (All[i] == entry)
                return i;
        }

        return -1;
    }

    public static IReadOnlyList<GfEntry> Junctioned(Player player)
    {
        var creature = player?.Creature;

        if (creature == null)
            return [];

        return All.Where(e => e.IsJunctioned(creature)).ToList();
    }

    public static IReadOnlyList<GfEntry> Junctionable(Player player)
    {
        var creature = player?.Creature;

        if (creature == null)
            return [];

        return All
            .Where(e => e.IsUnlocked(player!) && !e.IsJunctioned(creature))
            .ToList();
    }

    //The relic (name TBA) that upgrades GF Summons and Draw Magic tokens.
    public static bool HasSummonUpgrade(Player? player)
    {
        return player?.GetRelic<SolomonRing>() != null;
    }

    /// <summary>
    /// Show a pick-one screen. Uses the cinematic choose-a-card screen when
    /// possible (it only supports up to 3 cards), falls back to a grid.
    /// </summary>
    public static async Task<CardModel?> ChooseCard(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyList<CardModel> cards,
        LocString prompt)
    {
        if (cards.Count == 0)
            return null;

        if (cards.Count == 1)
            return cards[0];

        if (cards.Count <= 3)
        {
            return await CardSelectCmd.FromChooseACardScreen(
                choiceContext, cards, player, canSkip: false);
        }

        var selected = await CardSelectCmd.FromSimpleGrid(
            choiceContext, cards, player, new CardSelectorPrefs(prompt, 1));

        return selected.FirstOrDefault();
    }

    /// <summary>
    /// Let the player junction one of the GFs they don't have yet.
    /// Applies the corresponding GF power (does not play the GF card).
    /// </summary>
    public static async Task JunctionNewGf(
        PlayerChoiceContext choiceContext,
        Player player,
        CardModel? source = null)
    {
        var combatState = player.Creature?.CombatState;

        if (combatState == null)
            return;

        var junctionable = Junctionable(player);

        if (junctionable.Count == 0)
            return;

        bool upgraded = HasSummonUpgrade(player);
        var cards = new List<CardModel>();

        foreach (var entry in junctionable)
        {
            var card = entry.CreateGfCard(combatState, player);

            if (upgraded)
                CardCmd.Upgrade(card);

            cards.Add(card);
        }

        var chosen = await ChooseCard(choiceContext, player, cards, JunctionPrompt);

        if (chosen == null)
            return;

        var chosenEntry = junctionable[cards.IndexOf(chosen)];
        await chosenEntry.ApplyJunctionPower(choiceContext, player, source);
    }

    /// <summary>
    /// Pick one of the currently junctioned GFs, showing the given card per
    /// entry. Returns null if nothing is junctioned; skips the screen when
    /// only one GF is junctioned.
    /// </summary>
    public static async Task<GfEntry?> ChooseJunctionedGf(
        PlayerChoiceContext choiceContext,
        Player player,
        Func<GfEntry, CardModel> cardFactory,
        LocString prompt)
    {
        var junctioned = Junctioned(player);

        if (junctioned.Count == 0)
            return null;

        if (junctioned.Count == 1)
            return junctioned[0];

        var cards = junctioned.Select(cardFactory).ToList();
        var chosen = await ChooseCard(choiceContext, player, cards, prompt);

        if (chosen == null)
            return null;

        return junctioned[cards.IndexOf(chosen)];
    }
}
