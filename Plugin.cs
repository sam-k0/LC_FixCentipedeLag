using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;


namespace LC_Optim
{
    [BepInPlugin(PluginMetadata.PLUGIN_GUID, PluginMetadata.PLUGIN_NAME, PluginMetadata.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony thisHarmony;
        private static Dictionary<int, ulong> instanceMap = new Dictionary<int, ulong>();
        private static ulong deadtimer = 100;
        private void Awake()
        {
            thisHarmony = new Harmony(PluginMetadata.PLUGIN_GUID);
            thisHarmony.Patch(typeof(CentipedeAI).GetMethod("DoAIInterval"), postfix: new HarmonyMethod(typeof(Plugin), nameof(RemoveLagCentipede)), prefix: new HarmonyMethod(typeof(Plugin), nameof(RemoveLagCentipede)));
            Logger.LogWarning("Registered the patch method.");
            Logger.LogWarning("Deactivating UnityLog");
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