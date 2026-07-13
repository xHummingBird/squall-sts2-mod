using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using Squall.SquallCode.Cards.Ancient;
using Squall.SquallCode.Cards.Basic;
using Squall.SquallCode.Relics;

namespace Squall.SquallCode.Extensions;

[HarmonyPatch(typeof(TouchOfOrobas), "GetUpgradedStarterRelic")]
internal static class SquallTouchOfOrobasPatch
{
    private static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic is Revolver)
        {
            __result = ModelDb.Relic<Lionheart>().ToMutable();
        }
    }
}


[HarmonyPatch(typeof(ArchaicTooth), "TranscendenceUpgrades", MethodType.Getter)]
internal static class SquallArchaicToothTranscendencePatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Dictionary<ModelId, CardModel> __result)
    {
        __result[ModelDb.Card<SummonGf>().Id] = ModelDb.Card<PerfectJunction>();
    }
}


// [HarmonyPatch(typeof(DustyTome), nameof(DustyTome.AfterObtained))]
// public static class DustyTomePatch
// {
//     [HarmonyPrefix]
//     public static void Prefix(DustyTome __instance)
//     {
//         if (__instance.Owner?.Character is not Character.Squall)
//             return;
//
//         if (__instance.AncientCard == null)
//         {
//             __instance.AncientCard = ModelDb.Card<HerosLastWish>().Id;
//         }
//     }
// }