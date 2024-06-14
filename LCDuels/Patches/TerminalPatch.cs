using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LCDuels.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void patchStart(ref int ___groupCredits)
        {
            string url = "http://10.10.10.215:3000/";

            // Create an instance of HttpClient
            using (HttpClient client = new HttpClient())
            {
                    // Perform the GET request
                    HttpResponseMessage response = client.GetAsync(url).Result;

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string
                    string responseBody = response.Content.ReadAsStringAsync().Result;

                    // Convert the response to an integer
                    LCDuelsModBase.Instance.seedFromServer = int.Parse(responseBody);
                    Random lmfao = new Random(LCDuelsModBase.Instance.seedFromServer);
                    ___groupCredits = lmfao.Next(100, 1000);
            }
        }
    }
}
