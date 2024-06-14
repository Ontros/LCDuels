using GameNetcodeStuff;
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
            Random ra = new Random(LCDuelsModBase.Instance.seedFromServer);
            StartOfRound.Instance.ChangeLevel(ra.Next(0, StartOfRound.Instance.levels.Length));
            ___randomMapSeed = LCDuelsModBase.Instance.seedFromServer;
            //StartOfRound.Instance.SetPlanetsWeather();
            StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
        }
    }
}
