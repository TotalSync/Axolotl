using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;

namespace Axolotl
{
   public enum MobParts
	{
      //-----Basic Enemies-----
      WispNiter,        //Wisp, GreaterWisp
      JellyJelly,       //Jellyfish, 
      BettleChitin,     //Beetle, BeetleGuard, BettleQueen
      LemurianSpine,    //Lemurian, ElderLemurian
      SmoothStones,     //HermitCrab
      WarpedBlood,      //Imp, ImpOverlord
      DrabFeathers,     //BridThings
      LunarCore,        //LunarExploders
      //-------Heavy Enemies------
      AngryCore,        //Golem
      BisonHorns,       //Bison
      BrassSpike,       //BrassContraption
      //-----Environment----------
      CollosusMetal,    //BlackBeach
      SwampTar,         //Swamp
      PotteryFrag,      //ScorchedAchers
      CuriousSnowflake, // IcePlace
      Hellstone,        //Hell
      LunarPebel,       //Moon
      TimelessCrystal,  //Shop
      AgelessGold,      //Aurelion
      VoidShard,        //Void
      //-------Misc----------
      DroneScrap,       //Chance on friendly Drone Death, Drones on map
   }

   public static class MobPartsSystem
   {
      //Determines Which Players can Collect Items
      static bool[] isCollector;

      //Catalogs Parts gained in a run
      static int[,] partsCollected;
      //Catalogs Parts Saved
      static int[,] partsStored;

      

      static int DustOfNakunia;



   } 
}
