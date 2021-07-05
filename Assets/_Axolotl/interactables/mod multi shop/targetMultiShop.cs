using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Axolotl
{
    [BepInDependency("com.bepis.r2api")]
    public class targetMultiShop : NetworkBehaviour
    {
        public enum ShopType
        {
            Vanilla,
            VanillaEquip,
            Modded,
            ModdedEquip
        }

        public ShopType type;

		public GameObject terminalPrefab;
				
		public Transform[] terminalPositions;
		private GameObject[] terminalGameObjects;

		[Tooltip("Whether or not there's a chance the item contents are replaced with a '?'")]
		private bool hideDisplayContent = true;
		
		[SyncVar]
		private int cost;

		[SyncVar]
		private bool available = true;

		public int baseCost;
		public CostTypeIndex costType;

		private selectiveDropTable dropTable;

		private Xoroshiro128Plus rng;

        void Awake() 
        {
			if (NetworkServer.active)
			{
				this.rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
				this.GenerateTerminals();
			}       
		}

		// Token: 0x0600142C RID: 5164 RVA: 0x00053CA0 File Offset: 0x00051EA0
		private void Start()
		{
			if (Run.instance && NetworkServer.active)
			{
				this.Networkcost = Run.instance.GetDifficultyScaledCost(this.baseCost);
				if (this.terminalGameObjects != null)
				{
					GameObject[] array = this.terminalGameObjects;
					for (int i = 0; i < array.Length; i++)
					{
						PurchaseInteraction component = array[i].GetComponent<PurchaseInteraction>();
						component.Networkcost = this.cost;
						component.costType = this.costType;
					}
				}
			}
		}

		// Token: 0x0600142D RID: 5165 RVA: 0x00053D14 File Offset: 0x00051F14
		private void OnDestroy()
		{
			if (this.terminalGameObjects != null)
			{
				for (int i = this.terminalGameObjects.Length - 1; i >= 0; i--)
				{
					UnityEngine.Object.Destroy(this.terminalGameObjects[i]);
				}
				this.terminalGameObjects = null;
			}
		}

        // Update is called once per frame
        void Update()
        {

        }

        //This will populate all of the designated locations with selectiveMultiShops
        void GenerateTerminals()
        {
			this.terminalGameObjects = new GameObject[this.terminalPositions.Length];
			for (int i = 0; i < this.terminalPositions.Length; i++)
            {
				PickupIndex pickupIndex = PickupIndex.none;
				switch (this.type)
                {
					case ShopType.Modded:
						break;
					case ShopType.ModdedEquip:
						break;
					case ShopType.Vanilla:
						break;
					case ShopType.VanillaEquip:
						break;
                }
            }
        }

		private void DisableAllTerminals(Interactor interactor)
		{
			foreach (GameObject gameObject in this.terminalGameObjects)
			{
				gameObject.GetComponent<PurchaseInteraction>().Networkavailable = false;
				gameObject.GetComponent<ShopTerminalBehavior>().SetNoPickup();
			}
			this.Networkavailable = false;
		}


		public bool ShouldDisplayHologram(GameObject viewer)
		{
			return this.available;
		}


		public GameObject GetHologramContentPrefab()
		{
			return Resources.Load<GameObject>("Prefabs/CostHologramContent");
		}


		public void UpdateHologramContent(GameObject hologramContentObject)
		{
			CostHologramContent component = hologramContentObject.GetComponent<CostHologramContent>();
			if (component)
			{
				component.displayValue = this.cost;
				component.costType = this.costType;
			}
		}

		private void UNetVersion()
		{
		}


		public bool Networkavailable
		{
			get
			{
				return this.available;
			}
			[param: In]
			set
			{
				base.SetSyncVar<bool>(value, ref this.available, 1U);
			}
		}

		public int Networkcost
		{
			get
			{
				return this.cost;
			}
			[param: In]
			set
			{
				base.SetSyncVar<int>(value, ref this.cost, 2U);
			}
		}

		public override bool OnSerialize(NetworkWriter writer, bool forceAll)
		{
			if (forceAll)
			{
				writer.Write(this.available);
				writer.WritePackedUInt32((uint)this.cost);
				return true;
			}
			bool flag = false;
			if ((base.syncVarDirtyBits & 1U) != 0U)
			{
				if (!flag)
				{
					writer.WritePackedUInt32(base.syncVarDirtyBits);
					flag = true;
				}
				writer.Write(this.available);
			}
			if ((base.syncVarDirtyBits & 2U) != 0U)
			{
				if (!flag)
				{
					writer.WritePackedUInt32(base.syncVarDirtyBits);
					flag = true;
				}
				writer.WritePackedUInt32((uint)this.cost);
			}
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
			}
			return flag;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			if (initialState)
			{
				this.available = reader.ReadBoolean();
				this.cost = (int)reader.ReadPackedUInt32();
				return;
			}
			int num = (int)reader.ReadPackedUInt32();
			if ((num & 1) != 0)
			{
				this.available = reader.ReadBoolean();
			}
			if ((num & 2) != 0)
			{
				this.cost = (int)reader.ReadPackedUInt32();
			}
		}

		
	}
}
