using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LCDuels.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDuels
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class LCDuelsModBase : BaseUnityPlugin
    {
        private const string modGUID = "onty.duels";
        private const string modName = "LCDuels";
        private const string modVersion = "0.1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static LCDuelsModBase Instance;

        internal ManualLogSource mls;
        
        public int seedFromServer;

        public int getRandomMapID()
        {
                Random ra = new Random(seedFromServer);
            int output = ra.Next(0, StartOfRound.Instance.levels.Length-1);
            if (output < 3)
            {
                return output;
            }
            else
            {
                return output + 1;
            }
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("LC Duels has been awaken");

            harmony.PatchAll(typeof(LCDuelsModBase));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(RoundManagerPatch));
            harmony.PatchAll(typeof(TerminalPatch));
            harmony.PatchAll(typeof(HUDManagerPatch));
            harmony.PatchAll(typeof(StormyWeatherPatch));
        }
    }
}
