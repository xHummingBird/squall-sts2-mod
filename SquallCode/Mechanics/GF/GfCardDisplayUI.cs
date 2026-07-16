using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using Squall.SquallCode.Cards.Basic;
using Squall.SquallCode.Cards.Uncommon;
using Squall.SquallCode.Cards.Rare;
using Squall.SquallCode.Cards.Ancient;

namespace Squall.SquallCode.Mechanics.GF;

public static class GfCardDisplayUI
{
    private sealed class IconConfig
    {
        public string Name;
        public Func<CardModel, bool> ShouldShow;
        public Func<GfEntry, string> Scene;
    }

    private static readonly Dictionary<string, PackedScene> Cache = new();

    public static void EnsureAndRefresh(NCard cardNode)
    {
        if (cardNode == null)
            return;

        var model = cardNode.Model;
        var body = cardNode.Body;

        if (model == null || body == null)
            return;

        if (model is not (
            SummonGf or
            PerfectJunction or
            DrawMagic or
            CastMagic or
            Stock))
        {
            HideAll(body);
            return;
        }

        int slot = 0;

        foreach (var entry in GfRegistry.All)
        {
            bool shouldShow = ShouldShow(model, entry);

            string nodeName = $"GF_UI_{entry.Name}";

            var node = body.GetNodeOrNull<Control>(nodeName);

            if (!shouldShow)
            {
                if (node != null)
                    node.Visible = false;

                continue;
            }

            if (node == null)
            {
                var scene = GetScene(entry.ScenePath);

                if (scene == null)
                    continue;

                node = scene.Instantiate<Control>();

                node.Name = nodeName;
                node.MouseFilter = Control.MouseFilterEnum.Ignore;

                body.AddChild(node);

                // exact same trick as Crisis
                body.MoveChild(
                    node,
                    body.GetChildCount() - 1);
            }

            node.Visible = true;

            node.Position = new Vector2(
                75f,
                -205f + (slot * 40f));

            slot++;
        }
    }

    private static bool ShouldShow(
        CardModel model,
        GfEntry entry)
    {
        if (!TryGetOwner(model, out var owner))
            return false;

        var creature = owner.Creature;

        if (creature == null)
            return false;

        if (model is Stock)
            return entry.IsUnlocked(owner)
                   && !entry.IsJunctioned(creature);

        return entry.IsJunctioned(creature);
    }
    
    private static bool TryGetOwner(
        CardModel model,
        out Player? owner)
    {
        owner = null;

        if (model == null)
            return false;

        if (!model.IsMutable)
            return false;

        try
        {
            owner = model.Owner;
            return owner != null;
        }
        catch
        {
            return false;
        }
    }

    private static void EnsureSingleIcon(
        Control body,
        CardModel model,
        GfEntry entry,
        bool shouldShow,
        Vector2 position)
    {
        string nodeName = $"GF_UI_{entry.Name}";

        var node = body.GetNodeOrNull<Control>(nodeName);

        if (!shouldShow)
        {
            if (node != null)
                node.Visible = false;

            return;
        }

        if (node == null)
        {
            var scene = GetScene(entry.ScenePath);

            if (scene == null)
                return;

            node = scene.Instantiate<Control>();

            node.Name = nodeName;

            node.MouseFilter =
                Control.MouseFilterEnum.Ignore;

            body.AddChild(node);

            // Same trick as Crisis
            body.MoveChild(
                node,
                body.GetChildCount() - 1);

            node.ZIndex = 0;
        }

        node.Visible = true;
        node.Position = position;
    }

    private static void HideAll(Control body)
    {
        foreach (var entry in GfRegistry.All)
        {
            string nodeName = $"GF_UI_{entry.Name}";

            var node =
                body.GetNodeOrNull<Control>(nodeName);

            if (node != null)
                node.Visible = false;
        }
    }

    private static PackedScene? GetScene(string path)
    {
        if (Cache.TryGetValue(path, out var cached))
            return cached;

        var scene = GD.Load<PackedScene>(path);

        if (scene != null)
            Cache[path] = scene;

        return scene;
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class GfCardDisplayUI_Ready
{
    public static void Postfix(NCard __instance)
    {
        __instance.ModelChanged += _ =>
        {
            Callable.From(() =>
            {
                GfCardDisplayUI.EnsureAndRefresh(__instance);
            }).CallDeferred();
        };

        Callable.From(() =>
        {
            GfCardDisplayUI.EnsureAndRefresh(__instance);
        }).CallDeferred();
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class GfCardDisplayUI_UpdateVisuals
{
    public static void Postfix(NCard __instance)
    {
        GfCardDisplayUI.EnsureAndRefresh(__instance);
    }
}