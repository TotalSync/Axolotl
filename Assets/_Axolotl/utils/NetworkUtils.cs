using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace Axolotl
{
   public class NetworkUtils
   {
      public static void WritePickupIndex(NetworkWriter writer, PickupIndex value)
      {
          writer.WritePackedUInt32((uint)value.value);
      }

      public static PickupIndex ReadPickupIndex(NetworkReader reader)
      {
          return new PickupIndex ((int)reader.ReadPackedUInt32());
      }

      public static void WritePickupIndicies(NetworkWriter writer, PickupIndex[] indicies)
		{
         for(int i = 0; i < 3; i++)
			{
            writer.WritePackedUInt32((uint)indicies[i].value);
			}
		}

      public static PickupIndex[] ReadPickupIndicies(NetworkReader reader)
		{
         PickupIndex[] indicies = new PickupIndex[3];
         for(int i = 0; i < 3; i++) 
         {
            indicies[i] = ReadPickupIndex(reader);
         }
         return indicies;
		}
   }
}
