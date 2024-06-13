using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        //[HarmonyPatch("SubmitChat_performed")]
        //[HarmonyPrefix]
        //static void patchSubmitChat_performed()
        //{
        //    Vector3 mainPos = RoundManager.FindMainEntrancePosition(false, false);
        //    string[] outputs = new string[4];
        //    EntranceTeleport[] array = UnityEngine.Object.FindObjectsOfType<EntranceTeleport>();
        //    foreach (EntranceTeleport entranceTeleport in array)
        //    {
        //        if (!entranceTeleport.isEntranceToBuilding)
        //        {
        //            outputs[entranceTeleport.entranceId] = Vector3.Distance(mainPos, entranceTeleport.transform.position).ToString();
        //        }
        //    }
        //    UnityEngine.Debug.Log(outputs[1]+ ","+ outputs[2] + "," + outputs[3]);
        //}
    }
}
