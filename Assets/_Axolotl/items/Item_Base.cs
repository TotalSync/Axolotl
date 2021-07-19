using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;


namespace Axolotl
{
    [BepInDependency("com.bepis.r2api")]
    [R2APISubmoduleDependency(nameof(LanguageAPI))]
    public abstract class Item_Base : MonoBehaviour
    {
        public ItemDef item_def;
        public ItemDisplayRuleDict idr;
        public string pack_id;
        public string id;
        public string name_long;
        public string pickup_long;
        public string desc_long;
        public string lore_long;

        //Creates an empty item.
        public Item_Base()
        {
            this.item_def = ScriptableObject.CreateInstance<ItemDef>(); //(ItemDef)ScriptableObject.CreateInstance(nameof(ItemDef));
            this.idr = new ItemDisplayRuleDict();
            this.pack_id = "DEBUG";
            this.id = "DEBUG";
            this.name_long = "DEBUG_NAME";
            this.pickup_long = "DEBUG_PICKUP";      
            this.desc_long = "DEBUG_DESC";
            this.lore_long = "DEBUG_LORE";
        }
        
        public Item_Base(string name) 
        {
            this.id = name.ToUpper();
            this.name_long     = this.id;
            this.pickup_long   = this.id + "_PICKUP";
            this.desc_long     = this.id + "_DESC";
            this.lore_long     = this.id + "_LORE";
        }
        //Creates an Item_Base based off an item_def
        public Item_Base(ItemDef item_def)
        {
            this.item_def = item_def;
            this.id = item_def.nameToken.ToUpper();
            this.idr = new ItemDisplayRuleDict();
            Log.LogError(nameof(Item_Base) + "This funtion ran");
        }


        public virtual void initialize() 
        {

            langInit();
            setIDR();
            SetHooks();

        }

        //This function must be generated on a per-item basis
        //This function is for setting hooks so that an item has functionality.
        //It is called last in initalize()
        public abstract void SetHooks();

        //This function is for setting the Item Display Rules(IDR)
        public abstract void setIDR();

        //This function is not used currently, as all of my items do not require an unset.
        public virtual void UnsetHooks() { }

        public virtual void langInit()
        {
            LanguageAPI.Add(this.id, this.name_long);
            LanguageAPI.Add(this.id + "_PICKUP", this.pickup_long);
            LanguageAPI.Add(this.id + "_DESC", this.desc_long);
            LanguageAPI.Add(this.id + "_LORE", this.lore_long);
        }

        
    }
}