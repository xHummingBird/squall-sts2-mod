
using System;
using System.Collections.Generic;
using Squall.SquallCode.Cards.Ancient;
using Squall.SquallCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Squall.SquallCode.Cards.Basic;

namespace Squall.SquallCode.Mechanics.Crisis;

public static class CrisisCardPatch
{
    private sealed class IconConfig
    {
        public string ContainerName { get; }
        public string ScenePath { get; }
        public Vector2 Position { get; }
        public Func<CardModel, IHoverTip> HoverTipFactory { get; }
        public Func<CardModel, bool>? ShouldShow { get; }

        public IconConfig(
            string containerName,
            string scenePath,
            Vector2 position,
            Func<CardModel, IHoverTip> hoverTipFactory,
            Func<CardModel, bool>? shouldShow = null)
        {
            ContainerName = containerName;
            ScenePath = scenePath;
            Position = position;
            HoverTipFactory = hoverTipFactory;
            ShouldShow = shouldShow ?? (_ => true);
        }
    }

    private static readonly Dictionary<string, PackedScene> SceneCache = new();

    private static readonly IconConfig[] Icons =
    {
        new(
            "RoughDivideIconContainer",
            "res://Squall/scenes/CrisisCardDisplay_RoughDivide.tscn",
            new Vector2(225f, 8f),
            model => model.IsUpgraded
                ? HoverTipFactory.FromCard<RoughDivide>(true)
                : HoverTipFactory.FromCard<RoughDivide>()
        ),

        new(
            "FatedCircleIconContainer",
            "res://Squall/scenes/CrisisCardDisplay_FatedCircle.tscn",
            new Vector2(225f, 48f),
            model => model.IsUpgraded
                ? HoverTipFactory.FromCard<FatedCircle>(true)
                : HoverTipFactory.FromCard<FatedCircle>()
        ),

        new(
            "BlastingZoneIconContainer",
            "res://Squall/scenes/CrisisCardDisplay_BlastingZone.tscn",
            new Vector2(225f, 88f),
            model => model.IsUpgraded
                ? HoverTipFactory.FromCard<BlastingZone>(true)
                : HoverTipFactory.FromCard<BlastingZone>()
        ),
        
        
        new(
            "LionheartIconContainer",
            "res://Squall/scenes/CrisisCardDisplay_Lionheart.tscn",
            new Vector2(225f, 128f),
            model => model.IsUpgraded
                ? HoverTipFactory.FromCard<LionheartCard>(true)
                : HoverTipFactory.FromCard<LionheartCard>(),
            model => model.Owner?.GetRelic<Lionheart>() != null
        )
        
    };

    public static void EnsureAndRefresh(NHandCardHolder holder)
    {
        var model = holder.CardNode?.Model;
        var hitbox = holder.Hitbox;

        if (hitbox == null)
            return;

        if (model is not Renzokuken)
        {
            HideAll(hitbox);
            return;
        }

        foreach (var config in Icons)
        {
            EnsureSingleIcon(holder, hitbox, config);
        }
    }

    private static void EnsureSingleIcon(NHandCardHolder holder, Control hitbox, IconConfig config)
    {
        
        var model = holder.CardNode?.Model;
        if (model == null)
            return;
        
        var container = hitbox.GetNodeOrNull<Control>(config.ContainerName);
        
        bool shouldShow = config.ShouldShow?.Invoke(model) ?? true;

        if (!shouldShow)
        {
            if (container != null)
                container.Visible = false;

            return;
        }


        if (container == null)
        {
            var scene = GetScene(config.ScenePath);
            if (scene == null)
                return;

            container = scene.Instantiate<Control>();
            container.Name = config.ContainerName;

            // ✅ This is the ONLY interactive node
            container.MouseFilter = Control.MouseFilterEnum.Pass;

            hitbox.AddChild(container);
            hitbox.MoveChild(container, hitbox.GetChildCount() - 1);

            // Prevent children stealing hover
            SetChildControlsToIgnore(container);

            var capturedContainer = container;
            var capturedHolder = holder;
            var capturedConfig = config;

            container.MouseEntered += () =>
                OnHovered(capturedContainer, capturedHolder, capturedConfig);
            
            container.MouseExited += () =>
                OnUnhovered(capturedHolder);

        }

        container.Visible = true;
        container.Position = config.Position;
    }
    
    private static void OnHovered(Control owner, NHandCardHolder holder, IconConfig config)
    {
        var model = holder.CardNode?.Model;
        if (model == null)
            return;

        var card = holder.CardNode;
        
        var tip = NHoverTipSet.CreateAndShow(card, config.HoverTipFactory(model));

        if (tip != null)
        {
            tip.MouseFilter = Control.MouseFilterEnum.Ignore;

            // ✅ Position it to the LEFT of the card (like hovercard)
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
        foreach (var config in Icons)
        {
            var container = hitbox.GetNodeOrNull<Control>(config.ContainerName);
            if (container != null)
            {
                container.Visible = false;
                NHoverTipSet.Remove(container);
            }
        }
    }

    private static void SetChildControlsToIgnore(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is Control c)
                c.MouseFilter = Control.MouseFilterEnum.Ignore;

            SetChildControlsToIgnore(child);
        }
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
}

#region Hooks

[HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder._Ready))]
public static class CrisisCardPatch_Ready
{
    public static void Postfix(NHandCardHolder __instance)
    {
        Callable.From(() => CrisisCardPatch.EnsureAndRefresh(__instance)).CallDeferred();
    }
}

[HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder.UpdateCard))]
public static class CrisisCardPatch_UpdateCard
{
    public static void Postfix(NHandCardHolder __instance)
    {
        CrisisCardPatch.EnsureAndRefresh(__instance);
    }
}

#endregion
