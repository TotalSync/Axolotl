//#define VERBOSE

using System.Collections.Generic;
using RoR2;



namespace Axolotl
{
    public enum ShopType
    {
        Vanilla,
        VanillaEquip,
        Modded,
        ModdedEquip
    }

    public static class selectiveDropTableController
    {
        /*
         *  masterList[Modded?,Item?][itemRarity][items]
        */
        private static List<List<PickupIndex>>[,] masterList = new List<List<PickupIndex>>[2, 2];

        private static readonly WeightedSelection<List<PickupIndex>> itemSelector = new WeightedSelection<List<PickupIndex>>(8);
        private static readonly WeightedSelection<ShopType> shopTypeSelector = new WeightedSelection<ShopType>(4);
        private static readonly WeightedSelection<ItemTier> rarirtySelector= new WeightedSelection<ItemTier>(5);

        private static Xoroshiro128Plus rng;

        public static readonly int lastVanillaItem = 131;
        public static readonly int lastVanillaEquip = 40;

        public static float tier1Weight = 0.8f;
        public static float tier2Weight = 0.2f;
        public static float tier3Weight = 0.01f;

        public static float shopTier1Weight = 0.8f;
        public static float shopTier2Weight = 0.2f;
        public static float shopBossTierWeight = 0.02f;
        public static float shopTier3Weight = 0.01f;
        

        public static void initializeDropTables()
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    masterList[i, j] = new List<List<PickupIndex>>();
                    if (j == 0)
                    {
                        for (int k = 0; k <= (int)ItemTier.Boss; k++)
                        {
                            masterList[i, j].Add(new List<PickupIndex>());
                        }
                    }
                    else
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            masterList[i, j].Add(new List<PickupIndex>());
                        }
                    }
                    
                }
            }
        }        

        public static void generateDropTables()
        {
            rng = Run.instance.runRNG;
            initializeShopSelector();
            initializeRaritySelector();
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    foreach ( List<PickupIndex> list in masterList[i, j])
                    {
                        list.Clear();
                    }
                }
            }

            ItemIndex itemIndex = (ItemIndex)0;
            ItemIndex vanillaIEnd = (ItemIndex)lastVanillaItem;
            ItemIndex itemCount = (ItemIndex)ItemCatalog.itemCount;
            Log.LogInfo("There are " + (int)itemCount + " items in game. Generating Drop Tables.");

            while (itemIndex < itemCount)
            {
                if (Run.instance.availableItems.Contains(itemIndex))
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
#if DEBUG && !VERBOSE
                    Log.LogDebug(itemDef.name + " " + (int)itemIndex);
#else
#if DEBUG && VERBOSE
                    Log.LogInfo(itemDef.name + " " + (int)itemIndex);
#endif
#endif

                    List<PickupIndex> list = null;
                    bool modded = itemIndex > vanillaIEnd;
                    if (modded)
                    {
                        list = getDropList(ShopType.Modded, itemDef.tier);
                    }
                    else
                    {
                        list = getDropList(ShopType.Vanilla, itemDef.tier);
                    }

                    if (list != null && itemDef.DoesNotContainTag(ItemTag.WorldUnique))
                    {
                        masterList[modded ? 1 : 0, (int)ShopType.Vanilla % 2][(int)itemDef.tier].Add(PickupCatalog.FindPickupIndex(itemIndex));
                    }
                    
                }
                itemIndex++;
            }

            EquipmentIndex equipmentIndex = (EquipmentIndex)0;
            EquipmentIndex vanillaEEnd = (EquipmentIndex)lastVanillaEquip;
            EquipmentIndex equipmentCount = (EquipmentIndex)EquipmentCatalog.equipmentCount;
            Log.LogInfo("There are " + (int)equipmentCount + " equipments in game. Generating Drop Tables.");

            while (equipmentIndex < equipmentCount)
            { 
                EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
                List<PickupIndex> list = null;
                if (Run.instance.availableEquipment.Contains(equipmentIndex))
                {
                    if (equipmentDef.canDrop)
                    {
#if DEBUG && !VERBOSE
                        Log.LogDebug(equipmentDef.name + " " + (int)equipmentIndex);
#else
#if DEBUG && VERBOSE
                        Log.LogInfo(equipmentDef.name + " " + (int)equipmentIndex);
#endif
#endif
                        bool modded = equipmentIndex > vanillaEEnd;
                        if (modded)
                        {
                            list = getDropList(ShopType.ModdedEquip, ItemTier.NoTier, equipmentDef.isLunar);
                        }
                        else
                        {
                            list = getDropList(ShopType.VanillaEquip, ItemTier.NoTier, equipmentDef.isLunar);
                        }

                        if (list != null)
                        {
                            masterList[modded ? 1 : 0, (int)ShopType.VanillaEquip][equipmentDef.isLunar ? 1 : 0].Add(PickupCatalog.FindPickupIndex(equipmentIndex));
                        }
                    }
                }
                equipmentIndex++;
            }
