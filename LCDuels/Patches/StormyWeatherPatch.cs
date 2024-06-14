using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(StormyWeather))]
    internal class StormyWeatherPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void patchUpdate(ref List<GrabbableObject> ___metalObjects)
        {
            //___randomThunderTime = 0f;
            //int i = 0;
            //foreach (GrabbableObject obj in ___metalObjects)
            //{
            //    i++;
            //    UnityEngine.Debug.Log(i.ToString()+obj.ToString());
            //}
        }
    }
}
