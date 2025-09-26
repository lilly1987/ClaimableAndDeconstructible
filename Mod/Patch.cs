using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static HarmonyLib.Code;

namespace Lilly.ClaimableAndDeconstructible
{
    public static class Patch
    {
        public static HarmonyX harmony = null;
        public static string harmonyId = "Lilly.";

        public static void OnPatch(bool repatch = false)
        {
            if (repatch)
            {
                Unpatch();
            }
            if (harmony != null || !Settings.onPatch) return;
            harmony = new HarmonyX(harmonyId);
            try
            {
                harmony.PatchAll();
                MyLog.Message($"Patch <color=#00FF00FF>Succ</color>");
            }
            catch (System.Exception e)
            {
                MyLog.Error($"Patch Fail");
                MyLog.Error(e.ToString());
                MyLog.Error($"Patch Fail");
            }

        }

        public static void Unpatch()
        {
            MyLog.Message($"UnPatch");
            if (harmony == null) return;
            harmony.UnpatchSelf();
            harmony = null;
        }
        /*
        */
        [HarmonyPatch(typeof(WorkGiver_Uninstall), "HasJobOnThing")]
        [HarmonyPrefix]
        public static bool ClaimableBy(WorkGiver_Uninstall __instance, ref bool __result, Pawn pawn, Thing t, bool forced = false)
        {
            if (t.def.Claimable)
            {
                //if (t.Faction != pawn.Faction)
                //{
                //    __result= false;
                //    return false;
                //}
            }
            else if (pawn.Faction != Faction.OfPlayer)
            {
                __result = false;
                return false;
            }
            __result = __instance.HasJobOnThing(pawn, t, forced);
            return false;
        }

        [HarmonyPatch(typeof(Building), "ClaimableBy")]
        [HarmonyPrefix]
        public static bool ClaimableBy(Building __instance, ref AcceptanceReport __result, Faction by)
        {
            if (!__instance.def.Claimable)
            {
                __result = false;
                return false;
            }
            if (__instance.Faction == by)
            {
                __result = false;
                return false;
            }
            for (int i = 0; i < __instance.AllComps.Count; i++)
            {
                if (__instance.AllComps[i].CompPreventClaimingBy(by))
                {
                    __result = false;
                    return false;
                }
            }
            __result = true;
            return false;
            // 맵 조건 무시
            Faction faction;
            if ((faction = __instance.Faction) == null)
            {
                Map map = __instance.Map;
                faction = ((map != null) ? map.ParentFaction : null);
            }
            string value;
            if (__instance.FactionPreventsClaimingOrAdopting(faction, true, out value))
            {
                __result = value;
                return false;
            }
            __result = true;
            return false;
        }

        [HarmonyPatch(typeof(Building), "DeconstructibleBy")]
        [HarmonyPrefix]
        public static bool DeconstructibleBy(Building __instance,ref AcceptanceReport __result, Faction faction)
        {
            for (int i = 0; i < __instance.AllComps.Count; i++)
            {
                if (__instance.AllComps[i].CompForceDeconstructable())
                {
                    __result = true;
                    return false;
                }
            }
            if (!__instance.def.building.IsDeconstructible)
            {
                __result = false;
                return false;
            }
            __result = true;
            return false;
            if (DebugSettings.godMode)
            {
                __result = true;
                return false;
            }
            if (__instance.Faction == faction)
            {
                __result = true;
                return false;
            }
            if (__instance.def.building.alwaysDeconstructible)
            {
                __result = true;
                return false;
            }
            __result = __instance.ClaimableBy(faction);
            return false;
        }
    }
}
