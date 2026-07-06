using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Squall.SquallCode.Extensions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Cards.Basic;
using Squall.SquallCode.Relics;

namespace Squall.SquallCode.Character;

public class Squall : PlaceholderCharacterModel
{
    public const string CharacterId = "Squall";

    public static readonly Color Color = new("ffffff");
    private Vector2? _originalPosition;
    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Masculine;
    public override int StartingHp => 75;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeSquall>(),
        ModelDb.Card<StrikeSquall>(),
        ModelDb.Card<StrikeSquall>(),
        ModelDb.Card<StrikeSquall>(),
        ModelDb.Card<StrikeSquall>(),
        ModelDb.Card<DefendSquall>(),
        ModelDb.Card<DefendSquall>(),
        ModelDb.Card<DefendSquall>(),
        ModelDb.Card<DefendSquall>(),
        ModelDb.Card<DefendSquall>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<Revolver>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<SquallCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<SquallRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SquallPotionPool>();

    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets.
        These are just some of the simplest assets, given some placeholders to differentiate your character with.
        You don't have to, but you're suggested to rename these images. */
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }
    
    public override CustomEnergyCounter? CustomEnergyCounter =>
        new CustomEnergyCounter(EnergyCounterPaths, new Color(0.2f, 0.2f, 0.2f), new Color(1f, 1f, 1f));
    
    private string EnergyCounterPaths(int i)
    {
        return i switch
        {
            1 => "charui/big_energy.png".ImagePath(),
            _ => "charui/blank.png".ImagePath()
        };
    }
    
    private const string CustomVisualScenePath = "res://Squall/scenes/squall.tscn";
    public override string CustomRestSiteAnimPath => "res://Squall/scenes/Squall_rest_site.tscn";
    
    // public override string CustomCharacterSelectBg => "char_selection_bg_squall.tscn".CharacterUiPath();
    public override string CustomMerchantAnimPath => "res://Cloud/scenes/Squall_merchant.tscn";
    public override string CustomIconTexturePath => "character_icon_squall.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_squall.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_squall.png".CharacterUiPath();
    // public override string CharacterSelectSfx => "res://Cloud/sounds/not_interested.wav";
    // public override string CharacterTransitionSfx => "res://Cloud/sfx/sword_swing_heavy.wav";
    
    // public override string CustomCharacterSelectTransitionPath =>
    //    "res://Cloud/images/transition/cloud_transition_mat.tres";
    
    public override NCreatureVisuals? CreateCustomVisuals()
    {
        // SquallAssets.EnsurePreloaded();
        return NodeFactory<NCreatureVisuals>.CreateFromScene(CustomVisualScenePath);
    }
    
    public override CreatureAnimator? GenerateAnimator(MegaSprite controller) => null;
    
    public (float total, float[] impacts) PlayAnimation(Creature creature, string trigger)
    {
        if (creature == null || string.IsNullOrEmpty(trigger))
            return (0f, Array.Empty<float>());

        var node = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (node?.Visuals == null)
            return (0f, Array.Empty<float>());

        var animPlayer = node.Visuals.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if (animPlayer == null)
            return (0f, Array.Empty<float>());

        string godotTrigger = trigger.ToLowerInvariant() switch
        {
            "hit" => "hurt",
            "idle" => "idle",
            "attack" => "attack_squall",
            "dead" => "die",
            "die" => "die",
            _ => trigger
        };

        if (!animPlayer.HasAnimation(godotTrigger))
            return (0f, Array.Empty<float>());

        var anim = animPlayer.GetAnimation(godotTrigger);
        float totalLength = (float)anim.Length;

        animPlayer.Play(godotTrigger);
        
        if (godotTrigger != "idle" && godotTrigger != "die")
            animPlayer.Queue("idle");
        
        return (totalLength, Array.Empty<float>());
    } 
    
    public async Task DashTo(
        Creature player,
        Creature target,
        float durationSeconds = 0.3f,
        float distance = 200f,
        bool dashBehind = false,
        string? overrideAnim = null)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(player);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        if (node == null || targetNode == null) return;

        if (!_originalPosition.HasValue)
            _originalPosition = node.GlobalPosition;
        
        PlayAnimation(player, overrideAnim ??"dash");
		
        bool playerIsLeftOfTarget = node.GlobalPosition.X < targetNode.GlobalPosition.X;
		
        Vector2 offsetDir = playerIsLeftOfTarget ? Vector2.Left : Vector2.Right;
		
        if (dashBehind)
            offsetDir = -offsetDir;

        Vector2 targetPos = targetNode.GlobalPosition + offsetDir * distance;

        var tween = node.CreateTween();
        tween.TweenProperty(node, "global_position", targetPos, durationSeconds)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        await node.ToSignal(tween, Tween.SignalName.Finished);
    }
    
    public async Task DashPast(
        Creature player,
        Creature target,
        string? attackAnim = null,
        float durationSeconds = 0.3f,
        float behindDistance = 200f,
        float overshoot = 0f)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(player);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        if (node == null || targetNode == null) return;

        if (!_originalPosition.HasValue)
            _originalPosition = node.GlobalPosition;

        Vector2 frontDir = (player.Side == CombatSide.Player) ? Vector2.Left : Vector2.Right;
        Vector2 behindDir = -frontDir;

        Vector2 endPos = targetNode.GlobalPosition + behindDir * (behindDistance + overshoot);

        PlayAnimation(player, attackAnim);

        var tween = node.CreateTween();
        tween.TweenProperty(node, "global_position", endPos, durationSeconds)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        await node.ToSignal(tween, Tween.SignalName.Finished);
    }
    
    
    public async Task Retreat(
        Creature player,
        string? animation = "retreat",
        bool goIdle = true)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(player);
        if (node == null || !_originalPosition.HasValue) return;

        if (!string.IsNullOrEmpty(animation))
            PlayAnimation(player, animation);

        var tween = node.CreateTween();
        tween.TweenProperty(node, "global_position", _originalPosition.Value, 0.3f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.InOut);

        await node.ToSignal(tween, Tween.SignalName.Finished);

        _originalPosition = null;

        var visuals = node.Visuals.GetNodeOrNull<Node2D>("Visuals");
        if (visuals != null)
            visuals.Position = Vector2.Zero;

        if (goIdle)
            PlayAnimation(player, "idle");
    }
    
    public void DoScreenShake(ShakeStrength strength = ShakeStrength.Medium,
        ShakeDuration duration = ShakeDuration.Short)
    {
        NGame.Instance?.ScreenShake(strength, duration);
    }

    public Node2D PlayVfxOnTarget(Creature target, string path, string animName)
    {
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        if (targetNode?.Visuals == null)
            return null;

        var scene = GD.Load<PackedScene>(path);
        var vfx = scene.Instantiate<Node2D>();

        targetNode.Visuals.AddChild(vfx);
        vfx.Position = Vector2.Zero;

        var animPlayer = vfx.GetNode<AnimationPlayer>("AnimationPlayer");

        if (animPlayer.HasAnimation(animName))
            animPlayer.Play(animName);

        return vfx;
    }
    
    [HarmonyPatch(typeof(NCreature), nameof(NCreature.SetAnimationTrigger))]
    public static class NCreatureSetTriggerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(NCreature __instance, string trigger)
        {
            // This ensures the engine's triggers automatically drive your AnimationPlayer.
            if (__instance.Entity?.Player?.Character is Squall character)
            {
                character.PlayAnimation(__instance.Entity, trigger);
                return false; // skip default skeletal animation path
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
    public static class SquallStartDeathAnimPatch
    {
        [HarmonyPostfix]
        public static void Postfix(NCreature __instance, ref float __result)
        {
            if (__instance.Entity?.Player?.Character is Squall character)
            {
                character.PlayAnimation(__instance.Entity, "die");
                var animPlayer = __instance.Visuals.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
                __result = animPlayer?.GetAnimation("die")?.Length ?? 1.5f;
            }
        }
    }
    
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatVictory))]
    public static class SquallVictoryAnimationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(IRunState runState, CombatState? combatState)
        {
            var creatures = combatState?.Creatures?.Where(c => c.IsPlayer);

            if (creatures == null)
                return;

            foreach (var creature in creatures)
            {
                if (creature.Player?.Character is not Squall)
                    continue;

                var node = NCombatRoom.Instance?.GetCreatureNode(creature);
                var animPlayer = node?.Visuals?.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

                if (animPlayer == null)
                    continue;
                AudioHelper.PlayRandomVictory();
                animPlayer.Play("victory_before");
                if (animPlayer.HasAnimation("victory"))
                    animPlayer.Queue("victory");
            }
        }
    }
    
   [HarmonyPatch(typeof(Hook), nameof(Hook.AfterDamageReceived))]
    public static class SquallDamageAnimationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Creature target, DamageResult result, ValueProp props, Creature? dealer)
        {
            if (target.Player?.Character is not Squall character)
                return;
            
            if (dealer == null || dealer.Side != CombatSide.Enemy)
                return;
            
            if (props.HasFlag(ValueProp.SkipHurtAnim) || props.HasFlag(ValueProp.Unpowered))
                return;

            if (result.WasFullyBlocked && result.BlockedDamage > 0)
            {
                character.PlayAnimation(target, "block"); 
            }
            
            else if (result.UnblockedDamage > 0 && !target.IsDead)
            {
                character.PlayAnimation(target, "hit");
                if (target.CurrentHp < 20)
                {
                    AudioHelper.PlayRandomDamagedCritical();
                }
                else if (result.UnblockedDamage < 10)
                {
                    AudioHelper.PlayRandomDamaged();
                }
                else
                {
                    AudioHelper.PlayRandomDamagedHigh();
                }
            }
        }
    }
}