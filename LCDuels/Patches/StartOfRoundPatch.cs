﻿using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPatch(nameof(StartOfRound.ChooseNewRandomMapSeed))]
        [HarmonyPostfix]
        static void patchChooseNewRandomMapSeed(ref float ___randomMapSeed)
        {
            if (LCDuelsModBase.playing)
            {
                StartOfRound.Instance.ChangeLevel(LCDuelsModBase.Instance.getRandomMapID());
                ___randomMapSeed = LCDuelsModBase.Instance.seedFromServer;
                StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
            }
        }
    }
}
