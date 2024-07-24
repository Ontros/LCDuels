using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.AI;
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
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Postfix(PlayerControllerB __instance)
        {
        }

        [HarmonyPatch(nameof(PlayerControllerB.SetItemInElevator))]
        [HarmonyPostfix]
        static void patchSetItemInElevatorPatch()
        {
            if (LCDuelsModBase.playing)
            {
                LCDuelsModBase.Instance.currentValue = RoundManager.Instance.scrapCollectedInLevel;
                LCDuelsModBase.Instance.mls.LogInfo("Sending score "+RoundManager.Instance.scrapCollectedInLevel);
                _ = LCDuelsModBase.Instance.SendMessage(new { type="score",value= RoundManager.Instance.scrapCollectedInLevel.ToString() });
                LCDuelsModBase.Instance.UpdateInGameStatusText();
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
            LCDuelsModBase.Instance.death = true;
            LCDuelsModBase.Instance.WaitingForResult();
            _ = LCDuelsModBase.Instance.SendMessage(new { type = "death" });
        }
    }
}
