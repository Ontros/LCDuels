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
            //GrabbableObject[] grabbableObjects = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
            //float closestItem = 100000f;
            //foreach (GrabbableObject grabbableObject in grabbableObjects)
            //{
            //    //UnityEngine.Debug.Log(grabbableObject.ToString()+grabbableObject.isInShipRoom);
            //    if (!grabbableObject.isInFactory)
            //    {
            //        closestItem = Mathf.Min(closestItem, (Vector3.Distance(grabbableObject.transform.position, StartOfRound.Instance.allPlayerScripts[0].transform.position)));
            //    }
            //}
            //UnityEngine.Debug.Log(closestItem.ToString());
            //if (___currentlyHeldObjectServer != null)
            //{
            //    UnityEngine.Debug.Log(Vector3.Distance(___currentlyHeldObjectServer.transform.position, StartOfRound.Instance.allPlayerScripts[0].transform.position));
            //}
        }
    }
}
