using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(StartMatchLever))]
    internal class StartMatchLeverPatch
    {
        [HarmonyPatch(nameof(StartMatchLever.PullLever))]
        [HarmonyPrefix]
        static bool patchChooseNewRandomMapSeed(StartMatchLever __instance)
        {
            //LCDuelsModBase.Instance.mls.LogInfo("Pull lever called");
            if (LCDuelsModBase.playing)
            {
                if (LCDuelsModBase.Instance.gameReady)
                {
                    return true;
                }
                else
                {
                    _ = LCDuelsModBase.Instance.SendMessage(new { type = "ready" });
                    LCDuelsModBase.Instance.matchLever = __instance;
                    __instance.StartCoroutine(LCDuelsModBase.Instance.waitUntilGameIsReady());
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    }
}
