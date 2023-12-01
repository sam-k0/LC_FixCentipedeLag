using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace LC_Optim
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony thisHarmony;
        private void Awake()
        {
            thisHarmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            thisHarmony.Patch(typeof(CentipedeAI).GetMethod("DoAIInterval"), postfix: new HarmonyMethod(typeof(Plugin), nameof(RemoveLagCentipede)));
            Logger.LogWarning("Registered the patch method.");
        }


        public static void RemoveLagCentipede(CentipedeAI __instance)
        {
            if(!__instance.TargetClosestPlayer(1.5f, false, 70f))
            {
                // Enemy is probably stuck
                __instance.KillEnemyOnOwnerClient();                
            }
        }

        public void OnDestroy()
        {
            thisHarmony.UnpatchSelf();
        }
    }
}