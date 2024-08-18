using DunGen;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(SaveFileUISlot))]
    internal class SaveFileUISlotPatch
    {
        [HarmonyPatch("OnEnable")]
        [HarmonyPostfix]
        static void OnEnablePatch(SaveFileUISlot __instance)
        {
            LCDuelsModBase.Instance.saveFileUISlots.Add(__instance);
            if (!LCDuelsModBase.playing)
            {
                return;
            }
            if (__instance.fileNum == -1)
            {
                __instance.enabled = false;
                LCDuelsModBase.Instance.specialTipText = __instance.specialTipText;
            }
            else
            {
            Transform text = __instance.gameObject.transform.Find("Text (TMP)");
            UnityEngine.Debug.Log(text);
            TextMeshProUGUI fileNameText = text.GetComponent<TextMeshProUGUI>();
            __instance.fileStatsText.text = "";
            __instance.fileNotCompatibleAlert.enabled = false;
            switch (__instance.fileNum)
            {
                case -1:
                    __instance.enabled = false;
                    break;
                case 0:
                    fileNameText.text = "Best of 1";
                    __instance.transform.parent.Find("EnterAName").GetComponent<TextMeshProUGUI>().text = "Game modes";
                    break;
               case 1:
                    fileNameText.text = "Best of 3";
                    break;
               case 2:
                    fileNameText.text = "High quota";
                    break;
            }
            }
        }

        [HarmonyPatch(nameof(SaveFileUISlot.SetFileToThis))]
        [HarmonyPostfix]
        static void patchSetFileToThis(SaveFileUISlot __instance)
        {
            if (!LCDuelsModBase.playing)
            {
                return;
            }
            if (LCDuelsModBase.Instance.isPublicQueue)
            {
                LCDuelsModBase.Instance.specialTipText.text = "Try to get as much loot as possible in 1 day without dying.";
                GameNetworkManager.Instance.currentSaveFileName = "LCSaveFile1";
                GameNetworkManager.Instance.saveFileNum = 1;
                LCDuelsModBase.Instance.gameMode = 1;
            }
            else
            {
            switch (__instance.fileNum)
            {
                case 0:
                LCDuelsModBase.Instance.gameMode = 1;
                    LCDuelsModBase.Instance.specialTipText.text = "Try to get as much loot as possible in 1 day without dying."; 
                    break;
               case 1:
                LCDuelsModBase.Instance.gameMode = 2;
                    LCDuelsModBase.Instance.specialTipText.text = "You get a point for getting more loot in a day. You have 3 days to win.";
                    break;
               case 2:
                LCDuelsModBase.Instance.gameMode = 3;
                    LCDuelsModBase.Instance.specialTipText.text = "Whoever survives more days without ejecting wins.";
                    break;
            }
            }
            __instance.SetButtonColorForAllFileSlots();
            LCDuelsModBase.Instance.specialTipText.enabled = true;
        }
    }
}
