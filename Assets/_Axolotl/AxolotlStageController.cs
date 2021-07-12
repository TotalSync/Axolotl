#define DEBUG
#define VERBOSE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;


namespace Axolotl
{
    public static class AxolotlStageController
    {
        private static AssetBundle bundle;
        private static GameObject DebugPrefab;
        private static GameObject SelectPrefab;
        private static targetMultiShopController target_controller;



        public static void setHooks()
        {
            Log.LogError(nameof(initializeAxolotlStage) + ": " + Stage.instance.sceneDef.cachedName);
            if (Stage.instance.sceneDef.cachedName == "ShipShopStage")
            {
                initializeAxolotlStage(Run.instance);
            }
        }

        public static bool loadAssetBundle()
        {
            bool error_flag = false;
            Log.LogWarning(nameof(AxolotlStageController) + " " + nameof(loadAssetBundle) + ": Attempting to Loading AB at " + BepInEx.Paths.PluginPath + "/Axolotl/InteractablePrefabs");
            bundle = AssetBundle.LoadFromFile(BepInEx.Paths.PluginPath + "/Axolotl/InteractablePrefabs");
            if (bundle == null)
            {
                Log.LogError(nameof(AxolotlStageController) + " " + nameof(loadAssetBundle) + ": Unable to load InteractablePrefab Bundle");
                return !error_flag;
            }
            DebugPrefab = bundle.LoadAsset<GameObject>("DebugPrefab");
            if (DebugPrefab == null)
            {
                Log.LogError(nameof(AxolotlStageController) + " " + nameof(loadAssetBundle) + ": Unable to load DebugPrefab.");
                error_flag = true;
            }
            SelectPrefab = bundle.LoadAsset<GameObject>("SelectiveMultiShop");
            if (SelectPrefab == null)
            {
                Log.LogError(nameof(AxolotlStageController) + " " + nameof(loadAssetBundle) + ": Unable to load SelectiveMultiShop.");
                error_flag = true;
            }
            return error_flag;
        }

        public static bool initializeAxolotlStage(Run run)
        {
            bool error_flag = false;
            GameObject[] foundLocations = getSpawnLocations("SelectChestSpawn");
            List<Transform> transforms = new List<Transform>();
            if (foundLocations != null)
            {
                foreach(var obj in foundLocations)
                {
                    if (obj == null)
                    {
                        Log.LogWarning(nameof(initializeAxolotlStage) + ": Skipping null Object in spawn list.");
                    }
                    /*else
                    {
                        transforms.Add(obj.transform);
                    }*/
                    
                    else
                    {
                        var objt = GameObject.Instantiate(DebugPrefab, obj.transform);
                        if (objt == null)
                        {
                            Log.LogError(nameof(initializeAxolotlStage) + ": Unable to spawn object at location.");
                        }
                        else
                        {
                            objt.SetActive(true);

                            Log.LogInfo(nameof(initializeAxolotlStage) + ": Spawned Chest at Location");
                        }
                    }
                    
                }
                
                target_controller = new targetMultiShopController(transforms.ToArray());
            } 
            else 
            {
                Log.LogError(nameof(initializeAxolotlStage) + " Unable to spawn: Target Multishops");
                error_flag = true;
            }
            
            return error_flag;
        }

        private static GameObject[] getSpawnLocations(string regex)
        {
            var objects = GameObject.FindGameObjectsWithTag("Chest");
            for (int i = 0; i < objects.Length; i++)
            {
                if (!objects[i].name.StartsWith(regex))
                {
                    objects[i] = null;
                }
            }
            return objects;
        }
    }
}