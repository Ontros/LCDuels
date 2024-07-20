using DunGen;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
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
        [HarmonyPatch(nameof(GameNetworkManager.Disconnect))]
        [HarmonyPostfix]
        static void PatchDisconnectPost()
        {
            if (LCDuelsModBase.playing)
            {
                LCDuelsModBase.Instance.wsTerminated = true;
                _ = LCDuelsModBase.Instance.localWS.CloseAsync(WebSocketCloseStatus.NormalClosure,"Closing the connection normally", CancellationToken.None);
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
