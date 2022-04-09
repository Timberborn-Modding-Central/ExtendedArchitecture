using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Timberborn.AssetSystem;
using Timberborn.BlockSystem;
using Timberborn.BlockSystemNavigation;
using Timberborn.Buildings;
using Timberborn.EntitySystem;
using Timberborn.FactionSystemGame;
using Timberborn.Goods;
using Timberborn.Localization;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;

namespace Elec.ExtendedArchitecture
{
    [BepInPlugin("Elec.ExtendedArchitecture", "Extended Architecture", "0.2.0")]
    public class Plugin : BaseUnityPlugin
    {
        private static bool _hasLoaded = false;
        private static AssetBundle _assetBundle;
        private void Awake()
        {
            _assetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "elec.extendedarchitecture.bundle"));
            if (_assetBundle == null) throw new Exception("AssetBundle not found!");
            
            Harmony.CreateAndPatchAll(typeof(Plugin));
            
            Logger.LogInfo($"Loaded!");
        }

        static void FixMaterialShader(GameObject obj, Shader shader)
        {
            var meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer)
            {
                foreach (var mat in meshRenderer.materials)
                {
                    mat.shader = shader;
                }
            }

            foreach (Transform child in obj.transform)
            {
                if (child.gameObject)
                {
                    FixMaterialShader(child.gameObject, shader);
                }
            }
        }

        private static Dictionary<string, GameObject> _moddedBuildings = new();

        static void AddBuilding(string path, Shader shader, GoodAmount[] goodAmounts)
        {
            var building = _assetBundle.LoadAsset<GameObject>(path);
            var prefabName = building.GetComponent<Prefab>().PrefabName;
            building.GetComponent<Building>().BuildingCost = goodAmounts;
            FixMaterialShader(building, shader);
            LoadLocalization(building);
            _moddedBuildings.Add(prefabName, building);
        }

        [HarmonyPatch(typeof(FactionObjectCollection), "GetObjects")]
        [HarmonyPostfix]
        static void FactionObjectCollection_GetObjects_Postfix(ref IEnumerable<Object> __result, FactionObjectCollection __instance)
        {
            if (!_hasLoaded)
            {
                var assetLoaderInfo = typeof(FactionObjectCollection).GetField("_resourceAssetLoader", 
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );
                if (assetLoaderInfo == null)
                {
                    throw new Exception("_resourceAssetLoader doesn't exist!");
                }
                var assetLoader = (IResourceAssetLoader) assetLoaderInfo.GetValue(__instance);

                var platformModel = assetLoader.Load<GameObject>("Buildings/Paths/Platform/Platform.Full.Folktails");
                var shader = platformModel.GetComponent<MeshRenderer>().materials[0].shader;

                var logSpec = assetLoader.Load<GoodSpecification>("Goods/LogSpecification");
                var plankSpec = assetLoader.Load<GoodSpecification>("Goods/PlankSpecification");
                var metalSpec = assetLoader.Load<GoodSpecification>("Goods/MetalBlockSpecification");

                AddBuilding("4x1Arch", shader,
                    new []{
                        new GoodAmount(logSpec, 8), 
                        new GoodAmount(plankSpec, 8)
                    } );
                AddBuilding("5x1Arch", shader,
                    new []{
                        new GoodAmount(logSpec, 4), 
                        new GoodAmount(plankSpec, 10),
                        new GoodAmount(metalSpec, 6)
                    } );
                AddBuilding("Stairwell", shader, new GoodAmount[0] );

                _hasLoaded = true;
            }
            __result = __result.Concat(_moddedBuildings.Values);
        }

        [HarmonyPatch(typeof(PrefabNameMapper), "GetPrefab", new [] {typeof(string)})]
        [HarmonyPrefix]
        static bool PrefabNameMapper_GetPrefab_Prefix(ref GameObject __result, string prefabName)
        {
            return !_moddedBuildings.TryGetValue(prefabName, out __result);
        }
        
        private static Dictionary<string, string> _moddedLocalization = new();
        
        [HarmonyPatch(typeof(Loc), "T", new [] {typeof(string)})]
        [HarmonyPrefix]
        static bool Loc_T_Prefix(ref string __result, string key)
        {
            return !_moddedLocalization.TryGetValue(key, out __result);
        }

        static void LoadLocalization(GameObject obj)
        {
            var localization = obj.GetComponent<Localization>();
            if (localization == null) return;
            
            _moddedLocalization.Add(localization.DisplayNameLocKey, localization.displayNameText);
            _moddedLocalization.Add(localization.DescriptionLocKey, localization.descriptionText);
            _moddedLocalization.Add(localization.FlavorDescriptionLocKey, localization.flavorText);
        }
    }
}
