using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.AI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void Postfix(PlayerControllerB __instance)
        {
            //__instance.gameplayCamera.transform.localRotation = Quaternion.Euler(70, 0, 0);
            //__instance.gameplayCamera.transform.LookAt(new Vector3 (0, 0, 0));
            //UnityEngine.Debug.Log(1
            //+"\n"+__instance.gameplayCamera.transform.rotation.eulerAngles
            //+"\n" + __instance.gameplayCamera.transform.localRotation.eulerAngles
            //+"\n" + __instance.gameplayCamera.transform.parent.rotation.eulerAngles
            //+"\n" + __instance.gameplayCamera.transform.parent.localRotation.eulerAngles);
            LCDuelsModBase.Instance.lastPosition = __instance.transform.position;
            if (LCDuelsModBase.Instance.targettedGO != null)
            {
                //Vector3 direction = (LCDuelsModBase.Instance.targettedGO.transform.position - GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position).normalized;
                __instance.gameplayCamera.transform.LookAt(LCDuelsModBase.Instance.targettedGO.transform);
                FieldInfo field = __instance.GetType().GetField("cameraUp", BindingFlags.NonPublic | BindingFlags.Instance);
                field.SetValue(__instance, __instance.gameplayCamera.transform.localEulerAngles.x);
                //Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                //LCDuelsModBase.Instance.agent.transform.rotation = lookRotation;
                //GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.rotation = lookRotation;
                //__instance.thisPlayerBody.Rotate(new Vector3(0f, direction.y, 0f), Space.Self);
                //__instance.thisPlayerBody.rotation = Quaternion.LookRotation(new Vector3(0f, direction.y, 0f));
                //float cameraUp = -80f;
                //__instance.smoothLookTurnCompass.localEulerAngles = new Vector3(
                //    cameraUp,
                //    __instance.smoothLookTurnCompass.localEulerAngles.y, 
                //    __instance.smoothLookTurnCompass.localEulerAngles.z);
                //__instance.smoothLookTurnCompass.eulerAngles = new Vector3(
                //    __instance.smoothLookTurnCompass.eulerAngles.x, 
                //    __instance.smoothLookTurnCompass.eulerAngles.y, 
                //    __instance.thisPlayerBody.transform.eulerAngles.z);
                //__instance.thisPlayerBody.eulerAngles = new Vector3(
                //    __instance.thisPlayerBody.eulerAngles.x, 
                //    __instance.smoothLookTurnCompass.eulerAngles.y, 
                //    __instance.thisPlayerBody.eulerAngles.z);
                //__instance.gameplayCamera.transform.localEulerAngles = new Vector3(
                //    cameraUp, 
                //    __instance.gameplayCamera.transform.localEulerAngles.y, 
                //    __instance.gameplayCamera.transform.localEulerAngles.z);
            }
        }

        [HarmonyPatch("CalculateSmoothLookingInput")]
        [HarmonyPrefix]
        static bool patchCalculate()
        {
            return LCDuelsModBase.Instance.targettedGO == null;
        }

        [HarmonyPatch(nameof(PlayerControllerB.SetItemInElevator))]
        [HarmonyPostfix]
        static void patchSetItemInElevatorPatch()
        {
            if (LCDuelsModBase.playing)
            {
                LCDuelsModBase.Instance.currentValue = RoundManager.Instance.scrapCollectedInLevel;
                LCDuelsModBase.Instance.mls.LogInfo("Sending score "+RoundManager.Instance.scrapCollectedInLevel);
                _ = LCDuelsModBase.Instance.SendMessage(new { type="score",value= RoundManager.Instance.scrapCollectedInLevel.ToString() });
                LCDuelsModBase.Instance.UpdateInGameStatusText();
            }
        }
        //[HarmonyPatch(nameof(PlayerControllerB.Update))] if public member
        [HarmonyPatch("UpdatePlayerPositionClientRpc")]
        [HarmonyPostfix]
        static void patchUpate(PlayerControllerB __instance)
        {
            if (__instance.isInHangarShipRoom != LCDuelsModBase.Instance.isInShip && LCDuelsModBase.playing &&!__instance.playersManager.hangarDoorsClosed)
            {
                LCDuelsModBase.Instance.isInShip = __instance.isInHangarShipRoom;
                if (__instance.isInHangarShipRoom ) 
                {
                    _ = LCDuelsModBase.Instance.SendMessage(new { type= "position", value= "0"});
                }
                else
                {
                    _ =LCDuelsModBase.Instance.SendMessage(new { type= "position", value= "1"});
                }
            }
        }
        [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
        [HarmonyPostfix]
        static void patchKillPlayer()
        {
            LCDuelsModBase.Instance.mls.LogInfo("Sending death info");
            LCDuelsModBase.Instance.death = true;
            LCDuelsModBase.Instance.WaitingForResult();
            _ = LCDuelsModBase.Instance.SendMessage(new { type = "death" });
        }
        [HarmonyPatch(nameof(PlayerControllerB.DamagePlayer))]
        [HarmonyPrefix]
        static bool patchDamage()
        {
            return false;
        }
    }
}
