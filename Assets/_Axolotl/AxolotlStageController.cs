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

        private static modShopSpawner shopSpawner;



        //This function sets up a check at the begining of each stage to see if
        //The stage cachedName is our stage.
        /*public static void setHooks()
        {
            Log.LogDebug(nameof(initializeAxolotlStage) + ": " + Stage.instance.sceneDef.cachedName);
            if (Stage.instance.sceneDef.cachedName == "ShipShopStage")
            {
                initializeAxolotlStage();
            }
        }*/

        //Redirect for loading asset bundles.
        public static bool loadAssetBundle()
        {
            shopSpawner = new modShopSpawner();
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