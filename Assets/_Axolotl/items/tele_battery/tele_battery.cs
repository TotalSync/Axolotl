using UnityEngine;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;

namespace Axolotl
{
   [BepInDependency("com.bepis.r2api")]
   [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(ItemAPI), nameof(PrefabAPI))]
   public class tele_battery : Item_Base
   {
      public GameObject prefab;
      private float tele_duration_offset = 0.1f;


      public tele_battery(ItemDef item_def)
      {
          base.item_def = item_def;
          base.id = item_def.nameToken.ToUpper();
          base.idr = new ItemDisplayRuleDict();
          base.name_long = "Teleporter Battery";
          base.pickup_long = "Charges the teleport by <style=cIsUtility>10%</style> <style=cStack>(Stacks Logrithmically)"
                              + "</style> when it starts. <style=cIsUtility> It counts all players' batteries.</style>";
          base.desc_long = "Charges the teleport by <style=cIsUtility>10%</style> <style=cStack>(Stacks Logrithmically)"
                              + "</style> when it starts. <style=cIsUtility> It counts all players' batteries.</style>";
          base.lore_long = " ";
          //Log.LogError(nameof(star_glass) + "This funtion ran");
      }

      private bool preChargeTele(TeleporterInteraction tele)
        {
            int sum = 0;
            foreach(var player in PlayerCharacterMasterController.instances) { 
            
                if (!player.master.IsDeadAndOutOfLivesServer())
                {
                    var body = player.master.GetBody();
                    if(body != null)
                    {
                        var count = body.inventory.GetItemCount(this.item_def);
                        if (count > 0)
                        {
                            Log.LogDebug(nameof(preChargeTele) + ": " + count + " number of batteries found on a player");
                            sum += count;
                        }
                    }
                }
            }
            Log.LogDebug(nameof(preChargeTele) + ": " + sum + "Batteries found in total.");
            if (sum > 0)
            {
                tele.holdoutZoneController.Network_charge += (Mathf.Log10(sum)/2.5f) + tele_duration_offset;
                return true;
            }
            else
            {
                return false;
            }
        }

      private void spawnItemDisplay()
        {
            var obj = GameObject.Find("TeleporterBeacon");
            if (obj != null)
            {
                var prefab = Instantiate(AxolotlShop.ContentPackProvider.contentPack.itemDefs.Find("tele_battery").pickupModelPrefab, obj.transform);
                prefab.transform.rotation *= Quaternion.Euler(new Vector3(-90.0f, 0.0f, 75.0f));
                prefab.transform.position += new Vector3(-2.38f, -0.85f, -4.84f);
                prefab.transform.localScale /= prefab.transform.lossyScale.magnitude;
                prefab.transform.localScale *= 100.0f;
                Log.LogDebug(nameof(spawnItemDisplay) + ": teleporter origin at ( " + obj.transform.position.ToString() + " )");
                Log.LogDebug(nameof(spawnItemDisplay) + ": spawning display at ( " + prefab.transform.position.ToString() + " )");
                prefab.SetActive(true);
            } else
            {
                Log.LogError(nameof(spawnItemDisplay) + ": Unable to Find Teleporter");
            }
        }

		#region Setup
		public override void setIDR()
        {
            GameObject ItemBodyModelPrefab = AxolotlShop.ContentPackProvider.contentPack.itemDefs.Find("tele_battery").pickupModelPrefab;
            if (ItemBodyModelPrefab == null)
            {
                Log.LogError(nameof(setIDR) + ": " + nameof(greedy_milk) + " ModelPrefab broke.");
            } else
            {
                ItemBodyModelPrefab.transform.localScale /= ItemBodyModelPrefab.transform.lossyScale.magnitude;
                ItemBodyModelPrefab.transform.rotation *= Quaternion.Euler(new Vector3(90, 0, 0));
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
            }
        }

      public override void langInit()
        {
            LanguageAPI.Add(this.id, this.name_long);
            LanguageAPI.Add(this.id + "_PICKUP", this.pickup_long);
            LanguageAPI.Add(this.id + "_DESC", this.desc_long);
            LanguageAPI.Add(this.id + "_LORE", this.lore_long);
        }
      
      public override void SetHooks()
        {
            On.RoR2.TeleporterInteraction.OnInteractionBegin += (orig, self, action) => 
            {
                bool found = preChargeTele(self);
                if (found)
                {
                    Log.LogDebug("Batteries Found. Spawning IDR.");
                    spawnItemDisplay();
                }
                orig(self, action);
            };
        }

		#endregion

	}


}
