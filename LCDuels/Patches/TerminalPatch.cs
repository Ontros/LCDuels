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
            Random random = new Random(69);
            ___groupCredits = random.Next(100, 1000);
        }
    }
}
