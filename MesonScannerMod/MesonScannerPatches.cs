using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Networks;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;
using Networks;
using UnityEngine;

namespace MesonScannerMod
{
    [HarmonyPatch(typeof(SPUMesonScanner), "Render")]
    public class SPUMesonScannerRender
    {
        [HarmonyPrefix]
        public static bool Prefix(SPUMesonScanner __instance)
        {
            if (InventoryManager.Instance.ActiveHand.Slot.Get() is SprayCan sprayCan)
            {
                Utils.colorSwatch = sprayCan.GetPaintMaterial();
            }
            else if (InventoryManager.Instance.InactiveHand.Slot.Get() is SprayCan sprayCan2)
            {
                Utils.colorSwatch = sprayCan2.GetPaintMaterial();
            }
            else
            {
                Utils.colorSwatch = null;
            }

            switch (Utils.CurrentMode)
            {
                case Utils.Mode.Pipes:
                    foreach (PipeNetwork allPipeNetwork in PipeNetwork.AllPipeNetworks)
                    {
                        foreach (INetworkedStructure structure in allPipeNetwork.StructureList)
                        {
                            if (structure is Pipe pipe && !pipe.IsOccluded && !(pipe is HydroponicTray))
                            {
                                if (Utils.colorSwatch == null || (pipe.CustomColor.Normal == null && Utils.colorSwatch == pipe.PaintableMaterial) || Utils.colorSwatch == pipe.CustomColor.Normal)
                                {
                                    SPUMesonScannerAddToBatch.AddToBatch(pipe);
                                }
                            }
                        }
                    }
                    break;
                case Utils.Mode.Cables:
                    foreach (CableNetwork allCableNetwork in CableNetwork.AllCableNetworks)
                    {
                        foreach (Cable cable in allCableNetwork.CableList)
                        {
                            if (!cable.IsOccluded)
                            {
                                if (Utils.colorSwatch == null || (cable.CustomColor.Normal == null && Utils.colorSwatch == cable.PaintableMaterial) || Utils.colorSwatch == cable.CustomColor.Normal)
                                {
                                    SPUMesonScannerAddToBatch.AddToBatch(cable);
                                }
                            }
                        }
                    }
                    break;
                case Utils.Mode.Chutes:
                    foreach (ChuteNetwork allChuteNetwork in ChuteNetwork.AllChuteNetworks)
                    {
                        foreach (Chute chute in allChuteNetwork.StructureList)
                        {
                            if (!chute.IsOccluded)
                            {
                                if (Utils.colorSwatch == null || (chute.CustomColor.Normal == null && Utils.colorSwatch == chute.PaintableMaterial) || Utils.colorSwatch == chute.CustomColor.Normal)
                                {
                                    SPUMesonScannerAddToBatch.AddToBatch(chute);
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            SPUMesonScannerRenderMeshes.RenderMeshes(__instance);
            SPUMesonScannerCleanUp.CleanUp(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(SPUMesonScanner), "AddToBatch")]
    public class SPUMesonScannerAddToBatch
    {
        [HarmonyReversePatch]
        public static void AddToBatch(Structure structure)
        {
        }
    }

    [HarmonyPatch(typeof(SPUMesonScanner), "RenderMeshes")]
    public class SPUMesonScannerRenderMeshes
    {
        [HarmonyReversePatch]
        public static void RenderMeshes(SPUMesonScanner instance)
        {
        }
    }

    [HarmonyPatch(typeof(SPUMesonScanner), "CleanUp")]
    public class SPUMesonScannerCleanUp
    {
        [HarmonyReversePatch]
        public static void CleanUp(SPUMesonScanner instance)
        {
        }
    }

    [HarmonyPatch(typeof(InventoryManager), "NormalMode")]
    public class KeyToggles
    {
        [HarmonyPrefix]
        public static void Prefix(InventoryManager __instance)
        {
            bool secondary = KeyManager.GetMouseDown("Secondary");
            if (secondary)
            {
                if (InventoryManager.Parent is Human human)
                {
                    if (human.GlassesSlot.Get() is SensorLenses lenses)
                    {
                        if (lenses.Sensor is SPUMesonScanner)
                        {
                            Utils.NextMode();
                        }
                    }
                }
            }
        }
    }

    public class Utils
    {
        public static Material colorSwatch;
        public static Mode CurrentMode = Mode.Pipes;
        public enum Mode { Pipes, Cables, Chutes}

        public static void NextMode()
        {
            switch (CurrentMode)
            {
                case Mode.Pipes:
                    CurrentMode = Mode.Cables;
                    ConsoleWindow.Print("Set Mode Cables");
                    break;
                case Mode.Cables:
                    CurrentMode = Mode.Chutes;
                    ConsoleWindow.Print("Set Mode Chutes");
                    break;
                case Mode.Chutes:
                    CurrentMode = Mode.Pipes;
                    ConsoleWindow.Print("Set Mode Pipes");
                    break;
                default:
                    break;
            }
        }
    }
}
