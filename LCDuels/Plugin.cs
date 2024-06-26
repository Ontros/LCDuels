using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LCDuels.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using DunGen;
using System.Collections;

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

        public string enemyPlayerName = "Unknown";

        public string enemyPlayerScrap = "Equal loot amount"; //0 same, 1 less, 2 more

        public string enemyPlayerLocation = "in ship"; //0 ship, 1 outisde, 2 inside 

        public bool gameReady = false;

        ClientWebSocket localWS = null;

        public Terminal terminal = null;

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

        public void UpdateInGameStatusText()
        {
            HUDManager.Instance.controlTipLines[0].text = "VS: "+enemyPlayerName;
            HUDManager.Instance.controlTipLines[1].text = "You have "+enemyPlayerScrap;
            HUDManager.Instance.controlTipLines[2].text = "He is "+enemyPlayerLocation;
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
            harmony.PatchAll(typeof(StartMatchLeverPatch));

        }

        public async Task InitWS()
        {
            Uri serverUri = new Uri("ws://localhost:8080");
            using (ClientWebSocket webSocket = new ClientWebSocket())
            {
                localWS = webSocket;
                await webSocket.ConnectAsync(serverUri, CancellationToken.None);
                mls.LogInfo("Connected to server");

                _ = Register(webSocket, "yourSteamId", "yourSteamUsername");
                // Start receiving messages
                await ReceiveMessages(webSocket);
            }
        }

        async Task ReceiveMessages(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[1024];

            while (webSocket.State == WebSocketState.Open)
            {
                mls.LogInfo("Starting while loop");
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                HandleMessage(message);
            }
            mls.LogInfo("Socket closed");
        }

        void HandleMessage(string message)
        {
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
            if (data == null || !data.ContainsKey("type"))
            {
                mls.LogInfo("Invalid message received");
                return;
            }

            string messageType = data["type"].ToString();
            switch (messageType)
            {
                case "match_found":
                    string opponentId = data["opponentId"].ToString();
                    enemyPlayerName = data["opponentUsername"].ToString();
                    seedFromServer = int.Parse(data["seed"].ToString());
                    mls.LogInfo($"Match found! Opponent: {enemyPlayerName} (ID: {opponentId})");
                    System.Random lmfao = new System.Random(seedFromServer);
                    terminal.groupCredits = lmfao.Next(100, 1000);
                    StartMatchLever matchLever = UnityEngine.Object.FindFirstObjectByType<StartMatchLever>();
                    matchLever.triggerScript.disabledHoverTip = "[Pull to get ready]";
                    matchLever.triggerScript.interactable = true;
                    StartOfRound.Instance.randomMapSeed = seedFromServer;
                    StartOfRound.Instance.ChangeLevel(getRandomMapID());
                    StartOfRound.Instance.SetPlanetsWeather();
                    UpdateInGameStatusText();
                    string text;
                    if (StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.None)
                    {
                        text = "Weather: " + StartOfRound.Instance.currentLevel.currentWeather.ToString();
                    }
                    else
                    {
                        text = "";
                    }
                    string levelDescription = StartOfRound.Instance.currentLevel.LevelDescription;
                    StartOfRound.Instance.screenLevelDescription.text = string.Concat(new string[]
                    {
                        "Orbiting: ",
                        StartOfRound.Instance.currentLevel.PlanetName,
                        "\n",
                        levelDescription,
                        "\n",
                        text
                    });
                    //mls.LogInfo("Before task");
                    //Task.Run(() => {
                    //    mls.LogInfo("Inside task");
                    //    StartCoroutine(updateTheScreen());
                    //});
                    //    mls.LogInfo("After task");
                    break;

                case "game_start":
                    mls.LogInfo("Game started!");
                    gameReady = true;
                    _ = HandleGameStart();
                    break;

                case "position":
                    string position = data["value"].ToString();
                    switch (position)
                    {
                        case "0":
                            enemyPlayerLocation = "in ship";
                            break;
                        case "1":
                            enemyPlayerLocation = "outside";
                            break;
                        case "2":
                            enemyPlayerLocation = "inside";
                            break;
                        default:
                            enemyPlayerLocation = "data error";
                            break;

                    }
                    mls.LogInfo($"Opponent position: {enemyPlayerLocation}");
                    UpdateInGameStatusText();
                    break;

                case "score":
                    string scoreComparasion = data["value"].ToString();
                    switch (scoreComparasion)
                    {
                        case "0":
                            enemyPlayerScrap = "equal loot amount";
                            break;
                        case "1":
                            enemyPlayerScrap = "less loot";
                            break;
                        case "2":
                            enemyPlayerScrap = "more loot";
                            break;
                        default:
                            enemyPlayerScrap = "data error";
                            break;

                    }
                    mls.LogInfo($"Opponent score is {enemyPlayerScrap}");
                    UpdateInGameStatusText();
                    break;

                case "opponent_left":
                    mls.LogInfo("Your opponent has left the game.");
                    break;

                default:
                    mls.LogInfo($"Unknown message type: {messageType}");
                    break;
            }
            mls.LogInfo("Received: " + message);
            // Parse and handle the message as needed
        }

        async Task HandleGameStart()
        {
            mls.LogInfo("Handling game start");
            await Task.Run(async () =>
            {
                ////It get stuck somewhere here idk why pls help

                //StartOfRound.Instance.StartGame();
                //matchLever.playersManager.StartGame();
                mls.LogInfo("LOADING GAME1");
                StartMatchLever matchLever = UnityEngine.Object.FindFirstObjectByType<StartMatchLever>();
                mls.LogInfo("LOADING GAME2");
                GameNetworkManager.Instance.gameHasStarted = true;
                mls.LogInfo("LOADING GAME3");
                StartOfRound.Instance.inShipPhase = false;
                mls.LogInfo("LOADING GAME4");
                StartOfRound.Instance.fullyLoadedPlayers.Clear();
                mls.LogInfo("LOADING GAME5");
                matchLever.triggerScript.disabledHoverTip = "[Wait for ship to land]";
                mls.LogInfo("LOADING GAME6");
                StartOfRound.Instance.currentPlanetAnimator.SetTrigger("LandOnPlanet");
                mls.LogInfo("LOADING GAME7" + StartOfRound.Instance.currentLevel.name);
                StartOfRound.Instance.NetworkManager.SceneManager.LoadScene(StartOfRound.Instance.currentLevel.name, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                //mls.LogInfo("LOADING GAME8");
                //RuntimeDungeon RD = UnityEngine.Object.FindFirstObjectByType<RuntimeDungeon>();
                ////mls.LogInfo(UnityEngine.Object.FindFirstObjectByType<RuntimeDungeon>().ToString());
                //if (!RD)
                //{
                //    mls.LogInfo("No RD");

                //}
                //RoundManager.Instance.GenerateNewLevelClientRpc(seedFromServer, StartOfRound.Instance.currentLevel.levelID);
                await Task.Delay(500);
                mls.LogInfo("LOADING GAME8");
                RoundManager.Instance.LoadNewLevel(seedFromServer, StartOfRound.Instance.currentLevel);
            });
            mls.LogInfo("Handled game start");
        }

        async Task Register(ClientWebSocket webSocket, string steamId, string steamUsername)
        {
            var message = new
            {
                type = "register",
                steamId,
                steamUsername
            };
            await SendMessage(message);
        }

        public async Task SendMessage(object message)
        {
            string jsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonMessage);
            await localWS.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            mls.LogInfo("Sent message");
        }
    }
}
