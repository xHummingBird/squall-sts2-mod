
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Squall.SquallCode.Extensions;

namespace Squall.SquallCode.Mechanics.Firepower;

public partial class FirepowerDisplayOverlay : Control
{
    public static FirepowerDisplayOverlay? Instance { get; private set; }

    private Control? _display;

    private TextureRect? _fire1;
    private TextureRect? _fire2;
    private TextureRect? _fire3;

    private AnimationPlayer? _anim;

    private Player? _player;
    private IHoverTip? _hoverTip;
    
    private Tween? _fireTween;
    private int _lastProgress = -1;

    public override void _Ready()
    {
        Instance = this;
        Name = "FirepowerDisplayOverlay";

        MouseFilter = MouseFilterEnum.Pass;

        CallDeferred(nameof(Setup));
    }

    private async void Setup()
    {
        if (!IsInsideTree())
            return;

        for (int i = 0; i < 60; i++)
        {
            var state = CombatManager.Instance?.DebugOnlyGetState();

            var player =
                state?.Players.FirstOrDefault(
                    p => LocalContext.IsMe(p));

            if (player != null)
            {
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

            await ToSignal(
                tree,
                SceneTree.SignalName.ProcessFrame);
        }

        if (_player == null)
        {
            QueueFree();
            return;
        }

        var scene =
            GD.Load<PackedScene>(
                "res://Squall/scenes/firepower_display.tscn");

        if (scene == null)
        {
            GD.PushError(
                "[Squall Firepower] Failed to load firepower_display.tscn");

            QueueFree();
            return;
        }

        _display = scene.Instantiate<Control>();

        AddChild(_display);

        _display.MouseFilter = MouseFilterEnum.Pass;

        _display.SetAnchorsPreset(
            LayoutPreset.BottomLeft);

        // Adjust this later if needed
        _display.Position =
            new Vector2(130, 40);

        _fire1 =
            _display.GetNodeOrNull<TextureRect>(
                "fire1");

        _fire2 =
            _display.GetNodeOrNull<TextureRect>(
                "fire2");

        _fire3 =
            _display.GetNodeOrNull<TextureRect>(
                "fire3");

        _anim =
            _display.GetNodeOrNull<AnimationPlayer>(
                "AnimationPlayer");

        _anim?.Play("spin");

        _hoverTip = SquallStaticHoverTip.Firepower;

        _display.Connect(
            SignalName.MouseEntered,
            Callable.From(OnHovered));

        _display.Connect(
            SignalName.MouseExited,
            Callable.From(OnUnhovered));

        UpdateDisplay();
    }

    public override void _Process(double delta)
    {
        UpdateDisplay();
    }
    
    
    private void PlayGainAnimation(Control fire)
    {
        if (fire == null)
            return;

        fire.Visible = true;

        fire.Scale = new Vector2(2f, 2f);

        var tween = fire.CreateTween();

        tween.TweenProperty(
                fire,
                "scale",
                Vector2.One,
                0.5f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }
    
    
    private void AnimateFireConsume(TextureRect? fire)
    {
        if (fire == null)
            return;

        fire.Scale = Vector2.One;
        fire.Modulate = Colors.White;

        var tween = fire.CreateTween();

        tween.Parallel()
            .TweenProperty(
                fire,
                "scale",
                new Vector2(2f, 2f),
                0.5f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        tween.Parallel()
            .TweenProperty(
                fire,
                "modulate:a",
                0f,
                0.5f);

        tween.TweenCallback(
            Callable.From(() =>
            {
                fire.Visible = false;
                fire.Scale = Vector2.One;
                fire.Modulate = Colors.White;
            }));
    }

    
    
    private void PlayConsumeAnimation()
    {
        AnimateFireConsume(_fire1);
        AnimateFireConsume(_fire2);
        AnimateFireConsume(_fire3);
    }

    
    
    private void UpdateDisplay()
    {
        if (_player == null)
            return;

        if (_fire1 == null ||
            _fire2 == null ||
            _fire3 == null)
            return;

        int progress =
            FirepowerHelper.GetFirepowerProgress(_player);

        if (progress == _lastProgress)
            return;

        // Charge gained
        if (progress > _lastProgress)
        {
            if (progress == 1)
                PlayGainAnimation(_fire1);

            if (progress == 2)
                PlayGainAnimation(_fire2);

            if (progress == 3)
                PlayGainAnimation(_fire3);
        }
        // Firepower consumed
        else if (_lastProgress == 3 && progress == 0)
        {
            PlayConsumeAnimation();
        }

        _lastProgress = progress;
        
        if (!(_lastProgress == 3 && progress == 0))
        {
            _fire1.Visible = progress >= 1;
            _fire2.Visible = progress >= 2;
            _fire3.Visible = progress >= 3;
        }

    }


    private void OnHovered()
    {
        if (_hoverTip == null)
            return;
        
        NHoverTipSet.Clear();
        
        var tip =
            NHoverTipSet.CreateAndShow(
                this,
                _hoverTip);

        if (tip != null)
        {
            tip.GlobalPosition =
                GlobalPosition +
                new Vector2(-75f, -350f);

            tip.MouseFilter =
                MouseFilterEnum.Ignore;
        }
    }

    private void OnUnhovered()
    {
        NHoverTipSet.Remove(this);
    }

    public override void _ExitTree()
    {
        NHoverTipSet.Remove(this);

        if (Instance == this)
            Instance = null;

        _display = null;

        _fire1 = null;
        _fire2 = null;
        _fire3 = null;

        _anim = null;
        _player = null;
        _hoverTip = null;
    }
}

[HarmonyPatch(typeof(NEnergyCounter), nameof(NEnergyCounter._Ready))]
public static class FirepowerDisplayOverlayPatch
{
    public static void Postfix(
        NEnergyCounter __instance)
    {
        if (__instance == null)
            return;

        if (!GodotObject.IsInstanceValid(__instance))
            return;

        if (__instance.GetNodeOrNull<FirepowerDisplayOverlay>(
                "FirepowerDisplayOverlay") != null)
        {
            return;
        }

        __instance.AddChild(
            new FirepowerDisplayOverlay
            {
                Name = "FirepowerDisplayOverlay"
            });
    }
}
