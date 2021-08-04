#define DEBUG   // Basic Debugging Mode.
#define VERBOSE // Toggles Verbose print statements.

using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Security.Permissions;


using Path = System.IO.Path;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]


/*
 Main TODO List:

   1.) Rework LangAPI meshing for everything to mesh with the given convention in ThunderHenry.
   2.) Review code and strip excess.
      2.1) Add regioning to force code folding for long areas (IDRs)
   3.) Integrate item balance values into the cfg.

   * Setup Wwise. (Sound scaping things)
 */


namespace Axolotl
{
   [BepInDependency("com.bepis.r2api")]
   [BepInDependency("com.valex.ShaderConverter", BepInDependency.DependencyFlags.HardDependency)]
   [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
   //This attribute is required, and lists metadata for your plugin.
   [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
   [R2APISubmoduleDependency(nameof(ItemAPI), nameof(ItemDropAPI), nameof(LanguageAPI), nameof(RecalculateStatsAPI))]

   public class AxolotlShop : BaseUnityPlugin
   {
      //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
      public const string PluginGUID = PluginAuthor + "." + PluginName;
      public const string PluginAuthor = "Sync";
      public const string PluginName = "Axolotl";
      public const string PluginVersion = "0.0.1";

		//We need our item definition to persist through our functions.
		#region ItemDefs
		private static star_glass star_glass_def;
      private static greedy_milk greedy_milk_def;
      private static tele_battery tele_battery_def;
      private static popato_chisps popato_chisps_def;
      private List<Item_Base> item_list = new List<Item_Base>();
		#endregion

		public void Awake()
      {
			#region Debugging & Tool Initialization
			Log.Init(Logger);       //Init our logger
         ConfigHandler config = new ConfigHandler(); //Init our config handler.

         //This Loads our content packs from the Unity Objects.
         //They only load the items, as the stages need to be in a seperate asset bundle.
         //When we add stages, we will need to modify this.
         Assets.PopulateAssets();
         ContentPackProvider.init();
			#endregion

			#region CharacterInit

			//Axolotl.Tanker.TankerPlugin.instance = new Tanker.TankerPlugin();
			//Axolotl.Tanker.TankerPlugin.instance.InitCharacter();


			#endregion

			#region ItemLoading

			var obj = new object();
         var item_def = ScriptableObject.CreateInstance<ItemDef>();

         //These functions try to load our custom items.
         #region  ItemInits

         if (tryLoadItemDef("greedy_milk", out obj))
         {
            item_def = (ItemDef)obj;
            greedy_milk_def = new greedy_milk((ItemDef)item_def);
            greedy_milk_def.initialize();
            item_list.Add(greedy_milk_def);
         }
         if (tryLoadItemDef("star_glass", out obj))
         {
            item_def = (ItemDef)obj;
            star_glass_def = new star_glass((ItemDef)item_def);
            star_glass_def.initialize();
            item_list.Add(star_glass_def);
         }
         if (tryLoadItemDef("tele_battery", out obj))
         {
            item_def = (ItemDef)obj;
            tele_battery_def = new tele_battery((ItemDef)item_def);
            tele_battery_def.initialize();
            item_list.Add(tele_battery_def);
         }
         if (tryLoadItemDef("popato_chisps", out obj))
         {
            item_def = (ItemDef)obj;
            popato_chisps_def = new popato_chisps((ItemDef)item_def);
            popato_chisps_def.initialize();
            item_list.Add(popato_chisps_def);
         }
		   #endregion
			var item_ban_list = config.loadConfig(item_list);

         //This loop does a few things. It loops through all of the items in our content pack.
         //If there is an item that is banned (disabled in the config), it does not initialize the item.
         //If the item does not have an IDR, then it will initialize the item with an empty IDR.
         foreach (var item in ContentPackProvider.serializedContentPack.itemDefs)
         {
            Log.LogInfo("Finding: " + item.nameToken);
            var ind = item_list.FindIndex(x =>
#if (DEBUG && VERBOSE)
               Log.printItemBool(x) && //This spits out a verbose loop of all items attempted to load for IDRs.
#endif
               x.id == item.nameToken);
            var ban = item_ban_list.FindIndex(x => x == item.nameToken);
            if (ban == -1)
            {
                if (ind == -1 || item_list[ind].id != item.nameToken)
                {
                    Log.LogWarning(nameof(Awake) + " Item Add Loop " + ": " + item.nameToken + " No IDR defined, or item is not in list.");
                    Log.LogWarning("Index: " + ind + ". If this number is negative, then all is working normal.");
                    ItemAPI.Add(new CustomItem(item, (ItemDisplayRuleDict)null));
                }
                else
                {
                    ItemAPI.Add(new CustomItem(item, item_list[ind].idr));
                    //cItem.langInit();
                }
            }
            else
            {
                Log.LogMessage("Skipping " + item.nameToken + " because it was banned in the config.");
            }
         }
			#endregion

			//This sets our hooks for our stage when it loads.
			Log.LogDebug(nameof(AxolotlStageController) + " " + nameof(Awake) + ": Setting Stage hook.");

			#region SystemHooks

         //This loads the assets needed to initialize the stage.
         if (AxolotlStageController.loadStageAssets())
         {
            Log.LogFatal(nameof(AxolotlStageController.loadStageAssets) + ": There was an error loading the stage assets.");
         }

         //This initializes the drop tables to a 0 value for use later.
         selectiveDropTableController.initializeDropTables();
			
         //This Hook checks to see if the stage name is correct.
			//If it is correct, it generates the stage.
			Stage.onStageStartGlobal += (stage) =>
         {
            Log.LogDebug(nameof(AxolotlStageController.initializeAxolotlStage) + ": " + stage.sceneDef.cachedName);
            if (stage.sceneDef.cachedName == "ShipShopStage")
            {
                AxolotlStageController.initializeAxolotlStage();
            }
         };

         //This Hook sets the drop table generation to the start of the run.
         Run.onRunStartGlobal += (run) =>
         {
            selectiveDropTableController.generateDropTables();
         };
			#endregion

			Log.LogInfo(nameof(Awake) + ": " + PluginName +" has Loaded.");
         return;
      }

       //Currently this is just an item spawner.
      private void Update()
      {
#if DEBUG
         if (Input.GetKeyDown(KeyCode.F2))
         {
            //Get the player body to use a position:	
            var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

            //And then drop our defined item in front of the player.
            Log.LogInfo($"Player pressed F4. Spawning our custom item at coordinates {transform.position}");
            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(popato_chisps_def.item_def.itemIndex), transform.position, transform.forward * 20f);
         }
         //This if statement checks if the player has currently pressed F2.
         if (Input.GetKeyDown(KeyCode.F3))
         {
            //Get the player body to use a position:	
            var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

            //And then drop our defined item in front of the player.
            Log.LogInfo($"Player pressed F3. Spawning our custom item at coordinates {transform.position}");
            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(greedy_milk_def.item_def.itemIndex), transform.position, transform.forward * 20f);
         }
         if (Input.GetKeyDown(KeyCode.F4))
         {
            //Get the player body to use a position:	
            var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

            //And then drop our defined item in front of the player.
            Log.LogInfo($"Player pressed F4. Spawning our custom item at coordinates {transform.position}");
            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(star_glass_def.item_def.itemIndex), transform.position, transform.forward * 20f);
         }
#endif
      }

