using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Squall.SquallCode.Extensions;

public static class SquallAssets
{
    private static PackedScene? _squallScene;
    private static PackedScene? _vfxScene;

    private const string SquallScenePath = "res://Squall/scenes/squall.tscn";
    private const string VfxPath = "res://Squall/scenes/vfx.tscn";

    public static PackedScene? SquallScene
    {
        get
        {
            _squallScene = LoadOrReload(_squallScene, SquallScenePath, "Squall scene");
            return _squallScene;
        }
    }

    public static PackedScene? IceScene
    {
        get
        {
            _vfxScene = LoadOrReload(_vfxScene, VfxPath, "Ice VFX");
            return _vfxScene;
        }
    }

    private static PackedScene? LoadOrReload(PackedScene? cachedScene, string path, string label)
    {
        if (cachedScene != null && GodotObject.IsInstanceValid(cachedScene))
            return cachedScene;

        GD.Print($"SquallAssets: Loading {label} from {path}");

        var scene = GD.Load<PackedScene>(path);

        if (scene == null)
        {
            GD.PrintErr($"SquallAssets: FAILED to load {label}: {path}");
            return null;
        }

        GD.Print($"SquallAssets: Loaded {label}");
        return scene;
    }

    public static void EnsurePreloaded()
    {
        _ = SquallScene;
        _ = IceScene;

        GD.Print("SquallAssets: EnsurePreloaded finished");
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterActEntered))]
public static class SquallAfterActEnteredPreloadPatch
{
    [HarmonyPrefix]
    public static void Prefix(IRunState runState)
    {
        var player = runState?.Players?.FirstOrDefault();

        if (player?.Character is not Character.Squall)
            return;

        GD.Print("AfterActEntered: Squall detected → preloading");

        SquallAssets.EnsurePreloaded();
    }
}


[HarmonyPatch(typeof(Hook), nameof(Hook.AfterRoomEntered))]
public static class SquallAfterRoomEnteredPreloadPatch
{
    [HarmonyPrefix]
    public static void Prefix(IRunState runState, AbstractRoom room)
    {
        var player = runState?.Players?.FirstOrDefault();

        if (player?.Character is not Character.Squall)
            return;

        GD.Print($"AfterRoomEntered: Squall detected → preloading. Room = {room.GetType().Name}");

        SquallAssets.EnsurePreloaded();
    }
}
