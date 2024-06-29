using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuManagerPatch
    {
        [HarmonyPatch("OnEnable")]
        [HarmonyPostfix]
        static void patchOnEnable(MenuManager __instance)
        {
            Button hostButton = Traverse.Create(__instance).Field("startHostButton").GetValue() as Button;
            LCDuelsModBase.Instance.mls.LogInfo(hostButton.ToString());
            LCDuelsModBase.Instance.menuManager = __instance;
            GameObject LCDButton = UnityEngine.Object.Instantiate(hostButton.gameObject);
            LCDButton.transform.SetParent(hostButton.transform.parent.GetComponent<RectTransform>(),false);
            LCDButton.transform.localPosition = hostButton.transform.localPosition + (hostButton.transform.localPosition - __instance.joinCrewButtonContainer.transform.localPosition);
            LCDButton.transform.localRotation = hostButton.transform.localRotation;
            LCDButton.transform.localScale = hostButton.transform.localScale;
            LCDButton.GetComponentInChildren<TextMeshProUGUI>().text = "> Play LC Duels";
            Button LCDbuttonbutton = LCDButton.GetComponent<Button>();
            LCDbuttonbutton.onClick.RemoveAllListeners();
            LCDbuttonbutton.onClick.AddListener(new UnityEngine.Events.UnityAction(OnHostLCDuels));
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void patchOnStart(MenuManager __instance)
        {
            if (LCDuelsModBase.Instance.endOfGameResult!="")
            {
                LCDuelsModBase.Instance.mls.LogInfo("Setting reason for leaving game");
                __instance.SetLoadingScreen(false,RoomEnter.Error, LCDuelsModBase.Instance.endOfGameResult);
                LCDuelsModBase.Instance.endOfGameResult = "";
            }
        }

        [HarmonyPatch(nameof(MenuManager.ClickHostButton))]
        [HarmonyPostfix]
        static void patchClickHostButton()
        {
            LCDuelsModBase.Instance.ResetValues(false);
        }

        public static void OnHostLCDuels()
        {
            LCDuelsModBase.Instance.mls.LogInfo("OnHostClicked");
            LCDuelsModBase.Instance.ResetValues(true);
            GameNetworkManager.Instance.lobbyHostSettings = new HostSettings("LCDuels game", false);
            GameNetworkManager.Instance.StartHost();
        }
    }
}
