using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void patchStart(Terminal __instance)
        {
            LCDuelsModBase.Instance.mls.LogInfo("Is LCDuels enabled: "+LCDuelsModBase.playing);
            if (LCDuelsModBase.playing)
            {
                __instance.groupCredits = 0;
                LCDuelsModBase.Instance.terminal = __instance;
                StartOfRound.Instance.screenLevelDescription.text = "Waiting for other player";
                StartMatchLever matchLever = UnityEngine.Object.FindFirstObjectByType<StartMatchLever>();
                matchLever.triggerScript.disabledHoverTip = "[Wait for the other player]";
                matchLever.triggerScript.interactable = false;
                LCDuelsModBase.Instance.UpdateInGameStatusText();
                Task.Run(LCDuelsModBase.Instance.InitWS);
            }
        }
    }
}
