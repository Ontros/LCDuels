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
        //[HarmonyPatch(nameof(PlayerControllerB.Update))] if public member
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void patchUpate(ref GrabbableObject ___currentlyHeldObjectServer)
        {
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
