using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Inventory;
using Assets.Scripts.Networks;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using HarmonyLib;
using Networks;
using Objects.Pipes;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.Atmospherics.Chemistry;

namespace MesonScannerMod
{
    [HarmonyPatch(typeof(SPUMesonScanner), "Render")]
    public class SPUMesonScannerRender
    {
        [HarmonyPrefix]
        public static bool Prefix(SPUMesonScanner __instance, List<ScannerMeshBatch> ____batchList)
        {
            Material material = null;
            if (InventoryManager.Instance.ActiveHand.Slot.Get() is SprayCan sprayCan)
            {
                material = sprayCan.GetPaintMaterial();
            }
            else if (InventoryManager.Instance.InactiveHand.Slot.Get() is SprayCan sprayCan2)
            {
                material = sprayCan2.GetPaintMaterial();
            }
            string modeString = "";

            bool alternaltiveMode = KeyManager.GetButton(KeyCode.LeftControl);
            if (Utils.CurrentMode == Utils.Mode.Pipes || (Utils.CurrentMode == Utils.Mode.Cables && Utils.CurrentDisplayMode == Utils.DisplayMode.Mode1))
            {
                switch (Utils.CurrentDisplayMode)
                {
                    case Utils.DisplayMode.Mode1:
                        modeString = "Pipes - Paintend Color";
                        break;
                    case Utils.DisplayMode.Mode2:
                        modeString = "Pipes - Stess";
                        break;
                    case Utils.DisplayMode.Mode3:
                        modeString = "Pipes - Temperature";
                        break;
                    default:
                        break;
                }
                if (alternaltiveMode)
                {
                    modeString += " + Connected Devices";
                }
                foreach (PipeNetwork allPipeNetwork in Utils.GetPipeNetworks(material))
                {
                    Color netowrkColor = Utils.GetPipenetworkColor(allPipeNetwork);
                    foreach (Pipe pipe in allPipeNetwork.StructureList)
                    {
                        if (!pipe.IsOccluded && (!(pipe is HydroponicTray) || alternaltiveMode) && (pipe.DamageState.TotalRatio == 0 || Utils.Blink())) //
                        {
                            Color color = pipe.CustomColor.Color;
                            if (Utils.CurrentDisplayMode != Utils.DisplayMode.Mode1)
                            {
                                color = netowrkColor;
                            }
                            if (pipe.IsBurst != 0) //Parent.DamageState.TotalRatio > Parent.CriticalHealth; //pipe.DamageState.Total >= pipe.DamageState.MaxDamage
                            {
                                Utils.AddToBatch(pipe, color, ref ____batchList, null, GameManager.Instance.CustomColors[4].Color);
                                continue;
                            }
                            Utils.AddToBatch(pipe, color, ref ____batchList);
                        }
                    }
                    if (alternaltiveMode)
                    {
                        foreach (Structure item in allPipeNetwork.DeviceList)
                        {
                            if (!item.IsOccluded)
                            {
                                Utils.AddToBatch(item, item.CustomColor.Color, ref ____batchList);
                            }
                        }
                    }
                }
            }
            if (Utils.CurrentMode == Utils.Mode.Cables)
            {
                switch (Utils.CurrentDisplayMode)
                {
                    case Utils.DisplayMode.Mode1:
                        modeString = "Cables + Pipes - Paintend Color";
                        break;
                    case Utils.DisplayMode.Mode2:
                        modeString = "Cables - Paintend Color";
                        break;
                    case Utils.DisplayMode.Mode3:
                        modeString = "Cables - Load";
                        break;
                    default:
                        break;
                }
                if (alternaltiveMode)
                {
                    modeString += " + Connected Devices";
                }
                foreach (CableNetwork allCableNetwork in Utils.GetCableNetworks(material))
                {
                    foreach (Cable cable in allCableNetwork.CableList)
                    {
                        if (!cable.IsOccluded)
                        {
                            Color color = Utils.GetCableColor(cable, allCableNetwork);
                            Utils.AddToBatch(cable, color, ref ____batchList);
                        }
                    }
                    if (alternaltiveMode)
                    {
                        modeString = "Connected Devices - Operational State";
                        foreach (Device item in allCableNetwork.DeviceList)
                        {
                            if (!item.IsOccluded)
                            {
                                Color thisColor = item.CustomColor.Color;
                                Utils.AddToBatch(item, thisColor, ref ____batchList);
                            }
                        }
                    }
                }
            }
            if (Utils.CurrentMode == Utils.Mode.Chutes && Utils.CurrentDisplayMode != Utils.DisplayMode.Mode3)
            {
                switch (Utils.CurrentDisplayMode)
                {
                    case Utils.DisplayMode.Mode1:
                        modeString = "Chutes - Paintend Color";
                        break;
                    case Utils.DisplayMode.Mode2:
                        modeString = "Chutes - Contents";
                        break;
                    default:
                        break;
                }
                if (alternaltiveMode)
                {
                    modeString += " + Connected Devices";
                }
                foreach (ChuteNetwork allChuteNetwork in Utils.GetChuteNetworks(material))
                {
                    foreach (Chute chute in allChuteNetwork.StructureList)
                    {
                        if (!chute.IsOccluded)
                        {
                            Color color = chute.CustomColor.Color;
                            if (Utils.CurrentDisplayMode == Utils.DisplayMode.Mode2)
                            {
                                color = GameManager.Instance.CustomColors[7].Color * 0.4f;
                                color += GameManager.Instance.CustomColors[1].Color * 0.6f;
                            }
                            Utils.AddToBatch(chute, color, ref ____batchList);
                            Thing thing = chute.TransportSlot.Get();
                            if (thing != null && Utils.CurrentDisplayMode == Utils.DisplayMode.Mode2 && !thing.IsOccluded)
                            {
                                Utils.AddToBatch(thing, GameManager.Instance.CustomColors[0].Color, ref ____batchList, chute);
                            }
                        }
                    }
                    if (alternaltiveMode || Utils.CurrentDisplayMode == Utils.DisplayMode.Mode2)
                    {
                        foreach (Structure item in allChuteNetwork.DeviceList)
                        {
                            if (alternaltiveMode && !item.IsOccluded)
                            {
                                SPUMesonScannerAddToBatch.AddToBatch(item);
                            }
                            if (Utils.CurrentDisplayMode == Utils.DisplayMode.Mode2)
                            {
                                foreach (Slot slot in item.Slots)
                                {
                                    Thing thing = slot.Get();
                                    if (thing != null && !thing.IsOccluded)
                                    {
                                        Utils.AddToBatch(thing, GameManager.Instance.CustomColors[0].Color, ref ____batchList);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (Utils.CurrentMode == Utils.Mode.Other && Utils.CurrentDisplayMode == Utils.DisplayMode.Mode2)
            {
                modeString = "Structures - Convection";
                foreach (Thing item in AtmosphericsManager.AtmosphericThings)
                {
                    if (item is Structure && !item.IsOccluded && item.EnergyConvected != 0 && !(item is Pipe))
                    {
                        Color color = Utils.GetConvectionColor(item.EnergyConvected);
                        Utils.AddToBatch(item, color, ref ____batchList);
                    }
                }
            }
            if (Utils.CurrentMode == Utils.Mode.Other && Utils.CurrentDisplayMode == Utils.DisplayMode.Mode3)
            {
                modeString = "Structures - Damage";
                foreach (Thing item in Thing.AllThings)
                {
                    if (item.DamageState.TotalRatio > 0 && item is Structure && !item.IsOccluded)
                    {
                        Color thisColor = GameManager.Instance.CustomColors[2].Color * (1 - item.DamageState.TotalRatio);
                        thisColor += GameManager.Instance.CustomColors[4].Color * item.DamageState.TotalRatio;
                        Utils.AddToBatch(item, thisColor, ref ____batchList);
                    }
                }
            }
            if (Utils.CurrentMode == Utils.Mode.Chutes && Utils.CurrentDisplayMode == Utils.DisplayMode.Mode3)
            {
                modeString = "Loose Items";
                foreach (Item item in Item.AllItems)
                {
                    if (item.ParentSlot == null && !item.IsOccluded)
                    {
                        Utils.AddToBatch(item, item.CustomColor.Color, ref ____batchList);
                    }
                }
            }
            if (Utils.CurrentMode == Utils.Mode.Other && Utils.CurrentDisplayMode == Utils.DisplayMode.Mode1)
            {
                foreach (Plant plant in Plant.AllPlants)
                {
                    Color thisColor = new Color(0, 0, 0);
                    float ratio;
                    if (alternaltiveMode)
                    {
                        ratio = plant.lifeRequirements.GrowthEfficiency();
                        ratio = ratio / 1.2f;
                        Mathf.Clamp01(ratio);
                        ratio = ratio * ratio;
                        thisColor = GameManager.Instance.CustomColors[4].Color * (1 - ratio);
                        thisColor += GameManager.Instance.CustomColors[2].Color * ratio;
                        modeString = "Plants - Growth Efficiency";
                    }
                    else
                    {
                        if (plant.IsDead)
                        {
                            thisColor = GameManager.Instance.CustomColors[4].Color;
                        }
                        else if (plant.IsSeeding)
                        {
                            thisColor = GameManager.Instance.CustomColors[2].Color;
                        }
                        else if (plant.Stage == 5)
                        {
                            thisColor = GameManager.Instance.CustomColors[9].Color * 0.5f;
                            thisColor += GameManager.Instance.CustomColors[2].Color * 0.5f;
                        }
                        else if (plant.Stage == 4)
                        {
                            thisColor = GameManager.Instance.CustomColors[11].Color;
                        }
                        else if (plant.Stage == 3)
                        {
                            thisColor = GameManager.Instance.CustomColors[5].Color;
                        }
                        else if (plant.Stage == 2)
                        {
                            thisColor = GameManager.Instance.CustomColors[10].Color;
                        }
                        else
                        {
                            thisColor = GameManager.Instance.CustomColors[0].Color;
                        }
                        modeString = "Plants - Growth Stage";
                    }
                    Utils.AddToBatch(plant, thisColor, ref ____batchList);

                    // 0: blue
                    // 1: gray
                    // 2: green
                    // 3: orange
                    // 4: red
                    // 5: yellow
                    // 6: white
                    // 7: black
                    // 8: brown
                    // 9: khaki
                    //10: pink
                    //11: purple
                }


            }
            if (InventoryManager.Parent is Human human)
            {
                foreach (InventoryWindow window in InventoryWindowManager.Instance.Windows)
                {
                    if (window.IsVisible && window.ParentSlot.StringHash == human.GlassesSlot.StringHash)
                    {
                        window.WindowTitleBar.SetTitle(modeString);
                        break;
                    }
                }
            }


            SPUMesonScannerRenderMeshes.RenderMeshes(__instance);
            SPUMesonScannerCleanUp.CleanUp(__instance);
            Utils.Clear();
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

            bool toggleMode = KeyManager.GetButtonDown(KeyCode.LeftShift);
            bool primary = KeyManager.GetMouseDown("Primary");
            bool secondary = KeyManager.GetMouseDown("Secondary");
            bool lockIn = KeyManager.GetButtonDown(KeyCode.Mouse2);
            bool keyUp = __instance.newScrollData > 0f;
            bool keyDown = __instance.newScrollData < 0f;
            if (InventoryManager.Parent is Human human)
            {
                if (human.GlassesSlot.Get() is SensorLenses lenses)
                {
                    if (lenses.Sensor is SPUMesonScanner && lenses.OnOff)
                    {
                        if (toggleMode)
                        {
                            Utils.NextDisplayMode();
                        }
                        if (lockIn)
                        {
                            Utils.SetLockIn();
                        }
                        if (InventoryManager.Instance.ActiveHand.Slot.Get() == null)
                        {
                            if (primary)
                            {
                                Utils.SetTarget(CursorManager.CursorThing);
                            }
                            if (secondary)
                            {
                                Utils.NextMode();
                            }
                            if (keyUp)
                            {
                                Utils.Iterate(+1);
                            }
                            if (keyDown)
                            {
                                Utils.Iterate(-1);
                            }
                        }
                    }
                }
            }
        }
    }

    public class Utils
    {
        public static Material colorSwatch;
        public static Mode CurrentMode = Mode.Cables;
        public static DisplayMode CurrentDisplayMode = DisplayMode.Mode1;

        private static bool LockIn = false;
        private static float maxEnergy = 0;
        private static float minEnergy = 0;
        private static float maxTemperature = 0;
        private static float minTemperature = 999999;
        private static int listIndex = -1;

        private static Material colorFilter = null;
        private static Dictionary<int, ScannerMeshBatch> Batches = new Dictionary<int, ScannerMeshBatch>(100);

        private static List<PipeNetwork> lockPipes = new List<PipeNetwork>();
        private static List<CableNetwork> lockCables = new List<CableNetwork>();
        private static List<ChuteNetwork> lockChutes = new List<ChuteNetwork>();

        public enum Mode { Pipes, Cables, Chutes, Other }
        public enum DisplayMode { Mode1, Mode2, Mode3 }

        public static void Clear()
        {
            Batches.Clear();
        }
        public static bool Blink()
        {
            int f = System.DateTime.Now.Millisecond;
            return (f >= 0 && f <= 300) || (f >= 500 && f <= 800); 
        }
        public static void Iterate(short i)
        {
            if (!LockIn)
            {
                return;
            }
            listIndex += i;
            int max = 0;
            switch (CurrentMode)
            {
                case Mode.Pipes:
                    max = lockPipes.Count;
                    break;
                case Mode.Cables:
                    max = lockCables.Count;
                    break;
                case Mode.Chutes:
                    max = lockChutes.Count;
                    break;
                default:
                    break;
            }
            while (listIndex >= max)
            {
                listIndex -= (max + 1);
            }

            while (listIndex < -1)
            {
                listIndex += (max + 1);
            }
        }
        public static void SetTarget(Thing thing)
        {
            if (thing == null)
            {
                lockPipes.Clear();
                lockCables.Clear();
                lockChutes.Clear();
                listIndex = -1;
                LockIn = false;
            }
            else if (thing is Cable cable)
            {
                listIndex = -1;
                if (CurrentMode != Mode.Cables)
                {
                    CurrentMode = Mode.Cables;
                    CurrentDisplayMode = DisplayMode.Mode2;
                }
                if (CurrentDisplayMode == DisplayMode.Mode1)
                {
                    CurrentDisplayMode = DisplayMode.Mode2;
                }
                LockIn = true;
                lockPipes.Clear();
                lockCables.Clear();
                lockChutes.Clear();
                lockCables.Add(cable.CableNetwork);
            }
            else if (thing is Chute chute)
            {
                listIndex = -1;
                if (CurrentMode != Mode.Chutes)
                {
                    CurrentMode = Mode.Chutes;
                    CurrentDisplayMode = DisplayMode.Mode1;
                }
                LockIn = true;
                lockPipes.Clear();
                lockCables.Clear();
                lockChutes.Clear();
                lockChutes.Add(chute.ChuteNetwork);
            }
            else if (thing is Pipe pipe)
            {
                listIndex = -1;
                if (CurrentMode != Mode.Pipes)
                {
                    CurrentMode = Mode.Pipes;
                    CurrentDisplayMode = DisplayMode.Mode1;
                }
                LockIn = true;
                lockPipes.Clear();
                lockCables.Clear();
                lockChutes.Clear();
                lockPipes.Add(pipe.PipeNetwork);
            }
        }
        public static void SetLockIn()
        {
            if (LockIn)
            {
                lockPipes.Clear();
                lockCables.Clear();
                lockChutes.Clear();
                listIndex = -1;
            }
            LockIn = !LockIn;
        }
        public static List<ChuteNetwork> GetChuteNetworks(Material color)
        {
            if (LockIn && lockChutes.Count > 0)
            {
                if (listIndex > -1)
                {
                    return new List<ChuteNetwork>() { lockChutes[listIndex] };
                }
                return lockChutes;
            }
            lockChutes.Clear();

            colorFilter = color;
            foreach (ChuteNetwork chuteNetwork in GetChuteNetworks())
            {
                if (colorFilter == null)
                {
                    lockChutes.Add(chuteNetwork);
                }
                else
                {
                    foreach (Chute chute in chuteNetwork.StructureList)
                    {
                        if ((chute.CustomColor.Normal == null && colorFilter == chute.PaintableMaterial) || colorFilter == chute.CustomColor.Normal)
                        {
                            lockChutes.Add(chuteNetwork);
                            break;
                        }
                    }
                }
            }
            return lockChutes;
        }
        private static List<ChuteNetwork> GetChuteNetworks()
        {
            return ChuteNetwork.AllChuteNetworks;
        }
        public static List<CableNetwork> GetCableNetworks( Material color)
        {
            if (LockIn && lockCables.Count > 0)
            {
                if (listIndex > -1)
                {
                    return new List<CableNetwork>() { lockCables[listIndex] };
                }
                return lockCables;
            }
            lockCables.Clear();
            colorFilter = color;
            foreach (CableNetwork cableNetwork in GetCableNetworks())
            {
                if (colorFilter == null )
                {
                    lockCables.Add(cableNetwork);
                }
                else
                {
                    foreach (Cable cable in cableNetwork.CableList)
                    {
                        if ((cable.CustomColor.Normal == null && colorFilter == cable.PaintableMaterial) || colorFilter == cable.CustomColor.Normal)
                        {
                            lockCables.Add(cableNetwork);
                            break;
                        }
                    }
                }
            }
            return lockCables;
        }
        private static List<CableNetwork> GetCableNetworks()
        {
            return CableNetwork.AllCableNetworks;
        }
        public static List<PipeNetwork> GetPipeNetworks(Material color)
        {
            if (LockIn && CurrentMode == Mode.Cables)
            {
                return new List<PipeNetwork>();
            }
            if (LockIn && lockPipes.Count > 0)
            {
                if (listIndex > -1)
                {
                    return new List<PipeNetwork>() { lockPipes[listIndex] };
                }
                return lockPipes;
            }
            colorFilter = color;
            lockPipes.Clear();

            foreach (PipeNetwork pipeNetwork in GetPipeNetworks())
            {
                if (colorFilter == null)
                {
                    lockPipes.Add(pipeNetwork);
                }
                else
                {
                    foreach (Pipe pipe in pipeNetwork.StructureList)
                    {
                        if (!(pipe is HydroponicTray) && !(pipe is PassiveVent) &&
                            ((pipe.CustomColor.Normal == null && colorFilter == pipe.PaintableMaterial) || colorFilter == pipe.CustomColor.Normal))
                        {
                            lockPipes.Add(pipeNetwork);
                            break;
                        }
                    }
                }
            }
            return lockPipes;
        }
        private static List<PipeNetwork> GetPipeNetworks()
        {
            return PipeNetwork.AllPipeNetworks;
        }
        public static void NextMode()   
        {
            switch (CurrentMode)
            {
                case Mode.Cables:
                    CurrentMode = Mode.Pipes;
                    break;
                case Mode.Pipes:
                    CurrentMode = Mode.Chutes;
                    break;
                case Mode.Chutes:
                    CurrentMode = Mode.Other;
                    break;
                case Mode.Other:
                    CurrentMode = Mode.Cables;
                    break;
                default:
                    break;
            }
            CurrentDisplayMode = DisplayMode.Mode1;
            colorFilter = null;
            listIndex = -1;
            LockIn = false;
    }
        public static void NextDisplayMode()
        {
            switch (CurrentDisplayMode)
            {
                case DisplayMode.Mode1:
                    CurrentDisplayMode = DisplayMode.Mode2;
                    break;
                case DisplayMode.Mode2:
                    CurrentDisplayMode = DisplayMode.Mode3;
                    break;
                case DisplayMode.Mode3:
                    if (CurrentMode == Mode.Cables && LockIn)
                    {
                        CurrentDisplayMode = DisplayMode.Mode2;
                    }
                    else
                    {
                        CurrentDisplayMode = DisplayMode.Mode1;
                    }
                    break;
                default:
                    break;
            }
        }
        public static Color GetConvectionColor(float convection)
        {
            if (convection < minEnergy)
            {
                minEnergy = convection;
            }
            if (convection > maxEnergy)
            {
                maxEnergy = convection;
            }
            Color thisColor = new Color(0, 0, 0);
            if (convection > 0)
            {
                float ratio = convection / maxEnergy;
                thisColor += GameManager.Instance.CustomColors[6].Color * (1-ratio);
                thisColor += GameManager.Instance.CustomColors[4].Color * ratio;
            }
            else
            {
                float ratio = convection / minEnergy;
                thisColor += GameManager.Instance.CustomColors[6].Color * (1 - ratio);
                thisColor += GameManager.Instance.CustomColors[0].Color * ratio;
            }
            return thisColor;
        }
        public static Color GetCableColor(Cable cable, CableNetwork cableNetwork)
        {
            if (CurrentDisplayMode != DisplayMode.Mode3)
            {
                return cable.CustomColor.Color;
            }
            float ratio = cableNetwork.CurrentLoad / cable.MaxVoltage;
            Color thisColor = GameManager.Instance.CustomColors[2].Color * (1- ratio);
            thisColor += GameManager.Instance.CustomColors[4].Color * ratio;
            return thisColor;
        }
        public static Color GetPipenetworkColor(PipeNetwork pipeNetwork)
        {
            if (CurrentDisplayMode == DisplayMode.Mode2)
            {
                float maxPressure = 60000;
                float maxLiquidRatio = 0.02f;
                if (pipeNetwork.NetworkContentType != Pipe.ContentType.Gas)
                {
                    maxPressure = 6000;
                    maxLiquidRatio = 1f;
                }
                float pressureRatio = pipeNetwork.Atmosphere.PressureGasses / maxPressure;
                float liquidRatio = pipeNetwork.Atmosphere.LiquidVolumeRatio / maxLiquidRatio;
                float ratio = Mathf.Clamp01(Mathf.Max(pressureRatio, liquidRatio));
                ;
                Color thisColor = new Color(0, 0, 0);
                if (ratio > 0.5f)
                {
                    ratio = (ratio - 0.5f) * 2f;
                    thisColor += GameManager.Instance.CustomColors[3].Color * (1 - ratio);
                    thisColor += GameManager.Instance.CustomColors[4].Color * ratio;
                }
                else
                {
                    ratio = ratio * 2;
                    thisColor += GameManager.Instance.CustomColors[2].Color * (1 - ratio);
                    thisColor += GameManager.Instance.CustomColors[3].Color * ratio;
                }
                return thisColor;
            }
            else if (CurrentDisplayMode == DisplayMode.Mode3)
            {
                float temp = pipeNetwork.Atmosphere.Temperature;
                if (temp < minTemperature)
                {
                    minTemperature = temp;
                }
                if (temp > maxTemperature)
                {
                    maxTemperature = temp;
                }
                float ratio = (temp - minTemperature) / (maxTemperature - minTemperature);
                Color thisColor = new Color(0, 0, 0);
                if (ratio > 0.5f)
                {
                    ratio = (ratio - 0.5f) * 2f;
                    thisColor += GameManager.Instance.CustomColors[6].Color * (1 - ratio);
                    thisColor += GameManager.Instance.CustomColors[4].Color * ratio;
                }
                else
                {
                    ratio = ratio*2;
                    thisColor += GameManager.Instance.CustomColors[0].Color * (1 - ratio);
                    thisColor += GameManager.Instance.CustomColors[6].Color * ratio;
                }
                return thisColor;
            }
            return GameManager.Instance.CustomColors[0].Color;
        }

        public static ScannerMeshBatch CreateMesh(Thing thing, Color color, Structure parent)
        {
            ScannerMeshBatch result = default(ScannerMeshBatch);
            result.Mesh = thing.Renderers[0].MeshFilter.mesh;
            if (thing is Plant plant && plant.ParentTray != null)
            {
                result.Mesh = plant.GrowthStates[plant.Stage].Visualizer.GetComponent<MeshFilter>().mesh;
            }
            if (thing is Structure structure)
            {
                if (structure.CurrentBuildState?.Visualizer != null)
                {
                    result.Mesh = structure.CurrentBuildState.Visualizer.GetComponent<MeshFilter>().mesh;
                }
            }
            result.Matrices = new List<Matrix4x4> { GetMatrix(thing, parent) };
            result.Colors = new List<Vector4> { color };
            return result;
        }
        public static ScannerMeshBatch CreateColliderMesh(Structure structure, Color color)
        {
            ScannerMeshBatch result = default(ScannerMeshBatch);
            result.Mesh = CursorManager.Instance.CursorHighlighter.GetComponent<Renderer>().GetComponent<MeshFilter>().mesh;
            //result.Mesh = CursorManager.Instance.CursorSelectionHighlighter.GetComponent<Renderer>().GetComponent<MeshFilter>().mesh;
            result.Matrices = new List<Matrix4x4> { Matrix4x4.TRS(structure.Position, structure.Rotation, Vector3.one) }; //structure.GetSmallGridBounds().size
            result.Colors = new List<Vector4> { color };
            return result;
        }

        public static void AddToBatch(Thing thing, Color color, ref List<ScannerMeshBatch> _batchList, Structure parent = null, Color colliderColor = default(Color))
        {
            int hash = thing.PrefabHash;
            if (thing is Plant plant)
            {
                hash += plant.Stage;
            }
            if (thing is Structure structure)
            {
                hash += structure.CurrentBuildStateIndex;
                if (colliderColor != default(Color))
                {
                    ScannerMeshBatch scannerMeshBatch = CreateColliderMesh(structure, colliderColor);
                    _batchList.Add(scannerMeshBatch);
                }
            }
            if (Batches.TryGetValue(hash, out var value))
            {
                value.Matrices.Add( GetMatrix(thing, parent) );
                value.Colors.Add(color);
            }
            else
            {
                ScannerMeshBatch scannerMeshBatch = CreateMesh(thing, color, parent);
                Batches.Add(hash, scannerMeshBatch);
                _batchList.Add(scannerMeshBatch);
            }
        }

        private static Matrix4x4 GetMatrix(Thing thing, Structure parent)
        {
            if (thing is Structure structure)
            {
                if (structure is Tank tank)
                {
                    return Matrix4x4.TRS(tank.Position + tank.Up, thing.Rotation, Vector3.one);
                }
                if (structure.PrefabHash == 1736080881) // Hangar door/gate
                {
                    return Matrix4x4.TRS(structure.Position - structure.Up, thing.Rotation, Vector3.one);
                }
                return  structure.GetBatchMatrix();
            }
            else
            {
                if (parent == null)
                {
                    return Matrix4x4.TRS(thing.Position, thing.Rotation, Vector3.one) ;
                }
                return parent.GetBatchMatrix();
            }
        }
    }
}