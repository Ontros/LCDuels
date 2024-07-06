using BepInEx.Logging;
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
            if (LCDuelsModBase.playing)
            {
                if (LCDuelsModBase.Instance.waitingForResult)
                {
                    return false;
                }
                else if (LCDuelsModBase.Instance.gameStarted)
                {
                    //Liftoff
                    return true;
                }
                else if (LCDuelsModBase.Instance.gameReady)
                {
                    //Ready so start game
                    return true;
                }
                else
                {
                    //Get ready
                    LCDuelsModBase.Instance.mls.LogInfo("Sending ready");
                    _ = LCDuelsModBase.Instance.SendMessage(new { type = "ready" });
                    LCDuelsModBase.Instance.matchLever = __instance;
                    __instance.StartCoroutine(LCDuelsModBase.Instance.waitUntilGameIsReady());
                    return false;
                }
            }
            else
            {
                //Just playing
                return true;
            }
        }
    }
}
