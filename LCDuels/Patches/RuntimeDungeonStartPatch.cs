using DunGen;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(RuntimeDungeon))]
    internal class RuntimeDungeonStartPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void StartPatch()
        {
            LCDuelsModBase.Instance.mls.LogInfo("Runtime dungeon start!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }
    }
}
