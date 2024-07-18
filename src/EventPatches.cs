using HarmonyLib;
using RimWorld;
using Verse;
using System;  // Ajout de cette ligne

namespace RimRPC
{
    [StaticConstructorOnStartup]
    public static class EventPatches
    {
        static EventPatches()
        {
            Log.Message("RimRPC: Initializing Harmony patches...");
            var harmony = new Harmony("com.rimworld.mod.rimrpc");
            try
            {
                harmony.PatchAll();
                Log.Message("RimRPC: Harmony patches applied successfully.");
            }
            catch (Exception ex)
            {
                Log.Error("RimRPC: Failed to apply Harmony patches. Exception: " + ex);
            }
        }

        [HarmonyPatch(typeof(IncidentWorker_Raid), "TryExecuteWorker")]
        public static class IncidentWorker_Raid_TryExecuteWorker_Patch
        {
            public static void Postfix(bool __result)
            {
                Log.Message("RimRPC: Patch for IncidentWorker_Raid applied.");
                if (__result)
                {
                    Log.Message("RimRPC: Raid detected!");
                    RimRPC.UpdateLastEvent("Dernier événement : Raid !");
                }
                else
                {
                    Log.Message("RimRPC: Raid attempt failed.");
                }
            }
        }

        [HarmonyPatch(typeof(Building), "SpawnSetup")]
        public static class Building_SpawnSetup_Patch
        {
            public static void Postfix(Building __instance)
            {
                Log.Message($"RimRPC: Patch for Building_SpawnSetup applied. Building created: {__instance.def.defName}");
                if (__instance.def.building != null && __instance.def.building.isNaturalRock)
                {
                    Log.Message("RimRPC: Colony building detected!");
                    RimRPC.UpdateLastEvent("Dernier événement : Colonie établie");
                }
            }
        }

        [HarmonyPatch(typeof(Messages), "Message")]
        public static class Messages_Message_Patch
        {
            public static void Postfix(TaggedString text, MessageTypeDef def)
            {
                Log.Message($"RimRPC: New message detected: {text}");
                RimRPC.UpdateLastEvent($"Dernier événement : {text}");
            }
        }

        [HarmonyPatch(typeof(IncidentWorker_ResourcePodCrash), "TryExecuteWorker")]
        public static class IncidentWorker_ResourcePodCrash_TryExecuteWorker_Patch
        {
            public static void Postfix(bool __result)
            {
                Log.Message("RimRPC: Patch for IncidentWorker_ResourcePodCrash applied.");
                if (__result)
                {
                    Log.Message("RimRPC: Resource Pod Crash detected!");
                    RimRPC.UpdateLastEvent("Dernier événement : Crash de Pod de Ressources");
                }
                else
                {
                    Log.Message("RimRPC: Resource Pod Crash attempt failed.");
                }
            }
        }
    }
}