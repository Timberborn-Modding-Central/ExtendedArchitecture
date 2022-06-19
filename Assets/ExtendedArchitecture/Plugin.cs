using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using Bindito.Core;
using Timberborn.AssetSystem;
using Timberborn.SingletonSystem;
using TimberbornAPI;
using TimberbornAPI.AssetLoaderSystem.AssetSystem;
using TimberbornAPI.Common;
using TimberbornAPI.LocalizationSystem;
using UnityEngine;
using Logger = UnityEngine.Logger;

namespace Elec.ExtendedArchitecture
{
    [BepInPlugin(PluginGuid, PluginGuid, PluginVersion)]
    [BepInDependency("com.timberapi.timberapi")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "Elec.ExtendedArchitecture";
        public const string PluginVersion = "0.3.0";

        public static ManualLogSource Log; 
        
        private void Awake()
        {
            Log = Logger;
            
            TimberAPI.AssetRegistry.AddSceneAssets(PluginGuid, SceneEntryPoint.Global);
            
            TimberAPI.DependencyRegistry.AddConfiguratorBeforeLoad(new BuildingAddConfigurator(), SceneEntryPoint.MainMenu);
            
            Log.LogInfo($"Loaded {PluginGuid} {PluginVersion}!");
        }
    }

    public class BuildingAdd : IInitializableSingleton
    {
        private readonly IAssetLoader _assetLoader;
        private readonly IResourceAssetLoader _resourceAssetLoader;
        
        public BuildingAdd(IAssetLoader assetLoader, IResourceAssetLoader resourceAssetLoader)
        {
            _assetLoader = assetLoader;
            _resourceAssetLoader = resourceAssetLoader;
        }
        
        public void Initialize()
        {
            var platformModel = _resourceAssetLoader.Load<GameObject>("Buildings/Paths/Platform/Platform.Full.Folktails");
            var shader = platformModel.GetComponent<MeshRenderer>().materials[0].shader;

            AddBuilding("4x1arch", shader);
            AddBuilding("5x1arch", shader);
            
            Plugin.Log.LogInfo($"Loaded buildings!");
        }

        private void AddBuilding(String name, Shader shader)
        {
            var building = _assetLoader.Load<GameObject>(Plugin.PluginGuid, $"elec.extendedarchitecture.bundle/{name}");
            
            FixMaterialShader(building, shader);
            TimberAPI.CustomObjectRegistry.AddGameObject(building);
        }
        
        private static void FixMaterialShader(GameObject obj, Shader shader)
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
    }

    public class BuildingAddConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<BuildingAdd>().AsSingleton();
        }
    }
}
