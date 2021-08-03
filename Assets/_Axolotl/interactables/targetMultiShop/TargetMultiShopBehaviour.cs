using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;

namespace Axolotl {
	public class TargetMultiShopBehaviour : NetworkBehaviour
	{

		[SyncVar(hook = "OnSyncPickupIndex")]
		private PickupIndex pickupIndex = PickupIndex.none;

		[SyncVar(hook = "OnSyncHidden")]
		private bool hidden;

		[SyncVar(hook = "SetHasBeenPurchased")]
		private bool hasBeenPurchased = false;

		[Tooltip("The PickupDisplay component that should show which item this shop terminal is offering.")]
		public PickupDisplay pickupDisplay;

		[Tooltip("The position from which the drop will be emitted")]
		public Transform dropTransform;

		[Tooltip("The drop table to select a pickup index from - only works if the pickup generates itself")]
		public PickupDropTable dropTable;
		
		private ShopType shopType;

		[Tooltip("The tier of items to drop - only works if the pickup generates itself and the dropTable field is empty.")]
		public ItemTier itemTier;

		public ItemTag bannedItemTag;

		[Tooltip("The velocity with which the drop will be emitted. Rotates with this object.")]
		public Vector3 dropVelocity;

		public Animator animator;
		public PurchaseInteraction purchaseInteraction;
		public TargetMultiShopController controller = null;


		public void Start()
		{
			if (NetworkClient.active)
			{
				this.UpdatePickupDisplayAndAnimations();
				this.controller = gameObject.transform.parent.parent.GetComponent<TargetMultiShopController>();
				if (this.controller == null)
				{
					Log.LogError("Controller Equals Null at " + nameof(Start) + " Even after attempting the new find.");
				}
			}
		}

		//This function does nothing at the moment. I included it because I was not sure if it would be needed again.
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
			List<PickupIndex> list = selectiveDropTableController.getDropList(shopType, itemTier, false);
			newPickupIndex = Run.instance.runRNG.NextElementUniform<PickupIndex>(list);
			this.SetPickupIndex(newPickupIndex, false);
		}


		private void UpdatePickupDisplayAndAnimations()
		{
			if (this.pickupDisplay)
			{
				this.pickupDisplay.SetPickupIndex(this.pickupIndex, this.hidden);
			}
			if (this.pickupIndex == PickupIndex.none)
			{
				closeChestAnimation();
			}
		}

		public void closeChestAnimation()
        {
			//Util.PlaySound("Play_UI_tripleChestShutter", base.gameObject);
			if (this.animator)
			{
				int layerIndex = this.animator.GetLayerIndex("Body");
				this.animator.PlayInFixedTime("ShieldUp", layerIndex);
			}
		}
		public void purchaseCorrection(Interactor activator)
		{
			this.controller.purchaseCorrection(activator, this.purchaseInteraction, this);
		}

		#region Networking
		#region Networking Helpers
		public void SetPickupIndex(PickupIndex newPickupIndex, bool newHidden = false)
		{
			if (this.pickupIndex != newPickupIndex || this.hidden != newHidden)
			{
				this.NetworkpickupIndex = newPickupIndex;
				this.Networkhidden = newHidden;
			}
		}

		public PickupIndex CurrentPickupIndex()
		{
			return this.pickupIndex;
		}

		public bool pickupIndexIsHidden
		{
			get
			{
				return this.hidden;
			}
		}

		public void SetHasBeenPurchased(bool newHasBeenPurchased)
		{
			Log.LogDebug("Setting Shop To Purchased To: " + newHasBeenPurchased);
			if (this.hasBeenPurchased != newHasBeenPurchased)
			{
				this.NetworkhasBeenPurchased = newHasBeenPurchased;
				if (newHasBeenPurchased)
				{
					this.closeChestAnimation();
					if (this.controller != null)
					{
						this.controller.DisableAllTerminals(gameObject);
					}
					else
					{
						Log.LogError(nameof(SetHasBeenPurchased) + ": Unable to retrieve parent object to call DisableAllTerminals");
					}
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
		#endregion


		[Server]
		public void DropPickup()
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void RoR2.ShopTerminalBehavior::DropPickup()' called on client");
				return;
			}
			this.SetHasBeenPurchased(true);
			if(this.pickupIndex == null)
         {
				Log.LogWarning(nameof(DropPickup) + ": Pickup Index is empty.");
				return;
         }
			PickupDropletController.CreatePickupDroplet(this.pickupIndex, (this.dropTransform ? this.dropTransform : base.transform).position, base.transform.TransformVector(this.dropVelocity));
		}


		private void UNetVersion()	{ }

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
			if ((base.syncVarDirtyBits & 1U) != 0)
			{
				if (!flag)
				{
					writer.WritePackedUInt32(base.syncVarDirtyBits);
					flag = true;
				}
				NetworkUtils.WritePickupIndex(writer, this.pickupIndex);
			}
			if ((base.syncVarDirtyBits & 2U) != 0)
			{
				if (!flag)
				{
					writer.WritePackedUInt32(base.syncVarDirtyBits);
					flag = true;
				}
				writer.Write(this.hidden);
			}
			if ((base.syncVarDirtyBits & 4U) != 0)
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
        #endregion
    }
}
