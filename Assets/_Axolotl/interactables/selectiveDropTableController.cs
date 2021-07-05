using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;

namespace Axolotl
{
    public class selectiveDropTableController : MonoBehaviour
    {

        //This holds all of the modded drop tables
        //The index of the array corresponds to the drop table used
        //Indexed with the Enum in selectiveDropTable.
        public selectiveDropTable[] dropTables = new selectiveDropTable[4];

        void Awake()
        {
            Run.onRunStartGlobal += (orig) =>
            {
                for (int i = 0; i < 4; i++)
                {
                    dropTables[i] = new selectiveDropTable((targetMultiShop.ShopType)i, orig);
                }
            };
        }
    }
}