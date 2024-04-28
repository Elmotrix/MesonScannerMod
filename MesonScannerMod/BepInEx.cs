using HarmonyLib;
using System;
using UnityEngine;

namespace MesonScannerMod
{
    #region BepInEx
    [BepInEx.BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class MesonScannerMod : BepInEx.BaseUnityPlugin
    {
        public const string pluginGuid = "net.elmo.stationeers.MesonScannerMod";
        public const string pluginName = "MesonScannerMod";
        public const string pluginVersion = "0.2";
        public static void Log(string line)
        {
            Debug.Log("[" + pluginName + "]: " + line);
        }
        void Awake()
        {
            try
            {
                var harmony = new Harmony(pluginGuid);
                harmony.PatchAll();
                Log("Patch succeeded");
            }
            catch (Exception e)
            {
                Log("Patch Failed");
                Log(e.ToString());
            }
        }
    }
    #endregion
}
