
using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using Squall.SquallCode.Cards;
using Squall.SquallCode.Cards.Basic;
using Squall.SquallCode.Relics;

namespace Squall.SquallCode.Mechanics.Crisis;

public static class CrisisCardUI
{
    private sealed class IconConfig
    {
        public string Name;
        public string Scene;
        public Vector2 Position;
        public Func<CardModel, bool> ShouldShow;
    }

    private static readonly Dictionary<string, PackedScene> Cache = new();

    private static readonly IconConfig[] Icons =
    {
        new()
        {
            Name = "RoughDivide_UI",
            Scene = "res://Squall/scenes/CrisisCardDisplay_RoughDivide.tscn",
            Position = new Vector2(75f, -205f),
            ShouldShow = _ => true
        },

        new()
        {
            Name = "FatedCircle_UI",
            Scene = "res://Squall/scenes/CrisisCardDisplay_FatedCircle.tscn",
            Position = new Vector2(75f, -165f),
            ShouldShow = _ => true
        },

        new()
        {
            Name = "BlastingZone_UI",
            Scene = "res://Squall/scenes/CrisisCardDisplay_BlastingZone.tscn",
            Position = new Vector2(75f, -125f),
            ShouldShow = _ => true
        },

        new()
        {
            Name = "Lionheart_UI",
            Scene = "res://Squall/scenes/CrisisCardDisplay_Lionheart.tscn",
            Position = new Vector2(75f, -85f),

            ShouldShow = model =>
            {
                if (!TryGetOwner(model, out var owner))
                    return false;

                return owner.GetRelic<Lionheart>() != null;
            }
        }
    };

    public static void EnsureAndRefresh(NCard cardNode)
    {
        if (cardNode == null)
            return;

        var model = cardNode.Model;
        var body = cardNode.Body;

        if (model == null || body == null)
            return;

        if (model is not Renzokuken)
        {
            HideAll(body);
            return;
        }

        foreach (var icon in Icons)
        {
            EnsureSingleIcon(body, model, icon);
        }
    }

    private static void EnsureSingleIcon(
        Control body,
        CardModel model,
        IconConfig config)
    {
        var node = body.GetNodeOrNull<Control>(config.Name);

        bool shouldShow;

        try
        {
            shouldShow = config.ShouldShow(model);
        }
        catch
        {
            shouldShow = false;
        }

        if (!shouldShow)
        {
            if (node != null)
                node.Visible = false;

            return;
        }

        if (node == null)
        {
            var scene = GetScene(config.Scene);

            if (scene == null)
                return;

            node = scene.Instantiate<Control>();
            node.Name = config.Name;

            node.MouseFilter = Control.MouseFilterEnum.Ignore;

            body.AddChild(node);
            body.MoveChild(node, body.GetChildCount() - 1);

            node.ZIndex = 0;
        }

        node.Visible = true;
        node.Position = config.Position;
    }

    private static void HideAll(Control body)
    {
        foreach (var icon in Icons)
        {
            var node = body.GetNodeOrNull<Control>(icon.Name);

            if (node != null)
                node.Visible = false;
        }
    }

    private static PackedScene? GetScene(string path)
    {
        if (Cache.TryGetValue(path, out var cached))
            return cached;

        var loaded = GD.Load<PackedScene>(path);

        if (loaded != null)
            Cache[path] = loaded;

        return loaded;
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
}

[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class CrisisDisplayUI_Ready
{
    public static void Postfix(NCard __instance)
    {
        __instance.ModelChanged += _ =>
        {
            Callable.From(() =>
            {
                CrisisCardUI.EnsureAndRefresh(__instance);
            }).CallDeferred();
        };

        Callable.From(() =>
        {
            CrisisCardUI.EnsureAndRefresh(__instance);
        }).CallDeferred();
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class CrisisDisplayUI_UpdateVisuals
{
    public static void Postfix(NCard __instance)
    {
        CrisisCardUI.EnsureAndRefresh(__instance);
    }
}
