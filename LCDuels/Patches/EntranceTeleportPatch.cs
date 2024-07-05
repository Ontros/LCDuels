using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(EntranceTeleport))]
    internal class EntranceTeleportPatch
    {
        [HarmonyPatch(nameof(EntranceTeleport.TeleportPlayer))]
        [HarmonyPostfix]
        static void patchStart(EntranceTeleport __instance)
        {
            if (LCDuelsModBase.playing)
            {
                if (__instance.isEntranceToBuilding ) 
                {
                    _ = LCDuelsModBase.Instance.SendMessage(new { type= "position", value= "2"});
                }
                else
                {
                    _ =LCDuelsModBase.Instance.SendMessage(new { type= "position", value= "1"});
                }
            }
        }
    }
}