       //This function acts as a wrapper for some logic and debugging.
       //@param name is the name of the item attempting to be found. It is the name of the itemDef in Unity.
       //@param item_def is the itemDef that was found by the search.
       public bool tryLoadItemDef(string name, out object item_def)
       {
           if (ContentPackProvider.contentPack.itemDefs.Find(name, out item_def))
           {
               Log.LogInfo(nameof(tryLoadItemDef) + ": Found " + name + " itemDef.");
               return true;
           }
           else
           {
               Log.LogError(nameof(Awake) + ": Could not find" + name + " ItemDefs");
               return false;
           }
       }

		//Asset Bundle Loading Classes. A similar class is in ThunderHenry
		#region AssetBundle
		public static class Assets
       {
           public static AssetBundle mainAssetBundle = null;
           //the filename of your assetbundle
           internal static string assetBundleName = "axolotl_bundle";

           public static void PopulateAssets()
           {
               var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
               mainAssetBundle = AssetBundle.LoadFromFile(Path.Combine(path, assetBundleName));
               ContentPackProvider.serializedContentPack = mainAssetBundle.LoadAsset<SerializableContentPack>(ContentPackProvider.contentPackName);
           }
       }

       public class ContentPackProvider : IContentPackProvider
       {
           public static SerializableContentPack serializedContentPack;
           public static ContentPack contentPack;
           //Should be the same names as your SerializableContentPack in the asset bundle
           public static string contentPackName = "AxolotlSerializableContentPack";

           public string identifier
           {
               get
               {
                   //If I see this name while loading a mod I will make fun of you
                   return "Axolotl";
               }
           }

           internal static void init()
           {
               contentPack = serializedContentPack.CreateContentPack();
           }

           internal static void setHook()
           {
               ContentManager.collectContentPackProviders += AddCustomContent;
           }

           private static void AddCustomContent(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
           {
               addContentPackProvider(new ContentPackProvider());
           }

           public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
           {
               args.ReportProgress(1f);
               yield break;
           }

           public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
           {
               ContentPack.Copy(contentPack, args.output);
               args.ReportProgress(1f);
               yield break;
           }

           public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
           {
               args.ReportProgress(1f);
               yield break;
           }
       }
		#endregion
	}
}
