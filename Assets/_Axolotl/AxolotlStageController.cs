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

        private static ModShopSpawner shopSpawner = new ModShopSpawner();
       
        public static bool loadStageAssets()
        {
            return shopSpawner.loadAssetBundle();
        }
        
        public static bool initializeAxolotlStage()
        {
            bool error_flag = false;
            shopSpawner.spawnShops();
            
            return error_flag;
        }

        
    }
}