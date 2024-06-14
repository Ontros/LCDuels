using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void patchStart(ref int ___groupCredits)
        {
            Random lmfao = new Random(69420);
            ___groupCredits = lmfao.Next(100, 1000);
        }
    }
}
