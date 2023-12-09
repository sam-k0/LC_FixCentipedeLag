using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace LC_Optim
{
    [BepInPlugin(PluginMetadata.PLUGIN_GUID, PluginMetadata.PLUGIN_NAME, PluginMetadata.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony thisHarmony;
        private static Dictionary<int, ulong> instanceMap = new Dictionary<int, ulong>();
        private static ulong deadtimer = 100;
        private static ManualLogSource Log;
        private static ConfigEntry<bool> configShowDebug;
        private static ConfigEntry<ulong> configDeadTimer;

        static void Debug(object data, LogLevel logLevel = LogLevel.Info)
        {
            if(configShowDebug.Value)Log.Log(logLevel, data);
        }

        private void Awake()
        {
            configShowDebug = Config.Bind("General", "Enable debug printing", true, "Enabling this will show debug info in console, e.g. when a new centipede gets tracked or removed.");
            configDeadTimer = Config.Bind("General", "Dead Timer", (ulong)100, "If the enemy tries to jump to the ceiling multiple times during that interval (in frames), it is assumed to be stuck." +
                " This will prevent the enemy from lagging the game.");

            thisHarmony = new Harmony(PluginMetadata.PLUGIN_GUID);
            thisHarmony.Patch(typeof(CentipedeAI).GetMethod("DoAIInterval"), prefix: new HarmonyMethod(typeof(Plugin), nameof(RemoveLagCentipede)));
            Debug("Registered the patch method", LogLevel.Warning);
            Log = Logger;
            
            
        }


        public static void RemoveLagCentipede(CentipedeAI __instance)
        {          
            if(!__instance.TargetClosestPlayer(1.5f, false, 70f))
            {
                int oid = __instance.GetInstanceID();
                ulong tfc = (ulong)Time.frameCount;
                if (!instanceMap.ContainsKey(oid))
                { 
                    instanceMap.Add(oid, tfc);
                    Debug($"Tracked {oid}");
                }
                else
                {
                    // already tracked
                    // check when the last call was
                    ulong lastFrameCount = instanceMap[oid];
                    ulong framesPassed = tfc - lastFrameCount;
                    if(framesPassed <= deadtimer)
                    {
                        // Delete enemy as it triggered this too often
                        __instance.KillEnemy(true);
                        // clear entry
                        instanceMap.Remove(oid);
                        Debug($"Removed centipede at {oid}", LogLevel.Info);
                    }
                    else // update the entry
                    {
                        instanceMap[oid] = tfc;
                    }
                }               
            }            
        }

        public void OnDestroy()
        {
            thisHarmony.UnpatchSelf();
        }
    }
}