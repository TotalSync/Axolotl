using BepInEx;  
using R2API;
using R2API.Utils;
using RoR2;
using System.Collections;
using UnityEngine;

namespace Axolotl
{
   [BepInDependency("com.bepis.r2api")]
   [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(ItemAPI))]
   public class greedy_milk : Item_Base
   {
      private float milk_level_health_gain = 5.0f;
      private float milk_inital_level_health = 0.0f;
      private bool milk_have_inital_level_health = false;

      public greedy_milk(ItemDef item_def) 
      {
         this.item_def = item_def;
         id = item_def.nameToken.ToUpper();
         idr = new ItemDisplayRuleDict();
         name_long = "Greedy Milk";
         pickup_long = "Increases <style=cIsHealth>Max Health</style> per level by <style=cIsHealth>+5</style> <style=cStack>(+5 per stack)"
                                 + "</style> for every level past level 1. <style=cIsUtility>This works retroactively.</style>";
         desc_long = "Increases <style=cIsHealth>Max Health</style> per level by <style=cIsHealth>+5</style> <style=cStack>(+5 per stack)"
                                 + "</style> for every level past level 1. <style=cIsUtility>This works retroactively.</style>";
         lore_long = "A sargent in the nearby space fleet was known for acts of arson to whatever planet he laid eyes upon.\n"
                                  + "Its was said that his favorite tool was something he called his greedy milk. Many children felt the burn of this cruel substence.\n";
      }
		private void milkHealthUpdate(CharacterBody body)
      {
         if (body != null)
         {
             if (body.inventory != null)
             { 
                 int item_count = body.inventory.GetItemCount(this.item_def);
                 if (item_count != 0)
                 {
                     if (milk_have_inital_level_health)
                     {
                         body.levelMaxHealth = (milk_level_health_gain * item_count) + milk_inital_level_health;
                     }
                     else
                     {
                         milk_have_inital_level_health = true;
                         milk_inital_level_health = body.levelMaxHealth;
                         body.levelMaxHealth = (milk_level_health_gain * item_count) + milk_inital_level_health;
                     }
                 }
             }
         }
         return;
      }

		#region Setup
		public override void setIDR()
        {
            GameObject ItemBodyModelPrefab = AxolotlShop.ContentPackProvider.contentPack.itemDefs.Find("greedy_milk").pickupModelPrefab;
            if (ItemBodyModelPrefab == null) {
                Log.LogError(nameof(setIDR) + ": " + nameof(greedy_milk) + " ModelPrefab broke.");
            } 
            else
            {
				#region IDRs
				ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
                idr.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "HandR",
                        localPos = new Vector3(-0.19528F, 0.07389F, -0.17651F),
                        localAngles = new Vector3(58.02077F, 167.347F, 93.26147F),
                        localScale = new Vector3(0.05F, 0.05F, 0.05F)
                    }
                });
                idr.Add("mdlHuntress", new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "Head",
                        localPos = new Vector3(-0.18595F, -0.00084F, -0.13336F),
                        localAngles = new Vector3(349.5154F, 249.8436F, 356.0982F),
                        localScale = new Vector3(0.05F, 0.05F, 0.05F)
                    }
                });
                idr.Add("mdlToolbot", new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "Head",
                        localPos = new Vector3(-2.52397F, 1.23269F, -1.26629F),
                        localAngles = new Vector3(315.5635F, 233.7695F, 325.0397F),
                        localScale = new Vector3(0.4F, 0.4F, 0.4F)
                    }
                });
                idr.Add("mdlEngi", new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "CannonHeadR",
                        localPos = new Vector3(0.00026F, 0.24843F, -0.0043F),
                        localAngles = new Vector3(0F, 45F, 0F),
                        localScale = new Vector3(0.05F, 0.05F, 0.05F)
                    }
                });
                idr.Add("mdlMage", new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "Head",
                        localPos = new Vector3(-0.00767F, 0.27467F, -0.47475F),
                        localAngles = new Vector3(65.67441F, 179.3688F, 178.9807F),
                        localScale = new Vector3(0.05F, 0.05F, 0.05F)
                    }
                });
                idr.Add("mdlMerc", new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "Chest",
                        localPos = new Vector3(0.00502F, 0.25654F, -0.58028F),
                        localAngles = new Vector3(43.23627F, 103.2376F, 110.1412F),
                        localScale = new Vector3(0.05F, 0.05F, 0.05F)
                    }
                });
                idr.Add("mdlTreebot", new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "PlatformBase",
                        localPos = new Vector3(-0.68894F, 1.66687F, -0.68098F),
                        localAngles = new Vector3(0F, 0F, 270.8968F),
                        localScale = new Vector3(0.2F, 0.2F, 0.2F)
                    }
                });
                idr.Add("mdlLoader", new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "ThighL",
                        localPos = new Vector3(-0.10642F, 0.50676F, -0.20183F),
                        localAngles = new Vector3(21.1155F, 187.0388F, 129.261F),
                        localScale = new Vector3(0.05F, 0.05F, 0.05F)
                    }
                });
                idr.Add("mdlCroco", new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "Chest",
                        localPos = new Vector3(0.92246F, 1.72773F, 3.78736F),
                        localAngles = new Vector3(46.47424F, 0.03554F, 0.12109F),
                        localScale = new Vector3(0.4F, 0.4F, 0.4F)
                    }
                });
                idr.Add("mdlCaptain", new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "Head",
                        localPos = new Vector3(-0.14419F, 0.43071F, -0.07489F),
                        localAngles = new Vector3(351.6593F, 184.1165F, 180.3262F),
                        localScale = new Vector3(0.05F, 0.05F, 0.05F)
                    }
                });
                idr.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
                {
                    new RoR2.ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "Pelvis",
                        localPos = new Vector3(0.33909F, 0.14138F, 0.14368F),
                        localAngles = new Vector3(17.62127F, 109.7287F, 208.814F),
                        localScale = new Vector3(0.05F, 0.05F, 0.05F)
                    }
                });
				#endregion
				//Log.LogInfo(nameof(greedy_milk) + nameof(setIDR) + " new IDR data set.");
			}
			//Log.LogError(nameof(setIDR) + ": Could not set new IDR.");
		}

      public override void SetHooks()
        {
            //Function Hook for Greedy Milk
            On.RoR2.CharacterBody.RecalculateStats += (orig, self) =>
            {
                if (self != null)
                {
                    milkHealthUpdate(self);
                    orig(self);
                }
            };
        }

      public override void langInit()
        {
            LanguageAPI.Add(this.id, this.name_long);
            LanguageAPI.Add(this.id + "_PICKUP", this.pickup_long);
            LanguageAPI.Add(this.id + "_DESC", this.desc_long);
            LanguageAPI.Add(this.id + "_LORE", this.lore_long);
            //Log.LogError(nameof(greedy_milk) + nameof(langInit) + "This funtion ran");
        }

		#endregion
   }
}
