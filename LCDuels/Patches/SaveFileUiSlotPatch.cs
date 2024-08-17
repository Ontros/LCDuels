using DunGen;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(SaveFileUISlot))]
    internal class SaveFileUISlotPatch
    {
        [HarmonyPatch("OnEnable")]
        [HarmonyPostfix]
        static void OnEnablePatch(SaveFileUISlot __instance)
        {
            UnityEngine.Debug.Log("1eneable save file UI slot");
            LCDuelsModBase.Instance.saveFileUISlots.Add(__instance);
            UnityEngine.Debug.Log("2eneable save file UI slot"+__instance.fileNum);
            TextMeshProUGUI fileNameText = __instance.gameObject.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
            switch (__instance.fileNum)
            {
                case -1:
            UnityEngine.Debug.Log("-1eneable save file UI slot");
                    __instance.enabled = false;
                    break;
                case 0:
            UnityEngine.Debug.Log("0eneable save file UI slot");
                    fileNameText.text = "Bo1";
                    break;
               case 1:
            UnityEngine.Debug.Log("1eneable save file UI slot");
                    fileNameText.text = "Bo3";
                    break;
               case 2:
            UnityEngine.Debug.Log("2eneable save file UI slot");
                    fileNameText.text = "High quota";
                    break;
            }
            UnityEngine.Debug.Log("3eneable save file UI slot");
            __instance.fileStatsText.text = "";
            UnityEngine.Debug.Log("4eneable save file UI slot");
            __instance.fileNotCompatibleAlert.enabled = false;
            __instance.transform.parent.Find("EnterAName").GetComponent<TextMeshProUGUI>().text = "Game modes";
        }
    }
}
