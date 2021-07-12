using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace Axolotl {
	public class targetMultiShopBehavior : NetworkBehaviour
	{
		selectiveDropTableController.ShopType shopType;

		[SyncVar(hook = "OnSyncPickupIndex")]
		private PickupIndex pickupIndex = PickupIndex.none;

		[SyncVar(hook = "OnSyncHidden")]
		private bool hidden;

		[SyncVar(hook = "SetHasBeenPurchased")]
		private bool hasBeenPurchased;

		[Tooltip("The PickupDisplay component that should show which item this shop terminal is offering.")]
		public PickupDisplay pickupDisplay;

		[Tooltip("The position from which the drop will be emitted")]
		public Transform dropTransform;

		[Tooltip("The drop table to select a pickup index from - only works if the pickup generates itself")]
		public PickupDropTable dropTable;

		[Tooltip("The tier of items to drop - only works if the pickup generates itself and the dropTable field is empty.")]
		public ItemTier itemTier;

		public ItemTag bannedItemTag;

		[Tooltip("The velocity with which the drop will be emitted. Rotates with this object.")]
		public Vector3 dropVelocity;

		public Animator animator;

		public bool pickupIndexIsHidden
		{
			get
			{
				return this.hidden;
			}
		}

		public void SetHasBeenPurchased(bool newHasBeenPurchased)
		{
			if (this.hasBeenPurchased != newHasBeenPurchased)
			{
				this.NetworkhasBeenPurchased = newHasBeenPurchased;
				if (newHasBeenPurchased && this.animator)
				{
					int layerIndex = this.animator.GetLayerIndex("Body");
					this.animator.PlayInFixedTime("Opening", layerIndex);
				}
			}
		}

		private void OnSyncHidden(bool newHidden)
		{
			this.SetPickupIndex(this.pickupIndex, newHidden);
		}

		private void OnSyncPickupIndex(PickupIndex newPickupIndex)
		{
			this.SetPickupIndex(newPickupIndex, this.hidden);
			if (NetworkClient.active)
			{
				this.UpdatePickupDisplayAndAnimations();
			}
		}

		public void Start()
		{
			if (NetworkClient.active)
			{
				this.UpdatePickupDisplayAndAnimations();
			}
		}

		[Server]
		public void GenerateNewPickupServer()
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void RoR2.ShopTerminalBehavior::GenerateNewPickupServer()' called on client");
				return;
			}
			if (this.hasBeenPurchased)
			{
				return;
			}
			PickupIndex newPickupIndex = PickupIndex.none;
			List<PickupIndex> list;
			if (this.shopType == selectiveDropTableController.ShopType.Modded || this.shopType == selectiveDropTableController.ShopType.Vanilla) { 
				switch (this.itemTier)
				{
					case ItemTier.Tier1:
						list = selectiveDropTableController.dropTables[(int)shopType].availableTeir1DropList;
						break;
					case ItemTier.Tier2:
						list = selectiveDropTableController.dropTables[(int)shopType].availableTeir2DropList;
						break;
					case ItemTier.Tier3:
						list = selectiveDropTableController.dropTables[(int)shopType].availableTeir3DropList;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			} 
			else
            {
				list = selectiveDropTableController.dropTables[(int)shopType].availableEquipmentDropList;
            }
			newPickupIndex = Run.instance.runRNG.NextElementUniform<PickupIndex>(list);
			this.SetPickupIndex(newPickupIndex, false);
		}

		public void SetPickupIndex(PickupIndex newPickupIndex, bool newHidden = false)
		{
			if (this.pickupIndex != newPickupIndex || this.hidden != newHidden)
			{
				this.NetworkpickupIndex = newPickupIndex;
				this.Networkhidden = newHidden;
			}
		}

		private void UpdatePickupDisplayAndAnimations()
		{
			if (this.pickupDisplay)
			{
				this.pickupDisplay.SetPickupIndex(this.pickupIndex, this.hidden);
			}
			if (this.pickupIndex == PickupIndex.none)
			{
				Util.PlaySound("Play_UI_tripleChestShutter", base.gameObject);
				if (this.animator)
				{
					int layerIndex = this.animator.GetLayerIndex("Body");
					this.animator.PlayInFixedTime(this.hasBeenPurchased ? "Open" : "Closing", layerIndex);
				}
			}
		}

		public PickupIndex CurrentPickupIndex()
		{
			return this.pickupIndex;
		}

		[Server]
		public void SetNoPickup()
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void RoR2.ShopTerminalBehavior::SetNoPickup()' called on client");
				return;
			}
			this.SetPickupIndex(PickupIndex.none, false);
		}

		[Server]
		public void DropPickup()
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void RoR2.ShopTerminalBehavior::DropPickup()' called on client");
				return;
			}
			this.SetHasBeenPurchased(true);
			PickupDropletController.CreatePickupDroplet(this.pickupIndex, (this.dropTransform ? this.dropTransform : base.transform).position, base.transform.TransformVector(this.dropVelocity));
		}
		/*
		[CompilerGenerated]
		private bool <GenerateNewPickupServer>g__PassesFilter|17_0(PickupIndex pickupIndex)
		{
			if (this.bannedItemTag == ItemTag.Any)
			{
			return true;
		}
		PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
			return pickupDef.itemIndex == ItemIndex.None || !ItemCatalog.GetItemDef(pickupDef.itemIndex).ContainsTag(this.bannedItemTag);
	}

	[CompilerGenerated]
	private PickupIndex<GenerateNewPickupServer> g__Pick|17_1(List<PickupIndex> list)
		{
			return Run.instance.treasureRng.NextElementUniform<PickupIndex>(list.Where(new Func<PickupIndex, bool>(this.<GenerateNewPickupServer>g__PassesFilter|17_0)).ToList<PickupIndex>());
	}*/

		private void UNetVersion()
		{
		}

		public PickupIndex NetworkpickupIndex
		{
			get
			{
				return this.pickupIndex;
			}
			[param: In]
			set
			{
				if (NetworkServer.localClientActive && !base.syncVarHookGuard)
				{
					base.syncVarHookGuard = true;
					this.OnSyncPickupIndex(value);
					base.syncVarHookGuard = false;
				}
				base.SetSyncVar<PickupIndex>(value, ref this.pickupIndex, 1U);
			}
		}

		public bool Networkhidden
		{
			get
			{
				return this.hidden;
			}
			[param: In]
			set
			{
				if (NetworkServer.localClientActive && !base.syncVarHookGuard)
				{
					base.syncVarHookGuard = true;
					this.OnSyncHidden(value);
					base.syncVarHookGuard = false;
				}
				base.SetSyncVar<bool>(value, ref this.hidden, 2U);
			}
		}

		public bool NetworkhasBeenPurchased
		{
			get
			{
				return this.hasBeenPurchased;
			}
			[param: In]
			set
			{
				if (NetworkServer.localClientActive && !base.syncVarHookGuard)
				{
					base.syncVarHookGuard = true;
					this.SetHasBeenPurchased(value);
					base.syncVarHookGuard = false;
				}
				base.SetSyncVar<bool>(value, ref this.hasBeenPurchased, 4U);
			}
		}

		public override bool OnSerialize(NetworkWriter writer, bool forceAll)
		{
			if (forceAll)
			{
				NetworkUtils.WritePickupIndex(writer, this.pickupIndex);
				writer.Write(this.hidden);
				writer.Write(this.hasBeenPurchased);
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
				NetworkUtils.WritePickupIndex(writer, this.pickupIndex);
			}
			if ((base.syncVarDirtyBits & 2U) != 0U)
			{
				if (!flag)
				{
					writer.WritePackedUInt32(base.syncVarDirtyBits);
					flag = true;
				}
				writer.Write(this.hidden);
			}
			if ((base.syncVarDirtyBits & 4U) != 0U)
			{
				if (!flag)
				{
					writer.WritePackedUInt32(base.syncVarDirtyBits);
					flag = true;
				}
				writer.Write(this.hasBeenPurchased);
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
				this.pickupIndex = NetworkUtils.ReadPickupIndex(reader);
				this.hidden = reader.ReadBoolean();
				this.hasBeenPurchased = reader.ReadBoolean();
				return;
			}
			int num = (int)reader.ReadPackedUInt32();
			if ((num & 1) != 0)
			{
				this.OnSyncPickupIndex(NetworkUtils.ReadPickupIndex(reader));
			}
			if ((num & 2) != 0)
			{
				this.OnSyncHidden(reader.ReadBoolean());
			}
			if ((num & 4) != 0)
			{
				this.SetHasBeenPurchased(reader.ReadBoolean());
			}
		}
	} 
}
