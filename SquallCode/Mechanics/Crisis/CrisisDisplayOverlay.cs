
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Squall.SquallCode.Extensions;

namespace Squall.SquallCode.Mechanics.Crisis;

public partial class CrisisDisplayOverlay : Control
{
    public static CrisisDisplayOverlay? Instance { get; private set; }

    private Control? _crisisDisplay;
    private RichTextLabel? _label;
    private Player? _player;
    private IHoverTip? _hoverTip;

    private int _lastValue = -1;
    private Tween? _popTween;
    private bool _exiting;

    private const int CrisisMax = 100;

    private static readonly Color CrisisGainGreen = new Color(0.4f, 1f, 0.4f);
    private static readonly Color CrisisMaxRed = new Color(1f, 0.25f, 0.2f);
    
    private static readonly Color CrisisDefaultBlue =
        new Color(0.55f, 0.75f, 1.0f);


    public override void _Ready()
    {
        Instance = this;
        Name = "CrisisDisplayOverlay";

        MouseFilter = MouseFilterEnum.Pass;

        // Defer setup so NEnergyCounter/combat UI can finish entering tree.
        CallDeferred(nameof(Setup));
    }

    private async void Setup()
    {
        if (!IsInsideTree())
            return;

        // Wait for CombatManager / LocalContext / local player.
        // This avoids the race condition from NEnergyCounter._Ready().
        for (int i = 0; i < 60; i++)
        {
            if (_exiting || !IsInsideTree())
                return;

            var state = CombatManager.Instance?.DebugOnlyGetState();
            var player = state?.Players.FirstOrDefault(p => LocalContext.IsMe(p));

            if (player != null)
            {
                // Not Squall? Delete the EMPTY overlay node.
                // CrisisDisplay.tscn has not been instantiated yet, so nothing flashes.
                if (player.Character is not Character.Squall)
                {
                    QueueFree();
                    return;
                }

                _player = player;
                break;
            }

            var tree = GetTree();
            if (tree == null)
                return;

            await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        }

        if (_player == null)
        {
            QueueFree();
            return;
        }

        if (_exiting || !IsInsideTree())
            return;

        var scene = GD.Load<PackedScene>("res://Squall/scenes/crisis_display.tscn");
        if (scene == null)
        {
            GD.PushError("[Squall Crisis] Failed to load res://Squall/scenes/crisis_display.tscn");
            QueueFree();
            return;
        }

        _crisisDisplay = scene.Instantiate<Control>();
        AddChild(_crisisDisplay);

        _crisisDisplay.MouseFilter = MouseFilterEnum.Ignore;
        _crisisDisplay.SetAnchorsPreset(LayoutPreset.BottomLeft);

        // Same position as Cloud Limit.
        // Adjust this if Squall gets multiple overlays like Crisis/AP/Firepower.
        _crisisDisplay.Position = new Vector2(-70, 120);
        _crisisDisplay.Visible = true;

        _label = _crisisDisplay.GetNodeOrNull<RichTextLabel>("%CrisisLabel");

        if (_label == null)
        {
            GD.PushError("[Squall Crisis] Could not find %CrisisLabel in CrisisDisplay.tscn");
            QueueFree();
            return;
        }

        _label.TreeExiting += () =>
        {
            _popTween?.Kill();
            _popTween = null;
            _label = null;
        };

        var font = GD.Load<Font>("res://themes/kreon_bold_shared.tres");

        if (font != null)
        {
            _label.AddThemeFontOverride("font", font);
            _label.AddThemeFontOverride("normal_font", font);
        }
        else
        {
            GD.PushWarning("[Squall Crisis] Failed to load res://themes/kreon_bold_shared.tres");
        }

        _label.AddThemeColorOverride("default_color", CrisisDefaultBlue);
        _label.Position += new Vector2(30f, -15f);
        _label.AddThemeColorOverride("font_outline_color", new Color(0.2f, 0.2f, 0.2f));
        _label.AddThemeConstantOverride("outline_size", 12);
        _label.AddThemeFontSizeOverride("normal_font_size", 26);

        _hoverTip = SquallStaticHoverTip.Crisis;

        _label.MouseFilter = MouseFilterEnum.Pass;
        _label.Connect(SignalName.MouseEntered, Callable.From(OnHovered));
        _label.Connect(SignalName.MouseExited, Callable.From(OnUnhovered));

        MouseFilter = MouseFilterEnum.Pass;
        Connect(SignalName.MouseEntered, Callable.From(OnHovered));
        Connect(SignalName.MouseExited, Callable.From(OnUnhovered));

        var data = CrisisManager.GetDataForUI(_player);
        data.OnCrisisChanged += OnCrisisChanged;

        UpdateDisplay(CrisisManager.GetCrisis(_player));
    }

