using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;


namespace Axolotl
{
    public class modShopSpawner : MonoBehaviour
    {
        private static string TargetSpanwerToken = "TargetHolder";
        private static string GatchaSpanwerToken = "GatchaSpawn";
        private static string UnlockSpanwerToken = "UnlockSpawn";

        private static AssetBundle bundle;
        private static GameObject targetPrefab;

        private static List<targetMultiShopController> targetControllers = new List<targetMultiShopController>();


        //This function spawns in all of the shops and thier necessary components.
        //If this is slow, consider remaking it using hard arrays instead of lists.
        public bool spawnShops()
        {
            bool error_flag = false;
            #region TargetSpawns
            GameObject[] foundLocations = getSpawnLocations(TargetSpanwerToken);
            List<Transform> shopTransforms = new List<Transform>();

            if (foundLocations != null)
            {
                foreach (var obj in foundLocations)
                {
                    if (obj == null)
                    {
                        Log.LogWarning(nameof(spawnShops) + ": Skipping null Object in spawn list.");
                    }
                    else
                    {
#if DEBUG
                        Log.LogInfo(nameof(spawnShops) + ": Attempting Spawn at: " + obj.transform.position.ToString());
#endif
                        shopTransforms.Add(obj.transform);
                    }
                    

                }
                foreach (Transform transform in shopTransforms)
                {
                    targetControllers.Add(new targetMultiShopController(transform.GetComponentsInChildren<Transform>(), targetPrefab));
                }
            }
            else
            {
                Log.LogError(nameof(spawnShops) + " Unable to spawn: Target Multishop Holders");
                error_flag = true;
            }
            #endregion
            return error_flag;
        }


        //This is a setup function which give the controller refs to
        //prefabs that will be spawned.
        public bool loadAssetBundle()
        {
            bool error_flag = false;
            Log.LogWarning(nameof(AxolotlStageController) + " " + nameof(loadAssetBundle) + ": Attempting to Loading AB at " + BepInEx.Paths.PluginPath + "/Axolotl/InteractablePrefabs");
            bundle = AssetBundle.LoadFromFile(BepInEx.Paths.PluginPath + "/Axolotl/InteractablePrefabs");
            if (bundle == null)
            {
                Log.LogError(nameof(AxolotlStageController) + " " + nameof(loadAssetBundle) + ": Unable to load InteractablePrefab Bundle");
                error_flag = true;
                return error_flag;
            }
            error_flag = loadAsset(bundle, error_flag, "TargetMultiShop", out targetPrefab);
            return error_flag;
        }

        public bool loadAsset(AssetBundle bundle, bool error_flag, string assetName, out GameObject prefab)
        {
            prefab = bundle.LoadAsset<GameObject>(assetName);
            if (prefab == null)
            {
                Log.LogError(nameof(AxolotlStageController) + " " + nameof(loadAssetBundle) + ": Unable to load asset.");
                error_flag = true;
            }
            return error_flag;
        }

        //This funciton Gets all of the game objects with the tag
        private GameObject[] getSpawnLocations(string regex)
        {
            var objects = GameObject.FindGameObjectsWithTag("Chest");
            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i].name.StartsWith(regex))
                {
                    list.Add(objects[i]);
                }
            }
            return list.ToArray();
        }
    }
}