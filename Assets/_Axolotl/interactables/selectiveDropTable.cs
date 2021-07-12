using System.Collections;
using System.Collections.Generic;
using RoR2;
using BepInEx;
using R2API;
using UnityEngine;


namespace Axolotl
{
    public class selectiveDropTable : MonoBehaviour
    {

        public List<PickupIndex> availableTeir1DropList;
        public List<PickupIndex> availableTeir2DropList;
        public List<PickupIndex> availableTeir3DropList;
        public List<PickupIndex> availableEquipmentDropList;
        private readonly WeightedSelection<List<PickupIndex>> selector = new WeightedSelection<List<PickupIndex>>(8);

        public static readonly int lastVanillaItem = 120;
        public static readonly int lastVanillaEquip = 37;

        public float tier1Weight = 0.8f;
        public float tier2Weight = 0.2f;
        public float tier3Weight = 0.01f;

        public selectiveDropTable(selectiveDropTableController.ShopType type, Run run)
        {
            switch (type) 
            {
                case selectiveDropTableController.ShopType.Modded:
                    foreach (PickupIndex item in run.availableTier1DropList)
                        {
                            var pickup = PickupCatalog.GetPickupDef(item);
                            if (pickup != null && (int)pickup.itemIndex > lastVanillaItem)
                            {
                                availableTeir1DropList.Add(item);
                            }
                        }
                    foreach (PickupIndex item in run.availableTier2DropList)
                        {
                            var pickup = PickupCatalog.GetPickupDef(item);
                            if (pickup != null && (int)pickup.itemIndex > lastVanillaItem)
                            {
                                availableTeir2DropList.Add(item);
                            }
                        }
                    foreach (PickupIndex item in run.availableTier3DropList)
                        {
                            var pickup = PickupCatalog.GetPickupDef(item);
                            if (pickup != null && (int)pickup.itemIndex > lastVanillaItem)
                            {
                                availableTeir3DropList.Add(item);
                            }
                        }
                    break;
                case selectiveDropTableController.ShopType.ModdedEquip:
                    foreach (PickupIndex item in run.availableEquipmentDropList)
                        {
                            var pickup = PickupCatalog.GetPickupDef(item);
                            if (pickup != null && (int)pickup.equipmentIndex > lastVanillaEquip)
                            {
                                availableEquipmentDropList.Add(item);
                            }
                        }
                    break;
                case selectiveDropTableController.ShopType.Vanilla:
                    foreach (PickupIndex item in run.availableTier1DropList)
                        {
                            var pickup = PickupCatalog.GetPickupDef(item);
                            if (pickup != null && (int)pickup.itemIndex > lastVanillaItem && (int)pickup.itemIndex > 0)
                            {
                                availableTeir1DropList.Add(item);
                            }
                        }
                    foreach (PickupIndex item in run.availableTier2DropList)
                        {
                            var pickup = PickupCatalog.GetPickupDef(item);
                            if (pickup != null && (int)pickup.itemIndex > lastVanillaItem && (int)pickup.itemIndex > 0)
                            {
                                availableTeir2DropList.Add(item);
                            }
                        }
                    foreach (PickupIndex item in run.availableTier3DropList)
                        {
                            var pickup = PickupCatalog.GetPickupDef(item);
                            if (pickup != null && (int)pickup.itemIndex > lastVanillaItem && (int)pickup.itemIndex > 0)
                            {
                                availableTeir3DropList.Add(item);
                            }
                        }
                    break;
                    case selectiveDropTableController.ShopType.VanillaEquip:
                    foreach (PickupIndex item in run.availableEquipmentDropList)
                        {
                            var pickup = PickupCatalog.GetPickupDef(item);
                            if (pickup != null && (int)pickup.itemIndex > lastVanillaEquip && (int)pickup.itemIndex > 0)
                            {
                                availableEquipmentDropList.Add(item);
                            }
                        }
                    break; 
            }
            generateWerightedSelection();
        }

        private void generateWerightedSelection()
        {
            this.selector.Clear();
            this.selector.AddChoice(this.availableTeir1DropList, tier1Weight);
            this.selector.AddChoice(this.availableTeir2DropList, tier2Weight);
            this.selector.AddChoice(this.availableTeir3DropList, tier3Weight);
            this.selector.AddChoice(this.availableEquipmentDropList, 1);
        }
        
        public PickupIndex GenerateDrop(Run run)
        {
            List<PickupIndex> list = this.selector.Evaluate(run.runRNG.nextNormalizedFloat);
            if (list.Count <= 0) 
            {
                return PickupIndex.none;
            }
            return run.runRNG.NextElementUniform<PickupIndex>(list);
        }
    }
}