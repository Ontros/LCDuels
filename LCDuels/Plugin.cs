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
using System.Reflection;

namespace LCDuels
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class LCDuelsModBase : BaseUnityPlugin
    {
        private const string modGUID = "onty.duels";
        private const string modName = "LCDuels";
        private const string modVersion = "1.2.0.0";
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
        public string versionString = "";
        public bool gameEndedWithError = false;
        public string queueName = "";
        public static bool debug = true;
        public bool isPublicQueue = false;
        public ClientWebSocket localWS = null;
        public Terminal terminal = null;
        public StartMatchLever matchLever = null;
        public MenuManager menuManager = null;
        public TextMeshProUGUI publicText;
        public TextMeshProUGUI friendsText;
        public TextMeshProUGUI serverName;
        public int gameMode = 0; // 0-undefined, 1-BO1, 2-BO3, 3-HQ
        public int yourScore = 0;
        public int enemyScore = 0;
        public int curDay = 1;
        public bool ejected = false;
        public List<SaveFileUISlot> saveFileUISlots = new List<SaveFileUISlot>();
        public TextMeshProUGUI specialTipText;

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
            gameEndedWithError = false;
            yourScore = 0;
            enemyScore = 0;
            curDay = 1;
            ejected = false;
        }

        public int getRandomMapID(System.Random ra)
        {
            foreach (SelectableLevel selectableLevel in StartOfRound.Instance.levels)
            {
                Console.WriteLine(selectableLevel.levelID+"-"+selectableLevel.name);
            }
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
        public int getRandomMapIDByTier(int tier, System.Random ra)
        {
            List<int> list = new List<int>();
            if (tier == 1)
            {
                list.Add(0);
                list.Add(1);
                list.Add(2);
            }
            else if (tier == 2)
            {
                list.Add(4);
                list.Add(5);
                list.Add(8);
                list.Add(12);
            }
            else
            {
                list.Add(6);
                list.Add(7);
                list.Add(9);
                list.Add(10);
            }
            int output = ra.Next(0, list.Count-1);
            return list[output];
        }

        public void UpdateInGameStatusText()
        {
            switch (gameMode)
            {
                case 1:
                    HUDManager.Instance.controlTipLines[0].text = "Join dc: dc.ontro.cz";
                    HUDManager.Instance.controlTipLines[1].text = enemyPlayerName=="Unknown"?"Waiting for player":(enemyPlayerName +" is " + enemyPlayerLocation);
                    HUDManager.Instance.controlTipLines[2].text = "VS: "+enemyPlayerName;
                    HUDManager.Instance.controlTipLines[3].text = "Your loot "+currentValue;
                    break;
                case 2:
                    HUDManager.Instance.controlTipLines[0].text = "Join dc: dc.ontro.cz";
                    HUDManager.Instance.controlTipLines[1].text = enemyPlayerName=="Unknown"?"Waiting for player":(enemyPlayerName +" is " + enemyPlayerLocation);
                    HUDManager.Instance.controlTipLines[2].text = "You "+yourScore+":"+enemyScore+" "+enemyPlayerName;
                    HUDManager.Instance.controlTipLines[3].text = "Your loot from this day "+currentValue;
                    break;
                case 3:
                    HUDManager.Instance.controlTipLines[0].text = "Join dc: dc.ontro.cz";
                    HUDManager.Instance.controlTipLines[1].text = enemyPlayerName=="Unknown"?"Waiting for player":(enemyPlayerName +" is " + enemyPlayerLocation);
                    HUDManager.Instance.controlTipLines[2].text = "Your total loot "+currentValue;
                    HUDManager.Instance.controlTipLines[3].text = "Current day: "+LCDuelsModBase.Instance.curDay;
                    break;
                default:
                    HUDManager.Instance.controlTipLines[0].text = "error";
                    HUDManager.Instance.controlTipLines[1].text = "error";
                    HUDManager.Instance.controlTipLines[2].text = "error";
                    HUDManager.Instance.controlTipLines[3].text = "error";
                    break;

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
            harmony.PatchAll(typeof(StartMatchLeverPatch));
            harmony.PatchAll(typeof(RuntimeDungeonStartPatch));
            harmony.PatchAll(typeof(GameNetworkManagerPatch));
            harmony.PatchAll(typeof(MenuManagerPatch));
            harmony.PatchAll(typeof(EntranceTeleportPatch));
            harmony.PatchAll(typeof(ItemDropShipPatch));
            harmony.PatchAll(typeof(SaveFileUISlotPatch));
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
            
                    // Start receiving messages
                    await ReceiveMessages(webSocket, true);
                }
            }
            else
            {
                 _ = Register(localWS);
            }
        }

        async Task ReceiveMessages(ClientWebSocket webSocket, bool shouldRegister)
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    mls.LogInfo("Starting while loop");
                    if (shouldRegister)
                    {
                        mls.LogInfo("Registering");
                        shouldRegister = false;
                        _ = Register(localWS);
                    }
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
                    gameEndedWithError = true;
                    waitingForResult = true;
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
                    gameMode = int.Parse(data["gameMode"].ToString());
                    mls.LogInfo($"Match found! Opponent: {enemyPlayerName} (ID: {opponentId})");
                    _ = matchFound();
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
                            enemyPlayerLocation = waitingForResult ? "in ship" : "outside";
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
                    if (debug)
                    {
                        mls.LogInfo($"Opponent position: {enemyPlayerLocation}");
                    }
                    UpdateInGameStatusText();
                    if (waitingForResult)
                    {
                        UpdateDisplayWaitingForResult();
                    }
                    break;

                case "score":
                    enemyPlayerScrap = data["value"].ToString();
                    if (debug)
                    {
                        mls.LogInfo($"Opponent score is {enemyPlayerScrap}");
                    }
                    UpdateInGameStatusText();
                    if (waitingForResult)
                    {
                        UpdateDisplayWaitingForResult();
                    }
                    break;

                case "error":
                    gameEndedWithError = true;
                    waitingForResult = true;
                    endOfGameResult = data["value"].ToString();
                    break;

                case "opponent_left":
                    mls.LogInfo("Your opponent has left the game.");
                    HUDManager.Instance.DisplayTip("You won!","You can leave or finish the day. Your opponent has left the game!");
                    endOfGameResult = "Won, opponent left";
                    break;

                case "won":
                    if (gameMode == 2 && (string)data["value"] != "15")
                    {
                        HUDManager.Instance.DisplayTip("Won the day", "You can pull the lever now");
                        yourScore = int.Parse(data["yourScore"].ToString());
                        enemyScore = int.Parse(data["enemyScore"].ToString());
                        gameReady = false;
                        yourScore = int.Parse(data["yourScore"].ToString());
                        enemyScore = int.Parse(data["enemyScore"].ToString());
                        terminal.groupCredits = 0;
                        GameNetworkManager.Instance.SetLobbyJoinable(false);
                        GameNetworkManager.Instance.disallowConnection = true;
                        StartOfRound.Instance.screenLevelDescription.text = "Waiting for other player\nYou can join our discord (dc.ontro.cz) to find people to play with.";
                        if (matchLever != null)
                        {
                            matchLever.triggerScript.disabledHoverTip = "[Wait for the other player]";
                            matchLever.triggerScript.interactable = false;

                        }
                        LCDuelsModBase.Instance.UpdateInGameStatusText();
                        terminal.StartCoroutine(LCDuelsModBase.Instance.waitUntilEndOfGame());
                    }
                    else
                    {
                    HUDManager.Instance.DisplayTip("You won!","You can leave or finish the day");
                    switch (data["value"])
                    {
                        case "1":
                            endOfGameResult = "Won, because other player died";
                            break;
                        case "3":
                            endOfGameResult = "Won, because you had more loot";
                            break;
                        case "5":
                            endOfGameResult = "Won, because you had the same amount of loot but you were faster";
                            break;
                        case "7":
                            endOfGameResult = "Won, because other player had no balls";
                            break;
                        case "9":
                            endOfGameResult = "Won, because you had the same amount of loot but you survived longer";
                            break;
                            case "11":
                                endOfGameResult = "Won, because you survived more days";
                                break;
                            case "13":
                                endOfGameResult = "Won, because you survived the same amount of days but you had more loot on ship";
                                break;
                            case "15":
                                endOfGameResult = "Won, because you got 2 points first";
                                break;
                        default:
                            endOfGameResult = "Won, reason unknown";
                            break;
                    }
                    }
                    break;

                case "lost":
                    if (gameMode == 2 && (string)data["value"] != "15")
                    {
                        HUDManager.Instance.DisplayTip("Lost the day", "You can pull the lever now");
                        yourScore = int.Parse(data["yourScore"].ToString());
                        enemyScore = int.Parse(data["enemyScore"].ToString());
                        gameReady = false;
                        terminal.groupCredits = 0;
                        GameNetworkManager.Instance.SetLobbyJoinable(false);
                        GameNetworkManager.Instance.disallowConnection = true;
                        StartOfRound.Instance.screenLevelDescription.text = "Waiting for other player\nYou can join our discord (dc.ontro.cz) to find people to play with.";
                        if (matchLever != null)
                        {
                            matchLever.triggerScript.disabledHoverTip = "[Wait for the other player]";
                            matchLever.triggerScript.interactable = false;

                        }
                        LCDuelsModBase.Instance.UpdateInGameStatusText();
                        terminal.StartCoroutine(LCDuelsModBase.Instance.waitUntilEndOfGame());
                    }
                    else
                    {
                    HUDManager.Instance.DisplayTip("You lost!","You can leave or finish the day");
                    switch (data["value"])
                    {
                        case "2":
                            endOfGameResult = "Lost, because other player survived";
                            break;
                        case "4":
                            endOfGameResult = "Lost, because you had less loot";
                            break;
                        case "6":
                            endOfGameResult = "Lost, because you had the same amount of loot but you were slower";
                            break;
                        case "8":
                            endOfGameResult = "Lost, because you had no balls";
                            break;
                        case "10":
                            endOfGameResult = "Lost, because you had the same amount of loot but you died sooner";
                            break;
                            case "12":
                                endOfGameResult = "Lost, because you survived less days";
                                break;
                            case "14":
                                endOfGameResult = "Lost, because you survived the same amount of days but you had less loot on ship";
                                break;
                            case "16":
                                endOfGameResult = "Lost, because the enemy got 2 points first";
                                break;
                        default:
                            endOfGameResult = "Lost, reason unknown";
                            break;
                    }
                    }
                    break;
                case "in_game_error":
                    gameEndedWithError = true;
                    waitingForResult = true;
                    endOfGameResult = "Something went wrong, please restart the game!";
                    break;
                case "chat":
                    var targetMethod = typeof(HUDManager).GetMethod("AddChatMessage", BindingFlags.NonPublic | BindingFlags.Instance);

                    targetMethod.Invoke(HUDManager.Instance, new object[] { data["value"], enemyPlayerName });
                    HUDManager.Instance.PingHUDElement(HUDManager.Instance.Chat, 2f, 1f, 0.2f);
                    break;
                case "new_seed":
                    seedFromServer = int.Parse(data["seed"].ToString());
                    StartOfRound.Instance.randomMapSeed = seedFromServer;
                    break;

                default:
                    if (debug)
                    {
                        mls.LogInfo($"Unknown message type: {messageType}");
                    }
                    else
                    {
                        mls.LogInfo("Unknown message received");
                    }
                    break;
            }
            if (debug)
            {
                mls.LogInfo("Received: " + message);
            }
            else
            {
                mls.LogInfo("Received message");
            }
        }

        public void UpdateDisplayWaitingForResult()
        {
            StartOfRound.Instance.screenLevelDescription.text = "Waiting for other player to die or get more then you or lift off\nEnemy loot: "+enemyPlayerScrap+"\nHe is: "+enemyPlayerLocation;
        }

        async Task Register(ClientWebSocket webSocket)
        {
            mls.LogInfo(versionString+"version");
            if (string.IsNullOrEmpty(menuManager.lobbyNameInputField.text)) {
                queueName = "";
            }
            var message = new
            {
                type = "register",
                steamId = SteamClient.SteamId.ToString(),
                steamUsername = SteamClient.Name.ToString(),
                queueName,                
                version = versionString,
                modVersion,
                gameMode
            };
            mls.LogInfo("Registering with username: " + message.steamUsername + "-"+queueName);
            await SendMessage(message);
        }

        //This is called on death/liftoff
        public void WaitingForResult()
        {
            if (gameMode == 1 || gameMode == 2 || (gameMode == 3 && ejected))
            {
                waitingForResult = true;
                if (gameMode == 2 && curDay < 3)
                {
                    matchLever.triggerScript.disabledHoverTip = "[ Waiting for end of day ]";
                }
                else
                {
                    matchLever.triggerScript.disabledHoverTip = "[ Waiting for end of game ]";

                }
                matchLever.triggerScript.interactable = false;
                UpdateDisplayWaitingForResult();
            }
        }

        public async Task SendMessage(object message)
        {
            string jsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonMessage);
            await localWS.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            mls.LogInfo("Sent message");
            if (debug)
            {
                mls.LogInfo("message"+jsonMessage);
            }
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
            yield return new WaitUntil(()=>endOfGameResult!=""&&waitingForResult);
            mls.LogInfo("Ending game");
            yield return new WaitForSeconds(3);
            GameNetworkManager.Instance.Disconnect();
        }

        async Task matchFound()
        {
                    matchLever = UnityEngine.Object.FindFirstObjectByType<StartMatchLever>();
                    matchLever.triggerScript.hoverTip = "[Pull to get ready]";
                    matchLever.triggerScript.interactable = true;
                    StartOfRound.Instance.randomMapSeed = seedFromServer;
                    if (gameMode != 3)
                    {
                        System.Random lmfao = new System.Random(seedFromServer);
                        int[] startingCash = { 0, 0, 0, 0, 0, 30, 30, 30, 60, 60, 60, 60, 300, 300, 300, 900, 900, 900, 1500, 1500 };
                        terminal.groupCredits += startingCash[lmfao.Next(0, startingCash.Length)];
                        if (lmfao.Next(0, 10000) == 6942)
                        {
                            terminal.groupCredits = int.MaxValue;
                        }
                if (gameMode == 1)
                {
                        StartOfRound.Instance.ChangeLevel(getRandomMapID(lmfao));
                }
                else
                {
                    StartOfRound.Instance.ChangeLevel(getRandomMapIDByTier(1 + enemyScore + yourScore, lmfao));
                }
                        StartOfRound.Instance.SetPlanetsWeather();
                    }
                    UpdateInGameStatusText();
                    string text = "";
                    if (gameMode == 1)
                    {
                            text = "Best of 1\n";
                    }
                    else if (gameMode == 2)
                    {
                            text = "Best of 3\n";

                    }
                    else if (gameMode == 3)
                    {
                            text = "High quota\n";

                    }
                    if (StartOfRound.Instance.currentLevel.currentWeather != LevelWeatherType.None)
                    {
                        text += "Weather: " + StartOfRound.Instance.currentLevel.currentWeather.ToString();
                    }
                    StartOfRound.Instance.screenLevelDescription.text = string.Concat(new string[]
                    {
                        "Pull the lever to get ready\nOrbiting: ",
                        StartOfRound.Instance.currentLevel.PlanetName,
                        "\n",
                        text
                    });
                    if (enemyScore == 0 && yourScore == 0)
                    {
                        HUDManager.Instance.DisplayTip("Match found","Pull the lever to get ready");
                    }
                    if (gameMode == 2)
            {
            GrabbableObject[] grabbableObjects = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
            foreach (GrabbableObject grabbableObject in grabbableObjects)
            {
                grabbableObject.SetScrapValue(0);
            }
            }
        }

        public void preventSaveLoading()
        {
            LCDuelsModBase.Instance.mls.LogInfo("Preventing loading a save");
            GameNetworkManager.Instance.currentSaveFileName = "PleaseDontCheatThx";
            GameNetworkManager.Instance.saveFileNum = 69;
        }
        public void allowSaveLoading()
        {
            LCDuelsModBase.Instance.mls.LogInfo("Allowing loading a save");
            GameNetworkManager.Instance.currentSaveFileName = "LCSaveFile1";
            GameNetworkManager.Instance.saveFileNum = 1;
        }
    }
}
