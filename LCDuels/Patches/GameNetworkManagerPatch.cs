using DunGen;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        [HarmonyPatch(nameof(GameNetworkManager.SaveGame))]
        [HarmonyPrefix]
        static bool SaveGamePatch()
        {
            if (LCDuelsModBase.Instance.waitingForResult)
            {
                LCDuelsModBase.Instance.WaitingForResult();
            }
            return !LCDuelsModBase.playing;
        }

        [HarmonyPatch(nameof(GameNetworkManager.JoinLobby))]
        [HarmonyPrefix]
        static void PatchJoinLobby()
        {
            LCDuelsModBase.Instance.ResetValues(false);
        }

        [HarmonyPatch("DisconnectProcess")]
        [HarmonyPostfix]
        static void PatchDisconnectProcess()
        {
            LCDuelsModBase.Instance.DisconnectDone = true;
        }

        [HarmonyPatch(nameof(GameNetworkManager.Disconnect))]
        [HarmonyPrefix]
        static void PatchDisconnect()
        {
            if (LCDuelsModBase.playing)
            {
                _ = LCDuelsModBase.Instance.SendMessage(new { type = "leave" });
            }
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void PatchStart(GameNetworkManager __instance) {
            if (LCDuelsModBase.playing)
            {
                LCDuelsModBase.Instance.preventSaveLoading();
            }
        }
    }
}
