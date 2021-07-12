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

    }
}
