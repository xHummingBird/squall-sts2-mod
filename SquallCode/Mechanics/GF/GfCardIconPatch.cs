using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Squall.SquallCode.Cards.Ancient;
using Squall.SquallCode.Cards.Basic;
using Squall.SquallCode.Cards.Rare;
using Squall.SquallCode.Cards.Uncommon;

namespace Squall.SquallCode.Mechanics.GF;

/// <summary>
/// Shows the relevant GF icons on Junction-related cards in hand, in the
/// same style as CrisisCardPatch shows finisher icons on Renzokuken.
/// - Summon GF / Perfect Junction: junctioned GFs, hover tip = the GF card.
/// - Draw: junctioned GFs, hover tip = the token the GF generates.
/// - Cast: junctioned GFs, hover tip = the Magic the GF casts.
/// - Stock: junctionable GFs (not yet junctioned), hover tip = the GF card.
/// </summary>
public static class GfCardIconPatch
{
    private const float IconSize = 48f;
    private static readonly Vector2 FirstIconPosition = new(225f, 8f);
    private const float IconSpacing = 40f;

    private static readonly Dictionary<string, Texture2D?> TextureCache = new();

    public static void EnsureAndRefresh(NHandCardHolder holder)
    {
        var model = holder.CardNode?.Model;
        var hitbox = holder.Hitbox;

        if (hitbox == null)
            return;

        if (model is not (SummonGf or PerfectJunction or QuickSummon or DrawMagic or CastMagic or Stock))
        {
            HideAll(hitbox);
            return;
        }

        int slot = 0;

        foreach (var entry in GfRegistry.All)
        {
            bool shouldShow = ShouldShow(model, entry);

            var container = hitbox.GetNodeOrNull<Control>(ContainerName(entry));

            if (!shouldShow)
            {
                if (container != null)
                    container.Visible = false;

                continue;
            }

            container ??= CreateIcon(holder, hitbox, entry);

            if (container == null)
                continue;

            container.Visible = true;
            container.Position = FirstIconPosition + new Vector2(0f, slot * IconSpacing);
            slot++;
        }
    }

    private static string ContainerName(GfEntry entry)
    {
        return $"GfIconContainer_{entry.Name}";
    }

    private static bool ShouldShow(CardModel model, GfEntry entry)
    {
        var owner = model.Owner;
        var creature = owner?.Creature;

        if (owner == null || creature == null)
            return false;

        //Stock shows what can still be junctioned; the rest show what is.
        if (model is Stock)
            return entry.IsUnlocked(owner) && !entry.IsJunctioned(creature);

        return entry.IsJunctioned(creature);
    }

    private static IHoverTip GetHoverTip(CardModel model, GfEntry entry)
    {
        bool summonUpgrade = GfRegistry.HasSummonUpgrade(model.Owner);

        return model switch
        {
            DrawMagic => entry.DrawTokenTip(summonUpgrade),
            CastMagic => entry.CastMagicTip(model.IsUpgraded),
            _ => entry.GfCardTip(summonUpgrade)
        };
    }

    private static readonly Dictionary<string, PackedScene> SceneCache = new();

    private static Control? CreateIcon(
        NHandCardHolder holder,
        Control hitbox,
        GfEntry entry)
    {
        var scene = GetScene(entry.ScenePath);

        if (scene == null)
            return null;

        var container = scene.Instantiate<Control>();

        container.Name = ContainerName(entry);

        // Root receives hover
        container.MouseFilter = Control.MouseFilterEnum.Pass;

        hitbox.AddChild(container);
        hitbox.MoveChild(container, hitbox.GetChildCount() - 1);

        // Children don't steal mouse events
        SetChildControlsToIgnore(container);

        var capturedEntry = entry;
        var capturedHolder = holder;

        container.MouseEntered += () =>
            OnHovered(capturedHolder, capturedEntry);

        container.MouseExited += () =>
            OnUnhovered(capturedHolder);

        return container;
    }
    
    private static PackedScene? GetScene(string path)
    {
        if (SceneCache.TryGetValue(path, out var cached))
            return cached;

        var scene = GD.Load<PackedScene>(path);

        if (scene != null)
            SceneCache[path] = scene;

        return scene;
    }

    private static void SetChildControlsToIgnore(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is Control control)
                control.MouseFilter = Control.MouseFilterEnum.Ignore;

            SetChildControlsToIgnore(child);
        }
    }

    private static void OnHovered(NHandCardHolder holder, GfEntry entry)
    {
        var card = holder.CardNode;
        var model = card?.Model;

        if (card == null || model == null)
            return;

        var tip = NHoverTipSet.CreateAndShow(card, GetHoverTip(model, entry));

        if (tip != null)
        {
            tip.MouseFilter = Control.MouseFilterEnum.Ignore;
            tip.GlobalPosition = card.GlobalPosition + new Vector2(-400f, -200f);
        }
    }

    private static void OnUnhovered(NHandCardHolder holder)
    {
        var card = holder.CardNode;

        if (card != null)
            NHoverTipSet.Remove(card);
    }

    private static void HideAll(Control hitbox)
    {
        foreach (var entry in GfRegistry.All)
        {
            var container = hitbox.GetNodeOrNull<Control>(ContainerName(entry));

            if (container != null)
            {
                container.Visible = false;
                NHoverTipSet.Remove(container);
            }
        }
    }

    private static Texture2D? GetTexture(string path)
    {
        if (TextureCache.TryGetValue(path, out var cached))
            return cached;

        var texture = ResourceLoader.Exists(path)
            ? GD.Load<Texture2D>(path)
            : null;

        TextureCache[path] = texture;

        return texture;
    }
}

#region Hooks

[HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder._Ready))]
public static class GfCardIconPatch_Ready
{
    public static void Postfix(NHandCardHolder __instance)
    {
        Callable.From(() => GfCardIconPatch.EnsureAndRefresh(__instance)).CallDeferred();
    }
}

[HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder.UpdateCard))]
public static class GfCardIconPatch_UpdateCard
{
    public static void Postfix(NHandCardHolder __instance)
    {
        GfCardIconPatch.EnsureAndRefresh(__instance);
    }
}

#endregion
