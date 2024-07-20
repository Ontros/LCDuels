using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuManagerPatch
    {
        static Button LCDbuttonbutton;
        [HarmonyPatch("OnEnable")]
        [HarmonyPostfix]
        static void patchOnEnable(MenuManager __instance)
        {
            foreach (TextMeshProUGUI textMeshProUGUI in __instance.HostSettingsOptionsNormal.GetComponentsInChildren<TextMeshProUGUI>())
            {
                Debug.Log(textMeshProUGUI.name + "="+textMeshProUGUI.text);
            }
            PrintChildren(__instance.HostSettingsOptionsNormal);
            if (!GameNetworkManager.Instance.disableSteam)
            {
                if (LCDbuttonbutton ==  null)
                {
                    foreach (TextMeshProUGUI publicButton in __instance.HostSettingsOptionsNormal.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (publicButton.text == "Public")
                        {
                            LCDuelsModBase.Instance.publicText = publicButton;
                        }
                        else if (publicButton.text == "Friends-only")
                        {
                            LCDuelsModBase.Instance.friendsText = publicButton;
                        }
                        else if (publicButton.text == "Server name:")
                        {
                            LCDuelsModBase.Instance.serverName = publicButton;
                        }
                    }
                    LCDuelsModBase.Instance.versionString = __instance.versionNumberText.text;
                    Button hostButton = Traverse.Create(__instance).Field("startHostButton").GetValue() as Button;
                    LCDuelsModBase.Instance.mls.LogInfo(hostButton.ToString());
                    LCDuelsModBase.Instance.menuManager = __instance;
                    GameObject LCDButton = UnityEngine.Object.Instantiate(hostButton.gameObject);
                    LCDButton.transform.SetParent(hostButton.transform.parent.GetComponent<RectTransform>(),false);
                    LCDButton.transform.localPosition = hostButton.transform.localPosition + (hostButton.transform.localPosition - __instance.joinCrewButtonContainer.transform.localPosition);
                    LCDButton.transform.localRotation = hostButton.transform.localRotation;
                    LCDButton.transform.localScale = hostButton.transform.localScale;
                    LCDButton.GetComponentInChildren<TextMeshProUGUI>().text = "> Play LC Duels";
                    LCDbuttonbutton = LCDButton.GetComponent<Button>();
                    LCDbuttonbutton.onClick.RemoveAllListeners();
                    LCDbuttonbutton.onClick.AddListener(new UnityEngine.Events.UnityAction(OnPlayLCDuelsMenuOpen));
                }
            }
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void patchOnStart(MenuManager __instance)
        {
            if (LCDuelsModBase.Instance.endOfGameResult!="")
            {
                LCDuelsModBase.Instance.mls.LogInfo("Setting reason for leaving game");
                __instance.SetLoadingScreen(false, RoomEnter.Error, LCDuelsModBase.Instance.endOfGameResult + (!LCDuelsModBase.Instance.gameEndedWithError ? 
                    ("\nLoot values:\n" + LCDuelsModBase.Instance.currentValue + " vs " + LCDuelsModBase.Instance.enemyPlayerScrap) :""));
                LCDuelsModBase.Instance.endOfGameResult = "";
            }
        }
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void patchOnUpdate(MenuManager __instance)
        {
            if (LCDuelsModBase.playing)
            {
                __instance.lobbyTagInputField.gameObject.SetActive(false);
            }
        }

        [HarmonyPatch(nameof(MenuManager.ClickHostButton))]
        [HarmonyPrefix]
        static void patchClickHostButton()
        {
            LCDuelsModBase.playing = false;
            LCDuelsModBase.Instance.allowSaveLoading();
            //DeleteFileButton saveFileUISlot = UnityEngine.Object.FindFirstObjectByType<DeleteFileButton>();
            //Debug.Log(saveFileUISlot);
            //if (saveFileUISlot != null )
            //{
            //    Component[] components = saveFileUISlot.transform.parent.transform.parent.transform.parent.GetComponents<Component>();

            //    foreach (Component component in components)
            //    {
            //        Debug.Log("ComponentXD: " + component.GetType());
            //    }
            //}
            //else
            //{
            //    Debug.Log("no save file UI slot");
            //}
            ResetHostMenuValues();
            TextMeshProUGUI placeholderText = LCDuelsModBase.Instance.menuManager.lobbyNameInputField.GetComponentInChildren<TextMeshProUGUI>();
            placeholderText.text = "Name your server...";
        }

        [HarmonyPatch(nameof(MenuManager.ConfirmHostButton))]
        [HarmonyPrefix]
        static bool patchConfirm()
        {
            LCDuelsModBase.Instance.queueName = LCDuelsModBase.Instance.menuManager.lobbyNameInputField.text;
            if (LCDuelsModBase.playing)
            {
                OnHostLCDuels();
                return false;
            }
            else
            {
                return true;
            }
        }

        static void ResetHostMenuValues()
        {
            LCDuelsModBase.Instance.publicText.text = "Public";
            LCDuelsModBase.Instance.friendsText.text = "Friends-only";
            LCDuelsModBase.Instance.serverName.text = "Server name:";
        }

        [HarmonyPatch(nameof(MenuManager.HostSetLobbyPublic))]
        [HarmonyPostfix]
        static void patchHostSetLobbyPublic(bool setPublic)
        {
            if (LCDuelsModBase.playing)
            {
                LCDuelsModBase.Instance.isPublicQueue = setPublic;
                if (setPublic)
                {
                    LCDuelsModBase.Instance.menuManager.privatePublicDescription.text = "For public queue you have to follow the rules and play on the current version";
                    LCDuelsModBase.Instance.menuManager.lobbyNameInputField.text = "";
                    LCDuelsModBase.Instance.menuManager.lobbyNameInputField.enabled = false;
                    TextMeshProUGUI placeholderText = LCDuelsModBase.Instance.menuManager.lobbyNameInputField.GetComponentInChildren<TextMeshProUGUI>();
                    placeholderText.text = "Leave empty";
                }
                else
                {
                    LCDuelsModBase.Instance.menuManager.privatePublicDescription.text = "For private queue select the same queue name as the players you want to queue up with and make sure you have matching mods and version";
                    LCDuelsModBase.Instance.menuManager.lobbyNameInputField.enabled = true;
                    TextMeshProUGUI placeholderText = LCDuelsModBase.Instance.menuManager.lobbyNameInputField.GetComponentInChildren<TextMeshProUGUI>();
                    placeholderText.text = "Name your queue...";
                }
            }
        }

        public static void OnHostLCDuels()
        {
            if (!LCDuelsModBase.Instance.isPublicQueue &&  LCDuelsModBase.Instance.menuManager.lobbyNameInputField.text == "")
            {
                LCDuelsModBase.Instance.menuManager.SetLoadingScreen(false, RoomEnter.Error, "Please enter private queue name");
            }
            else
            {
                LCDuelsModBase.Instance.mls.LogInfo("OnHostClicked");
                LCDuelsModBase.Instance.ResetValues(true);
                LCDuelsModBase.Instance.preventSaveLoading();
                GameNetworkManager.Instance.lobbyHostSettings = new HostSettings("LCDuels game", false);
                GameNetworkManager.Instance.StartHost();
            }
        }

        public static void OnPlayLCDuelsMenuOpen()
        {
            LCDuelsModBase.Instance.menuManager.EnableLeaderboardDisplay(false);
            LCDuelsModBase.Instance.menuManager.HostSettingsScreen.SetActive(true);
            LCDuelsModBase.playing = true;
            LCDuelsModBase.Instance.publicText.text = "Public Queue";
            LCDuelsModBase.Instance.friendsText.text = "Private Queue";
            LCDuelsModBase.Instance.serverName.text = "Queue name:";

            string defaultValue;
            if (GameNetworkManager.Instance.disableSteam)
            {
                defaultValue = "Unnamed";
            }
            else if (!SteamClient.IsLoggedOn)
            {
                LCDuelsModBase.Instance.menuManager.DisplayMenuNotification("Could not connect to Steam servers! (If you just want to play on your local network, choose LAN on launch.)", "Continue");
                defaultValue = "Unnamed";
            }
            else
            {
                defaultValue = SteamClient.Name.ToString() + "'s Crew";
            }
            LCDuelsModBase.Instance.menuManager.lobbyNameInputField.text = "";
            LCDuelsModBase.Instance.menuManager.HostSetLobbyPublic(true);
        }

        public static void PrintChildren(GameObject parent)
        {
            // Check if the parent has children
            if (parent.transform.childCount == 0)
            {
                Component[] components = parent.GetComponents<Component>();

                // Print each component's type
                Debug.Log("Leaf Node: " + parent.name);
                foreach (Component component in components)
                {
                    Debug.Log("Component: " + component.GetType());
                }
                return;
            }

            // Iterate through each child of the parent GameObject
            foreach (Transform child in parent.transform)
            {
                // Print the child's name and type
                Debug.Log("Name: " + child.gameObject.name + ", Type: " + child.gameObject.GetType());

                // Recursively print information for the child's children
                PrintChildren(child.gameObject);
            }
        }
    }
}
