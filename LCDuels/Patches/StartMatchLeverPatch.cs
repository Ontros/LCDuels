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
        static bool patchChooseNewRandomMapSeed(ref bool ___leverHasBeenPulled)
        {
            if (LCDuelsModBase.playing)
            {
                if (LCDuelsModBase.Instance.gameReady)
                {
                    return true;
                }
                else
                {
                    _ = LCDuelsModBase.Instance.SendMessage(new { type = "ready" });
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
