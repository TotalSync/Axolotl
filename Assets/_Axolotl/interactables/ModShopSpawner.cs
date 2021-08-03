using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;

/*
 The idea behind this class is for it to function as a Director-Lite. I want to force certain items to spawn in certain areas
on the map. Because of this, it is easier to disable the director for this stage and to spawn in items manually.

Flow:
   1.) Load Assets and Prefabs
   2.) Find Locations that match SpawnerString
   3.) Spawn objects at target location parenting the object to the spawnobject 
 */



namespace Axolotl
{
   public class ModShopSpawner : MonoBehaviour
   {
      private static readonly string TargetSpawnerToken = "TargetSpawn";
      private static readonly string GatchaSpawnerToken = "GatchaSpawn";
      private static readonly string UnlockSpawnerToken = "UnlockSpawn";
   
      private static readonly string TargetHolderToken = "TargetMultiHolder";
      private static readonly string GatchaHolderToken = "GatchaHolder";
      
   
      private static AssetBundle bundle;
      private static GameObject targetSpawnPrefab;
      private static GameObject gatchaSpawnPrefab;
   
      private static List<GameObject> targetControllers = new List<GameObject>();
      private static List<GameObject> gatchaControllers = new List<GameObject>();


      //This function spawns in all of the shops and thier necessary components.
      //If this is slow, consider remaking it using hard arrays instead of lists.
      public bool spawnShops()
      {
         bool error_flag = false;
         
         error_flag = AttemptSpawnShop(TargetSpawnerToken, targetSpawnPrefab, targetControllers);
         error_flag = AttemptSpawnShop(GatchaSpawnerToken, gatchaSpawnPrefab, gatchaControllers);

			return error_flag;
      }


		#region SetupFunctions
		//This is a setup function which give the controller refs to
		//prefabs that will be spawned.
		public bool loadAssetBundle()
      {
         bool error_flag = false;
         Log.LogWarning(nameof(AxolotlStageController) + " " + nameof(loadAssetBundle) + ": Attempting to Loading AB at" + BepInEx.Paths.PluginPath + "/Axolotl/InteractablePrefabs");
         bundle = AssetBundle.LoadFromFile(BepInEx.Paths.PluginPath + "/Axolotl/InteractablePrefabs");
         if (bundle == null)
         {
             Log.LogError(nameof(AxolotlStageController) + " " + nameof(loadAssetBundle) + ": Unable to load InteractablePrefab Bundle");
             error_flag = true;
             return error_flag;
         }
         error_flag = loadAsset(bundle, error_flag, TargetHolderToken, out targetSpawnPrefab);
         error_flag = loadAsset(bundle, error_flag, GatchaHolderToken, out gatchaSpawnPrefab);

         return error_flag;
      }
   
      public bool loadAsset(AssetBundle bundle, bool error_flag, string assetName, out GameObject prefab)
      {
          prefab = bundle.LoadAsset<GameObject>(assetName);
          if (prefab == null)
          {
              Log.LogError(nameof(AxolotlStageController) + " " + nameof(loadAssetBundle) + ": Unable to load asset " + assetName);
              error_flag = true;
          }
          return error_flag;
      }
		#endregion

		#region SpawningFunctions
		private bool AttemptSpawnShop(string spawnerToken, GameObject prefab, List<GameObject> controllerList)
		{
         bool error_flag = false;

         GameObject[] foundLocations = getSpawnLocations(spawnerToken);
         if (foundLocations != null)
         {
            foreach (var location in foundLocations)
            {
               Log.LogInfo("Attempting Spawn at: " + location.transform.position.ToString());
               
               var gameObject = UnityEngine.GameObject.Instantiate<GameObject>(prefab, location.transform);
               controllerList.Add(gameObject);
            }
         }
         else
         {
            Log.LogError(nameof(AttemptSpawnShop) + " Unable to spawn: " + spawnerToken + ". No Locations Found.");
            error_flag = true;
         }

         Log.LogWarning("Spawned " + controllerList.Count + " Objects at " + spawnerToken);

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
		#endregion
	}
}