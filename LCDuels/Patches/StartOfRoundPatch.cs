using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
                LCDuelsModBase.Instance.mls.LogInfo("patchChooseNewRandomMapSeed called");
                if (LCDuelsModBase.Instance.gameMode != 3)
                {
                    StartOfRound.Instance.ChangeLevel(LCDuelsModBase.Instance.foundMapID);
                }
                ___randomMapSeed = LCDuelsModBase.Instance.seedFromServer;
                StartOfRound.Instance.SetMapScreenInfoToCurrentLevel();
            }
        }

        [HarmonyPatch(nameof(StartOfRound.StartGame))]
        [HarmonyPrefix]
        static void patchtest1()
        {
            LCDuelsModBase.Instance.mls.LogInfo("startgame called");
        }
        [HarmonyPatch(nameof(StartOfRound.StartGame))]
        [HarmonyPostfix]
        static void patchtest2()
        {
            LCDuelsModBase.Instance.mls.LogInfo("startgame finished");
        }

        [HarmonyPatch("PlayFirstDayShipAnimation")]
        [HarmonyPrefix]
        static bool patchPlayFirstDayShipAnimation()
        {
            return !LCDuelsModBase.playing;
        }

        [HarmonyPatch(nameof(StartOfRound.FirePlayersAfterDeadlineClientRpc))]
        [HarmonyPostfix]
        static void patchEject()
        {
            if (LCDuelsModBase.playing)
            {
                LCDuelsModBase.Instance.ejected = true;
                LCDuelsModBase.Instance.waitingForResult = true;
                _ = LCDuelsModBase.Instance.SendMessage(new { type = "eject", curDay = LCDuelsModBase.Instance.curDay });
            }
        }

        [HarmonyPatch("PassTimeToNextDay")]
        [HarmonyPostfix]
        static void patchPassTimeToNextDay()
        {
            if (LCDuelsModBase.playing)
            {
                LCDuelsModBase.Instance.mls.LogInfo("Sending liftoff");
                if (LCDuelsModBase.Instance.gameMode != 3)
                {
                    LCDuelsModBase.Instance.WaitingForResult();
                }
                LCDuelsModBase.Instance.curDay++;
                LCDuelsModBase.Instance.UpdateInGameStatusText();
                _ = LCDuelsModBase.Instance.SendMessage(new { type = "liftoff", curDay = LCDuelsModBase.Instance.curDay });
            }
        }
    }
}
