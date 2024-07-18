using HarmonyLib;
using RimWorld;
using Verse;
using System;

namespace RimRPC
{
    [StaticConstructorOnStartup]
    public static class EventPatches
    {
        static EventPatches()
        {
            Log.Message("RimRPC: Initializing Harmony patches...");
            var harmony = new Harmony("com.rimworld.mod.rimrpc");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(Messages))]
        [HarmonyPatch("Message", new Type[] { typeof(TaggedString), typeof(MessageTypeDef), typeof(bool) })]
        public static class Messages_Message_Patch
        {
            public static void Postfix(TaggedString text, MessageTypeDef def)
            {
                RimRPC.UpdateLastEvent($"Event: {text}");
            }
        }
    }
}