using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LCDuels.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        public static TextMeshPro inGameStatusText;

        public static bool playing = true;

        public int getRandomMapID()
        {
            System.Random ra = new System.Random(seedFromServer);
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

        public void CreateInGameStatusText()
        {
            HUDManager.Instance.controlTipLines[0].text = "TEST";
            GameObject inGameTextGO = new GameObject("inGameStatusTextGO");
            CanvasGroup canvas = HUDManager.Instance.Tooltips.canvasGroup;
            inGameTextGO.transform.SetParent(canvas.transform);
            inGameStatusText = inGameTextGO.AddComponent<TextMeshPro>();
            inGameStatusText.text = "TEST";
            //inGameStatusText.alignment = TextAlignmentOptions.Center;
            ////inGameStatusText.font = Resources.GetBuiltinResource<TMP_FontAsset>("Arial.ttf");
            //inGameStatusText.color = Color.white;
            //inGameStatusText.fontSize = 36;
            //RectTransform textRectTransform = inGameStatusText.GetComponent<RectTransform>();
            //RectTransform rectTransform = inGameTextGO.GetComponent<RectTransform>();
            //rectTransform.sizeDelta = new Vector2(600, 200); // Width, Height
            //rectTransform.anchoredPosition = new Vector2(0, 0); // Position
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
