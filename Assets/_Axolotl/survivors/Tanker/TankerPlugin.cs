using BepInEx;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using StubbedConverter;

//[module: UnverifiableCode]
//[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Axolotl.Tanker
{
    //[BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    //[BepInDependency("com.valex.ShaderConverter", BepInDependency.DependencyFlags.HardDependency)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    //[BepInPlugin(MODUID, MODNAME, MODVERSION)]
    [R2APISubmoduleDependency(new string[]
    {
        "PrefabAPI",
        "LanguageAPI",
        "SoundAPI",
    })]
    public class TankerPlugin
    {
        // if you don't change these you're giving permission to deprecate the mod-
        //  please change the names to your own stuff, thanks
        //   this shouldn't even have to be said
        public const string MODUID = "com.sync.Tanker";
        public const string MODNAME = "TankerSubmodule";
        public const string MODVERSION = "0.0.1";

        // a prefix for name tokens to prevent conflicts- please capitalize all name tokens for convention
        public const string developerPrefix = "AXOLOTL";

        // use this to toggle debug on stuff, make sure it's false before releasing
        public static bool debug = true;

        public static bool cancel;

        public static TankerPlugin instance;

        public void InitCharacter()
        {
            instance = this;

            // Load/Configure assets and read Config
            Modules.Assets.Init();
            if (cancel) return;
            Modules.Tokens.Init();
            Modules.Prefabs.Init();
            Modules.Buffs.Init();
            Modules.ItemDisplays.Init();
            Modules.Unlockables.Init();
            

            // Any debug stuff you need to do can go here before initialisation
            if (debug) { Modules.Helpers.AwakeDebug(); }

            //Initialize Content Pack
            Modules.ContentPackProvider.Initialize();

            LoadShaders();
            Hook();
        }

        private void LoadShaders()
        {
            // Using StubbedShaderConverter to convert stubbed materials into Hopoo equivalents.
            ShaderConvert.ConvertAssetBundleShaders(Modules.Assets.mainAssetBundle, true, debug);
        }

        private void Hook()
        {
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            // a simple stat hook, adds armor after stats are recalculated
            if (self)
            {
                if (self.HasBuff(Modules.Buffs.buffDefs[0].buffIndex))
                {
                    self.armor += 300f;
                }
            }
        }
    }
}
