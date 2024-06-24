using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        [HarmonyPatch(nameof(RoundManager.YRotationThatFacesTheFarthestFromPosition))]
        [HarmonyPrefix]
        static void prefixYRotationThatFacesTheFarthestFromPosition(ref bool ___hasInitializedLevelRandomSeed)
        {
            if (LCDuelsModBase.playing)
            {
                ___hasInitializedLevelRandomSeed = false;
            }
        }

        [HarmonyPatch(nameof(RoundManager.YRotationThatFacesTheFarthestFromPosition))]
        [HarmonyPostfix]
        static void postfixYRotationThatFacesTheFarthestFromPosition(ref bool ___hasInitializedLevelRandomSeed)
        {
            if (LCDuelsModBase.playing)
            {
                ___hasInitializedLevelRandomSeed = true;
            }
        }

        [HarmonyPatch(nameof(RoundManager.YRotationThatFacesTheNearestFromPosition))]
        [HarmonyPrefix]
        static void prefixYRotationThatFacesTheNearestFromPosition(ref bool ___hasInitializedLevelRandomSeed)
        {
            if (LCDuelsModBase.playing)
            {
                ___hasInitializedLevelRandomSeed = false;
            }
        }

        [HarmonyPatch(nameof(RoundManager.YRotationThatFacesTheNearestFromPosition))]
        [HarmonyPostfix]
        static void postfixYRotationThatFacesTheNearestFromPosition(ref bool ___hasInitializedLevelRandomSeed)
        {
            if (LCDuelsModBase.playing)
            {
                ___hasInitializedLevelRandomSeed = true;
            }
        }
    }
}
