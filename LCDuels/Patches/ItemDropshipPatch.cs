using DunGen;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(ItemDropship))]
    internal class ItemDropShipPatch
    {
        [HarmonyPatch("LandShipOnServer")]
        [HarmonyPrefix]
        static bool StartPatch()
        {
            return !LCDuelsModBase.playing;
        }
    }
}
