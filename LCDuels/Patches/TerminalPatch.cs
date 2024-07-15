using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void patchStart(Terminal __instance)
        {
            LCDuelsModBase.Instance.mls.LogInfo("Is LCDuels enabled: "+LCDuelsModBase.playing);
            if (LCDuelsModBase.playing)
            {
                __instance.groupCredits = 0;
                GameNetworkManager.Instance.SetLobbyJoinable(false);
                GameNetworkManager.Instance.disallowConnection = true;
                LCDuelsModBase.Instance.terminal = __instance;
                StartOfRound.Instance.screenLevelDescription.text = "Waiting for other player\nYou can join our discord (dc.ontro.cz) to find people to play with.";
                StartMatchLever matchLever = UnityEngine.Object.FindFirstObjectByType<StartMatchLever>();
                matchLever.triggerScript.disabledHoverTip = "[Wait for the other player]";
                matchLever.triggerScript.interactable = false;
                LCDuelsModBase.Instance.UpdateInGameStatusText();
                Task.Run(LCDuelsModBase.Instance.InitWS);
                __instance.StartCoroutine(LCDuelsModBase.Instance.waitUntilEndOfGame());
                StartOfRound.Instance.DisableShipSpeaker();
            }
        }

        [HarmonyPatch(nameof(Terminal.SyncGroupCreditsClientRpc))]
        [HarmonyPostfix]
        static void patchSyncBoughItemsWithServer(Terminal __instance)
        {
            if (LCDuelsModBase.playing)
            {
                foreach (int itemToDeliver in __instance.orderedItemsFromTerminal)
                {
                    LCDuelsModBase.Instance.mls.LogInfo("Spawing item"+itemToDeliver);
                    GameObject go = UnityEngine.Object.Instantiate(__instance.buyableItemsList[itemToDeliver].spawnPrefab, GameNetworkManager.Instance.localPlayerController.transform.position,Quaternion.identity,StartOfRound.Instance.propsContainer);
                    GrabbableObject grabbableObject = go.GetComponent<GrabbableObject>();
                    
                    if (grabbableObject != null)
                    {
                        go.GetComponent<NetworkObject>().Spawn(false);
                        grabbableObject.itemProperties.canBeGrabbedBeforeGameStart = true;
                        grabbableObject.isInElevator = true;
                        grabbableObject.isInShipRoom = true;
                        grabbableObject.fallTime = 0;
                        grabbableObject.parentObject = null;
                        grabbableObject.transform.SetParent(StartOfRound.Instance.elevatorTransform, true);
                        StartOfRound.Instance.currentShipItemCount++;
                    }
                }
                __instance.orderedItemsFromTerminal.Clear();
            }
        }

    }
}
