using DunGen;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {
        [HarmonyPatch("SetBuyingRateForDay")]
        [HarmonyPostfix]
        static void SetBuyingRateForDayPatch()
        {
                if (LCDuelsModBase.Instance.gameMode == 3)
                {
                TimeOfDay.Instance.timeUntilDeadline = TimeOfDay.Instance.totalTime;
                LCDuelsModBase.Instance.mls.LogInfo("!!!!!!!!!!!!!!Setting to 3");
                    TimeOfDay.Instance.daysUntilDeadline = 3;
                StartOfRound.Instance.deadlineMonitorText.text = "DEADLINE:\n3 days";
            }
        }
        [HarmonyPatch("UpdateProfitQuotaCurrentTime")]
        [HarmonyPostfix]
        static void UpdateProfitQuotaCurrentTimePatch()
        {
                if (LCDuelsModBase.Instance.gameMode == 3)
                {
                TimeOfDay.Instance.timeUntilDeadline = TimeOfDay.Instance.totalTime;
                LCDuelsModBase.Instance.mls.LogInfo("!!!!!!!!!!!!!!Setting to 3");
                    TimeOfDay.Instance.daysUntilDeadline = 3;
                StartOfRound.Instance.deadlineMonitorText.text = "DEADLINE:\n3 days";
            }
        }
    }
}
