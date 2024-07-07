using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        [HarmonyPatch("ChangeControlTip")]
        [HarmonyPrefix]
        static bool changeControlTipPatch()
        {
            return !LCDuelsModBase.playing;
        }

        [HarmonyPatch("ChangeControlTipMultiple")]
        [HarmonyPrefix]
        static bool changeControlTipMultiplePatch()
        {
            return !LCDuelsModBase.playing;
        }

        [HarmonyPatch("ClearControlTips")]
        [HarmonyPrefix]
        static bool clearControlTipPatch()
        {
            return !LCDuelsModBase.playing;
        }

        [HarmonyPatch("SubmitChat_performed")]
        [HarmonyPrefix]
        static bool submitChatPatch() 
        {
            if (LCDuelsModBase.playing)
            {
                if (GameNetworkManager.Instance.localPlayerController == null || !GameNetworkManager.Instance.localPlayerController.isTypingChat)
                {
                    return true;
                }
                return false;
            }
            return true;
        }
    }
}