#if DEBUG && VERBOSE
            PrintTableContents();
#endif

        }


        public static PickupIndex GenerateDrop(Run run)
        {
            List<PickupIndex> list = itemSelector.Evaluate(run.runRNG.nextNormalizedFloat);
            if (list.Count <= 0)
            {
                return PickupIndex.none;
            }
            return run.runRNG.NextElementUniform<PickupIndex>(list);
        }

        public static List<PickupIndex> getDropList(ShopType type, ItemTier tier = ItemTier.NoTier, bool isLunar = false)
        {
            int isEquip = getSecondCoord(type);
            if (isEquip == 1)
            {
                return masterList[getFirstCoord(type), isEquip][isLunar ? 1 : 0];
            }
            else
            {
                if(tier == ItemTier.NoTier)
                {
                    Log.LogError(nameof(getDropList) + ": Attemted Indexing of NoTier Item.");
                    return null;
                }
                return masterList[getFirstCoord(type), isEquip][(int)tier];
            }
        }


        

        public static ShopType rollShopType()
        {
            return shopTypeSelector.Evaluate(rng.nextNormalizedFloat);
        }

        public static ItemTier rollItemRarity()
        {
            return rarirtySelector.Evaluate(rng.nextNormalizedFloat);
        }

#region InitilzerFunctions
        private static void initializeShopSelector()
        {
            shopTypeSelector.AddChoice(ShopType.Vanilla, 1);
            shopTypeSelector.AddChoice(ShopType.VanillaEquip, 0.5f);
            shopTypeSelector.AddChoice(ShopType.Modded, 0.5f);
            shopTypeSelector.AddChoice(ShopType.ModdedEquip, 0.1f);
        }

        private static void initializeRaritySelector()
        {
            rarirtySelector.AddChoice(ItemTier.Tier1, tier1Weight);
            rarirtySelector.AddChoice(ItemTier.Tier2, tier2Weight);
            rarirtySelector.AddChoice(ItemTier.Tier3, tier3Weight);
            rarirtySelector.AddChoice(ItemTier.Boss, shopBossTierWeight);
        }

#endregion

#region DebugFunctions

        public static void PrintTableContents()
        {
            Log.LogDebug(nameof(selectiveDropTableController) + ": ------------------Dumping Drop Table Contents -----------------");
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (j == 0)
                    {
                        for (int k = 0; k <= (int)ItemTier.Boss; k++)
                        {
                            foreach(var item in masterList[i, j][k])
                            {
                                if (item != null)
                                {
                                    Log.LogDebug(((i == 1) ? "Modded" : "Vanilla") +
                                        " " + (isEquipment((ShopType)j) ? "Equipment" : "Item") +
                                        " " + (ItemTier)k +
                                        ": Item : " + PickupCatalog.GetPickupDef(item).nameToken);
                                }
                                else
                                {
                                    Log.LogError("Error Drop Table List is empty: " + ((i == 1) ? "Modded" : "Vanilla") +
                                        " " + (isEquipment((ShopType)j) ? "Equipment" : "Item") +
                                        " " + (ItemTier)k);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            foreach(var equip in masterList[i, j][k])
                            {
                                if (equip != null)
                                {
                                    Log.LogDebug(((i == 1) ? "Modded" : "Vanilla") +
                                        " " + (isEquipment((ShopType)j) ? "Equipment" : "Item") +
                                        " " + (k == 0 ? "Normal" : "Lunar") +
                                        ": Equipment : " + PickupCatalog.GetPickupDef(equip).nameToken);
                                }
                                else
                                {
                                    Log.LogError("Error Drop Table List is empty: " + ((i == 1) ? "Modded" : "Vanilla") +
                                        " " + (isEquipment((ShopType)j) ? "Equipment" : "Item") +
                                        " " + (k == 0 ? "Normal" : "Lunar"));
                                }
                            }
                        }
                    }

                }
            }
        }

#endregion

#region HelperFunctions
        private static int getFirstCoord(ShopType type)
        {
            int index = (int)type;
            return index - 2 > 0 ? 1 : 0;
        }

        private static int getSecondCoord(ShopType type)
        {
            int index = (int)type;
            return index % 2;
        }

        public static bool isEquipment(ShopType type)
        {
            return getSecondCoord(type) == 1;
        }

        public static bool isModded(ShopType type)
        {
            return getFirstCoord(type) == 1;
        }
#endregion
    }
}