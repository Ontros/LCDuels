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
    }
}