    private void PlayGainPop(bool stayRedAfter)
    {
        if (_exiting)
            return;

        var label = _label;

        if (label == null)
            return;

        if (!GodotObject.IsInstanceValid(label) || label.IsQueuedForDeletion())
            return;

        if (_popTween != null && GodotObject.IsInstanceValid(_popTween))
            _popTween.Kill();

        label.Scale = Vector2.One;

        // Flash green on gain.
        label.Modulate = CrisisGainGreen;

        _popTween = label.CreateTween();

        _popTween.TweenProperty(label, "scale", new Vector2(1.25f, 1.25f), 0.10f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        _popTween.TweenProperty(label, "scale", Vector2.One, 0.40f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        _popTween.Parallel().TweenProperty(
                label,
                "modulate",
                stayRedAfter ? CrisisMaxRed : CrisisDefaultBlue,
                0.40f
            )
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
    }

    private void OnHovered()
    {
        if (_exiting)
            return;

        if (_hoverTip == null)
            return;

        NHoverTipSet.Clear();

        var tip = NHoverTipSet.CreateAndShow(this, _hoverTip);
        tip.GlobalPosition = GlobalPosition + new Vector2(-75f, -550f);
        tip.MouseFilter = MouseFilterEnum.Ignore;
    }

    private void OnUnhovered()
    {
        NHoverTipSet.Remove(this);
    }

    private void OnCrisisChanged(int value)
    {
        UpdateDisplay(value);
    }

    private void UpdateDisplay(int value)
    {
        if (_exiting)
            return;

        var label = _label;

        if (label == null)
            return;

        if (!GodotObject.IsInstanceValid(label) || label.IsQueuedForDeletion())
            return;

        try
        {
            label.Text = $"[center]{value}[/center]";
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        bool isMaxed = value >= CrisisMax;

        if (_lastValue >= 0 && value > _lastValue)
        {
            PlayGainPop(isMaxed);
        }
        else
        {
            label.Scale = Vector2.One;
            label.Modulate = isMaxed ? CrisisMaxRed : Colors.White;
        }

        _lastValue = value;
    }

    public override void _ExitTree()
    {
        _exiting = true;

        if (_popTween != null && GodotObject.IsInstanceValid(_popTween))
            _popTween.Kill();

        _popTween = null;

        if (_player != null)
        {
            var data = CrisisManager.GetDataForUI(_player);
            data.OnCrisisChanged -= OnCrisisChanged;
        }

        NHoverTipSet.Remove(this);

        _label = null;
        _crisisDisplay = null;
        _player = null;
        _hoverTip = null;

        if (Instance == this)
            Instance = null;
    }
}

[HarmonyPatch(typeof(NEnergyCounter), nameof(NEnergyCounter._Ready))]
public static class CrisisDisplayOverlayPatch
{
    public static void Postfix(NEnergyCounter __instance)
    {
        if (__instance == null)
            return;

        if (!GodotObject.IsInstanceValid(__instance) || __instance.IsQueuedForDeletion())
            return;

        if (__instance.GetNodeOrNull<CrisisDisplayOverlay>("CrisisDisplayOverlay") != null)
            return;

        var overlay = new CrisisDisplayOverlay
        {
            Name = "CrisisDisplayOverlay"
        };

        __instance.AddChild(overlay);
    }
}
