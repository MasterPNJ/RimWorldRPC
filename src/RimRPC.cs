﻿using System;
using DiscordGameSDKWrapper;
using RimWorld;
using Verse;
using UnityEngine;

namespace RimRPC
{
    [StaticConstructorOnStartup]
    public class RimRPC
    {
        private const long ClientId = 1288106578825969816; // Your application ID, this one by default
        public static long lastEventTime = 0;
        public static string lastEventText = "";
        private const long eventDuration = 60; // 1 minute in seconds
        internal static Discord discord;
        internal static long Started = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        internal static ActivityManager activityManager;
        internal static Activity activity;

        public static void BootMeUp()
        {
            Initialize();
            UpdatePresence();
        }

        public static void Initialize()
        {
            try
            {
                discord = new Discord(ClientId, (UInt64)CreateFlags.NoRequireDiscord);
                activityManager = discord.GetActivityManager();
                activity = new Activity();
                Log.Message("RimRPC : Discord Game SDK initialisé avec le client ID par défaut.");
            }
            catch (Exception ex)
            {
                Log.Error($"RimRPC : Échec de l'initialisation du Discord Game SDK : {ex.Message}");
            }
        }

        public static void UpdatePresence()
        {
            if (discord == null)
            {
                Log.Error("RimRPC : Discord SDK non initialisé.");
                return;
            }

            var settings = RWRPCMod.Settings;

            // If event display is disabled, do not display events
            if (!settings.RpcShowGameMessages)
            {
                lastEventText = "";
                lastEventTime = 0;
            }

            // Status updated according to events
            string state = GetActivityState(settings);
            string details = GetActivityDetails(settings);

            // Check whether an event is still being displayed
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (currentTime - lastEventTime < eventDuration && !string.IsNullOrEmpty(lastEventText))
            {
                state = lastEventText; // Maintain the current event
            }

            activity.State = state;
            activity.Details = details;
            activity.Timestamps = new ActivityTimestamps
            {
                Start = Started
            };
            activity.Assets = new ActivityAssets
            {
                LargeImage = "logo",
                LargeText = "RimWorld"
            };

            activityManager.UpdateActivity(activity, (result) =>
            {
                if (result == Result.Ok)
                {
                    //Log.Message("RimRPC : Rich Presence mise à jour avec succès."); // Only for debugging --> spam in the log
                }
                else
                {
                    //Log.Error($"RimRPC : Échec de la mise à jour de la Rich Presence : {result}"); // Only for debugging --> spam in the log
                }
            });
        }

        private static string GetActivityState(RwrpcSettings settings)
        {
            if (settings.RpcCustomBottom)
            {
                return settings.RpcCustomBottomText;
            }
            else
            {
                string state = "";

                if (settings.RpcColonistCount)
                {
                    int colonistCount = GetColonistCount();
                    state += "RPC_ColonistCountLabel".Translate() + ": " + colonistCount + "\n";
                }

                if (settings.RpcBiome)
                {
                    string biome = GetCurrentBiome();
                    state += "RPC_BiomeLabel".Translate() + ": " + biome + "\n";
                }

                return state;
            }
        }

        private static string GetActivityDetails(RwrpcSettings settings)
        {
            if (settings.RpcCustomTop)
            {
                return settings.RpcCustomTopText;
            }
            else
            {
                string details = "";

                if (settings.RpcDay || settings.RpcHour || settings.RpcQuadrum || settings.RpcYear || settings.RpcYearShort)
                {
                    var gameTime = GetGameTimeString(settings);
                    details += gameTime + "\n";  // Back to the line after the time
                }

                if (settings.RpcColony)
                {
                    string colonyName = GetColonyName();
                    details += "RPC_ColonyLabel".Translate() + ": " + colonyName + "\n";
                }

                return details;
            }
        }

        // Methods for retrieving game information
        private static int GetColonistCount()
        {
            return PawnsFinder.AllMaps_FreeColonists.Count;
        }

        private static string GetCurrentBiome()
        {
            var map = Find.CurrentMap;
            if (map != null && map.Biome != null)
            {
                return map.Biome.LabelCap;
            }
            return "N/A";
        }

        private static string GetGameTimeString(RwrpcSettings settings)
        {
            var builder = new System.Text.StringBuilder();

            var tickManager = Find.TickManager;
            var currentMap = Find.CurrentMap;
            var worldGrid = Find.WorldGrid;

            if (tickManager != null && currentMap != null && worldGrid != null)
            {
                var tile = currentMap.Tile;

                // Check that the tile is valid
                if (tile >= 0 && tile < worldGrid.TilesCount)
                {
                    var longitude = worldGrid.LongLatOf(tile).x;
                    var ticksAbs = tickManager.TicksAbs;

                    var daysPassed = GenDate.DaysPassed;
                    var quadrum = GenDate.Quadrum(ticksAbs, longitude);
                    var year = GenDate.Year(ticksAbs, longitude);

                    if (settings.RpcDay)
                    {
                        builder.Append("RPC_DayLabel".Translate() + $": {daysPassed}");
                    }
                    if (settings.RpcQuadrum)
                    {
                        if (builder.Length > 0) builder.Append(" ");
                        builder.Append($"{quadrum}");
                    }
                    if (settings.RpcYear)
                    {
                        if (builder.Length > 0) builder.Append(" ");
                        builder.Append("RPC_YearLabel".Translate() + $": {year}");
                    }
                    else if (settings.RpcYearShort)
                    {
                        if (builder.Length > 0) builder.Append(" ");
                        builder.Append("RPC_YearShortLabel".Translate() + $": {year}");
                    }
                    if (settings.RpcHour)
                    {
                        int hour = GenLocalDate.HourOfDay(currentMap);
                        if (builder.Length > 0) builder.Append(" ");
                        builder.Append("RPC_HourLabel".Translate() + $": {hour}");
                    }
                }
                else
                {
                    // Handle the case where the tile is invalid
                    builder.Append("RPC_Playing".Translate());
                }
            }
            else
            {
                // The game has not yet been fully initialised
                builder.Append("RPC_MainMenu".Translate());
            }

            return builder.ToString();
        }

        private static string GetColonyName()
        {
            if (Find.World != null && Find.World.info != null)
            {
                return Find.World.info.name;
            }
            else
            {
                return "RPC_ColonyLabel".Translate();
            }
        }

        public static void Shutdown()
        {
            if (discord != null)
            {
                discord.Dispose();
                discord = null;
                Log.Message("RimRPC : Discord SDK fermé.");
            }
        }

        public static void GoToMainMenu_Postfix()
        {
            UpdatePresence();
        }

        // Regular update method
        public static void Update()
        {
            if (discord != null)
            {
                discord.RunCallbacks();
            }
        }
    }
}
