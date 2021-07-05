using System.Collections;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace Axolotl
{
    [BepInDependency("com.bepis.r2api")]
    public class targetMultiShop : MonoBehaviour
    {
        public enum ShopType
        {
            Vanilla,
            VanillaEquip,
            Modded,
            ModdedEquip
        }

        public ShopType type;

        void Awake() 
        {
            On.RoR2.Run.Start += (orig, self) =>
            {
                GenerateDropTables();
                orig(self);
            };
        }

        // Start is called before the first frame update
        void Start()
        {
            GenerateDropTables();
        }

        // Update is called once per frame
        void Update()
        {

        }

        //This will populate the items in the multishop terminals based ont the type of shop it is.
        void GenerateTerminals()
        {

        }

        void GenerateDropTables() 
        {
         
        }
    }
}
