using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

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
            if (HUDManager.Instance.chatTextField.text == "f")
            {
                GameNetworkManager.Instance.localPlayerController.StartCoroutine(LCDuelsModBase.Instance.StartLooting());
            }
            if (LCDuelsModBase.playing)
            {
                if (GameNetworkManager.Instance.localPlayerController == null || !GameNetworkManager.Instance.localPlayerController.isTypingChat)
                {
                    return true;
                }
                HUDManager hu = HUDManager.Instance;
                if (!string.IsNullOrEmpty(hu.chatTextField.text) && hu.chatTextField.text.Length < 50)
                {
                    hu.AddTextToChatOnServer(hu.chatTextField.text, (int)hu.localPlayer.playerClientId);
                }
                _ = LCDuelsModBase.Instance.SendMessage(new {type="chat", value=hu.chatTextField.text});
                hu.localPlayer.isTypingChat = false;
                hu.chatTextField.text = "";
                EventSystem.current.SetSelectedGameObject(null);
                hu.PingHUDElement(hu.Chat, 2f, 1f, 0.2f);
                hu.typingIndicator.enabled = false;
                return false;
            }
            return true;
        }
    }
}
