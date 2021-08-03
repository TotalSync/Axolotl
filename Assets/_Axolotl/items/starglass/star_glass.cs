using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace Axolotl
{
   [BepInDependency("com.bepis.r2api")]
   [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(RecalculateStatsAPI))]
   public class star_glass : Item_Base
   {
      //These items are counter items to help keep track of require stats
      private static ItemDef star_glass_kills;
      private static ItemDef star_glass_dmg;

      private int star_kill_threshold = 10;
      private float star_kill_bonus = .1f;


      public star_glass(ItemDef item_def)
      {
          base.item_def = item_def;
          base.id = item_def.nameToken.ToUpper();
          base.idr = new ItemDisplayRuleDict();
          this.name_long = base.name_long = "Star Glass";
          base.pickup_long = "Kills increase <style=cIsDamage>base damage</style> by <style=cIsDamage>0.1</style> <style=cStack>(+0.1 per stack)</style> for every <style=cIsDamage>10enemieskilled.</stlye>";
          base.desc_long = "Kills increase <style=cIsDamage>base damage</style> by <style=cIsDamage>0.1</style> <style=cStack>(+0.1 per stack)</style> for every <style=cIsDamage>10 enemieskilled.<stlye>";
          base.lore_long = "The remains of a celesitial body which entered the atmosphere of a distant planet. Many would kill for this. Its luster and beauty are"
                          + " hard to compare. Its no wonder the stories of pointed to these objects as ones of great power. Listening close to it you can hear a "
                          + "faint hum. The people of old believed that consuming these fallen stars brought unimaginable power. Little did they know of its impact...";
      }
      private void glassUpdateDamage(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (body == null || body.inventory == null)
            {
                return;
            }
            else
            {
                int item_count = body.inventory.GetItemCount(this.item_def);
                int dmg_count = body.inventory.GetItemCount(star_glass_dmg);
                args.baseDamageAdd += (star_kill_bonus * dmg_count * item_count);
            }
            return;
        }
      private void glassUpdateCount(DamageReport report)
        {
            if (!report.attacker || !report.attackerBody)
            {
                return;
            }
            int count = report.attackerBody.inventory.GetItemCount(this.item_def);
            if (count != 0)
            {
                report.attackerBody.inventory.GiveItem(star_glass_kills);
                int kill_count = report.attackerBody.inventory.GetItemCount(star_glass_kills);
                if (kill_count >= star_kill_threshold)
                {
                    report.attackerBody.inventory.GiveItem(star_glass_dmg);
                    report.attackerBody.inventory.RemoveItem(star_glass_kills, 10);
                }
            }
        }

		#region Setup

		public override void SetHooks()
        {
            object item_def = new object();
            if (AxolotlShop.ContentPackProvider.contentPack.itemDefs.Find("star_glass_dmg", out item_def))
            {
                star_glass_dmg = ((ItemDef)item_def);
                if (AxolotlShop.ContentPackProvider.contentPack.itemDefs.Find("star_glass_kills", out item_def))
                {
                    star_glass_kills = ((ItemDef)item_def);
                    //Function Hook for Star Glass
                    R2API.RecalculateStatsAPI.GetStatCoefficients += (body, args) =>
                    {
                        glassUpdateDamage(body, args);
                    };

                    RoR2.GlobalEventManager.onCharacterDeathGlobal += (report) =>
                    {
                        glassUpdateCount(report);
                    };
                }
            }

        }

      public override void setIDR()
      {
         GameObject ItemBodyModelPrefab = AxolotlShop.ContentPackProvider.contentPack.itemDefs.Find("star_glass").pickupModelPrefab;
         if (ItemBodyModelPrefab == null)
         {
             Log.LogError(nameof(setIDR) + ": " + nameof(star_glass) + " ModelPrefab broke.");
         }
         else
         {
            ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
			   #region IDRs
				idr.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "CalfL",
                        localPos = new Vector3(-0.00101F, 0.0023F, 0.02834F),
                        localAngles = new Vector3(0F, 0F, 0F),
                        localScale = new Vector3(0.08F, 0.08F, 0.08F)
                    }
                });
                idr.Add("mdlHuntress", new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "Muzzle",
                        localPos = new Vector3(-0.61191F, 0.00489F, -0.55067F),
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
                        childName = "RightArmController",
                        localPos = new Vector3(0.05462F, 4.05324F, -0.08729F),
                        localAngles = new Vector3(315.5635F, 233.7695F, 325.0397F),
                        localScale = new Vector3(0.6F, 0.6F, 0.6F)
                    }
                });
                idr.Add("mdlEngi", new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "CannonHeadL",
                        localPos = new Vector3(0.03073F, 0.29752F, 0.52147F),
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
                        childName = "CalfL",
                        localPos = new Vector3(0.00904F, 0.01769F, 0.02237F),
                        localAngles = new Vector3(65.67442F, 179.3688F, 178.9807F),
                        localScale = new Vector3(0.06F, 0.06F, 0.06F)
                    }
                });
                idr.Add("mdlMerc", new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "CalfL",
                        localPos = new Vector3(0.00788F, -0.04092F, 0.03867F),
                        localAngles = new Vector3(43.23627F, 103.2376F, 110.1412F),
                        localScale = new Vector3(0.06F, 0.06F, 0.06F)
                    }
                });
                idr.Add("mdlTreebot", new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "CalfFrontR",
                        localPos = new Vector3(0.01281F, 0.96816F, -0.04742F),
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
                        childName = "Chest",
                        localPos = new Vector3(-0.21457F, 0.03938F, 0.20843F),
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
                        childName = "CalfL",
                        localPos = new Vector3(-0.22129F, 0.05611F, 0.03178F),
                        localAngles = new Vector3(46.47424F, 0.03554F, 0.12109F),
                        localScale = new Vector3(0.8F, 0.8F, 0.8F)
                    }
                });
                idr.Add("mdlCaptain", new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "CalfL",
                        localPos = new Vector3(-0.00392F, 0.02245F, -0.01193F),
                        localAngles = new Vector3(351.6593F, 184.1165F, 180.3262F),
                        localScale = new Vector3(0.1F, 0.1F, 0.1F)
                    }
                });
                idr.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
                {
                    new RoR2.ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = ItemBodyModelPrefab,
                        childName = "UpperArmL",
                        localPos = new Vector3(-0.01174F, 0.3879F, -0.00083F),
                        localAngles = new Vector3(17.62127F, 109.7287F, 208.814F),
                        localScale = new Vector3(0.05F, 0.05F, 0.05F)
                    }
                });
				//Log.LogInfo(nameof(star_glass) + nameof(setIDR) + " new IDR data set.");
				#endregion
			}

		}

      public override void langInit()
      {
         LanguageAPI.Add(this.id, this.name_long);
         LanguageAPI.Add(this.id + "_PICKUP", this.pickup_long);
         LanguageAPI.Add(this.id + "_DESC", this.desc_long);
         LanguageAPI.Add(this.id + "_LORE", this.lore_long);
      }
		#endregion

	}
}
