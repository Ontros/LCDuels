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
using System.Net.Http.Headers;
using Steamworks;

namespace LCDuels
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class LCDuelsModBase : BaseUnityPlugin
    {
        private const string modGUID = "onty.duels";
        private const string modName = "LCDuels";
        private const string modVersion = "1.0.0.1";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static LCDuelsModBase Instance;

        internal ManualLogSource mls;
        
        public int seedFromServer = 64;

        public static TextMeshPro inGameStatusText;

        public static bool playing = true;

        public string enemyPlayerName = "Waiting";

        public string enemyPlayerScrap = "0"; //0 same, 1 less, 2 more

        public string enemyPlayerLocation = "in ship"; //0 ship, 1 outisde, 2 inside 

        public bool gameReady = false;

        public bool isInShip = true;

        public bool wsTerminated = false;

        public string endOfGameResult = "";

        public int currentValue = 0;

        public bool gameStarted = false;

        public bool DisconnectDone = false;

        public bool waitingForResult = false;

        public bool death = false;

        ClientWebSocket localWS = null;
        public Terminal terminal = null;
        public StartMatchLever matchLever = null;
        public MenuManager menuManager = null;

        public void ResetValues(bool isEnabled)
        {
            endOfGameResult = "";
            seedFromServer = 64;
            playing = isEnabled;
            enemyPlayerName = "Unknown";
            enemyPlayerScrap = "0";
            enemyPlayerLocation = "in ship";
            gameReady = false;
            isInShip = true;
            wsTerminated = false;
            currentValue = 0;
            gameStarted = false;
            waitingForResult = false;
            death = false;  
        }

        public int getRandomMapID()
        {
            int i = 0;
            foreach (SelectableLevel selectableLevel in StartOfRound.Instance.levels)
            {
                mls.LogInfo(i.ToString() +"-"+ selectableLevel.PlanetName);
                i++;
            }
            System.Random ra = new System.Random(seedFromServer);
            int output = ra.Next(0, StartOfRound.Instance.levels.Length-1);
            if (output < 3)
            {
                return output;
            }
            else if (output < 10)
            {
                return output + 1;
            }
            else
            {
                return output + 2;
            }
        }

        public void UpdateInGameStatusText()
        {
            HUDManager.Instance.controlTipLines[0].text = "Join dc: dc.ontro.cz";
            HUDManager.Instance.controlTipLines[1].text = "VS: "+enemyPlayerName;
            HUDManager.Instance.controlTipLines[2].text = "Your loot "+currentValue;
            HUDManager.Instance.controlTipLines[3].text = "He is " + enemyPlayerLocation;
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
            harmony.PatchAll(typeof(StartMatchLeverPatch));
            harmony.PatchAll(typeof(RuntimeDungeonStartPatch));
            harmony.PatchAll(typeof(GameNetworkManagerPatch));
            harmony.PatchAll(typeof(MenuManagerPatch));
            harmony.PatchAll(typeof(EntranceTeleportPatch));
            harmony.PatchAll(typeof(ItemDropShipPatch));
        }

        public async Task InitWS()
        {
            mls.LogInfo("InitWebSocket");
            if (localWS == null) {
                Uri serverUri = new Uri("ws://130.61.35.90:8080");
                using (ClientWebSocket webSocket = new ClientWebSocket())
                {
                    localWS = webSocket;
                    await webSocket.ConnectAsync(serverUri, CancellationToken.None);
                    mls.LogInfo("Connected to server");
            
                    _ = Register(localWS);
                    // Start receiving messages
                    await ReceiveMessages(webSocket);
                }
            }
            else
            {
                 _ = Register(localWS);
            }
        }

        async Task ReceiveMessages(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    mls.LogInfo("Starting while loop");
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    HandleMessage(message);
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocketException: {ex.Message}");
            }
            finally
            {
                if (webSocket.State != WebSocketState.Closed)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error occurred", CancellationToken.None);
                }
                webSocket.Dispose();
                mls.LogInfo("Socket closed");
                localWS = null;
                if (GameNetworkManager.Instance.isHostingGame && !wsTerminated)
                {
                    endOfGameResult = "Lost connection to server";
                }
            }
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
                    int[] startingCash = { 0, 0, 0, 0, 0, 30, 30, 30, 60, 60, 60, 60, 300, 300, 300, 700, 700, 700, 1000, 1000 };
                    terminal.groupCredits = startingCash[lmfao.Next(0,startingCash.Length)];
                    if (lmfao.Next(0,10000) == 6942)
                    {
                        terminal.groupCredits = int.MaxValue;
                    }
                    StartMatchLever matchLever = UnityEngine.Object.FindFirstObjectByType<StartMatchLever>();
                    matchLever.triggerScript.hoverTip = "[Pull to get ready]";
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
                        "Pull the lever to get ready\nOrbiting: ",
                        StartOfRound.Instance.currentLevel.PlanetName,
                        "\n",
                        text
                    });
                    HUDManager.Instance.SetDebugText("Match found, Pull the lever to get ready");
                    break;

                case "game_start":
                    mls.LogInfo("Game started!");
                    gameReady = true;
                    break;

                case "position":
                    string position = data["value"].ToString();
                    switch (position)
                    {
                        case "0":
                            enemyPlayerLocation = waitingForResult?"in ship":"outside";
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
                    if (waitingForResult)
                    {
                        UpdateDisplayWaitingForResult();
                    }
                    break;

                case "score":
                    enemyPlayerScrap = data["value"].ToString();
                    mls.LogInfo($"Opponent score is {enemyPlayerScrap}");
                    UpdateInGameStatusText();
                    if (waitingForResult)
                    {
                        UpdateDisplayWaitingForResult();
                    }
                    break;

                case "opponent_left":
                    mls.LogInfo("Your opponent has left the game.");
                    endOfGameResult = "Won, opponent left";
                    break;

                case "won":
                    switch (data["value"])
                    {
                        case "1":
                            endOfGameResult = "Won, because other player died";
                            break;
                        case "3":
                            endOfGameResult = "Won, becase you had more loot";
                            break;
                        case "5":
                            endOfGameResult = "Won, becase you had the same amount of loot but you were faster";
                            break;
                        case "7":
                            endOfGameResult = "Won, becase other player had no balls";
                            break;
                        case "9":
                            endOfGameResult = "Won, becase you had the same amount of loot but you survived longer";
                            break;
                        default:
                            endOfGameResult = "Won, reason unknown";
                            break;
                    }
                    break;

                case "lost":
                    switch (data["value"])
                    {
                        case "2":
                            endOfGameResult = "Lost, because other player survived";
                            break;
                        case "4":
                            endOfGameResult = "Lost, becase you had less loot";
                            break;
                        case "6":
                            endOfGameResult = "Lost, becase you had the same amount of loot but you were slower";
                            break;
                        case "8":
                            endOfGameResult = "Lost, becase you had no balls";
                            break;
                        case "10":
                            endOfGameResult = "Lost, becase you had the same amount of loot but you died sooner";
                            break;
                        default:
                            endOfGameResult = "Lost, reason unknown";
                            break;
                    }
                    break;
                case "in_game_error":
                    HUDManager.Instance.SetDebugText("Something has gone wrong, please restart the game");
                    break;

                default:
                    mls.LogInfo($"Unknown message type: {messageType}");
                    break;
            }
            mls.LogInfo("Received: " + message);
        }

        public void UpdateDisplayWaitingForResult()
        {
            StartOfRound.Instance.screenLevelDescription.text = "Waiting for other player to die or get more then you or lift off\nEnemy loot: "+enemyPlayerScrap+"\nHe is: "+enemyPlayerLocation;
        }

        async Task Register(ClientWebSocket webSocket)
        {
            var message = new
            {
                type = "register",
                steamId = SteamClient.SteamId.ToString(),
                steamUsername = SteamClient.Name.ToString()
            };
            mls.LogInfo("Registering with username: " + message.steamUsername);
            await SendMessage(message);
        }

        public void WaitingForResult()
        {
            waitingForResult = true;
            matchLever.triggerScript.disabledHoverTip = "[ Waiting for end of game ]";
            matchLever.triggerScript.interactable = false;
            UpdateDisplayWaitingForResult();
        }

        public async Task SendMessage(object message)
        {
            string jsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonMessage);
            await localWS.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            mls.LogInfo("Sent message");
        }

        public IEnumerator waitUntilGameIsReady()
        {
            mls.LogInfo("Waiting until game is ready");
            yield return new WaitUntil(() => gameReady);
            mls.LogInfo("Starting game");
            matchLever.StartGame();
            gameStarted = true;
        }
        
        public IEnumerator waitUntilEndOfGame()
        {
            mls.LogInfo("Wait until end of game");
            yield return new WaitUntil(()=>endOfGameResult!="");
            mls.LogInfo("Ending game");
            yield return new WaitForSeconds(3);
            GameNetworkManager.Instance.Disconnect();
        }
    }
}
