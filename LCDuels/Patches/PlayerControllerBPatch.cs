using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch(nameof(PlayerControllerB.SetItemInElevator))]
        [HarmonyPrefix]
        static void patchSetItemInElevatorPatch()
        {
            if (LCDuelsModBase.playing)
            {
                LCDuelsModBase.Instance.mls.LogInfo("Sending score "+RoundManager.Instance.scrapCollectedInLevel);
                _ = LCDuelsModBase.Instance.SendMessage(new { type="score",value= RoundManager.Instance.scrapCollectedInLevel.ToString() });
            }
        }
        //[HarmonyPatch(nameof(PlayerControllerB.Update))] if public member
        [HarmonyPatch("UpdatePlayerPositionClientRpc")]
        [HarmonyPostfix]
        static void patchUpate(PlayerControllerB __instance)
        {
            if (__instance.isInHangarShipRoom != LCDuelsModBase.Instance.isInShip && LCDuelsModBase.playing &&!__instance.playersManager.hangarDoorsClosed)
            {
                LCDuelsModBase.Instance.isInShip = __instance.isInHangarShipRoom;
                if (__instance.isInHangarShipRoom ) 
                {
                    _ = LCDuelsModBase.Instance.SendMessage(new { type= "position", value= "0"});
                }
                else
                {
                    _ =LCDuelsModBase.Instance.SendMessage(new { type= "position", value= "1"});
                }
            }
        }
        [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
        [HarmonyPostfix]
        static void patchKillPlayer()
        {
            LCDuelsModBase.Instance.mls.LogInfo("Sending death info");
            _ = LCDuelsModBase.Instance.SendMessage(new { type = "death" });
        }
    }
}
