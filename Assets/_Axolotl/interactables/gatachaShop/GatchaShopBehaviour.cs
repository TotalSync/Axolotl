using System.Runtime.InteropServices;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Axolotl
{
   public class GatchaShopBehaviour : NetworkBehaviour
   {
      private static readonly float UNAVAILABLE_TIME = 2f;

      public GameObject ballPrefab;
      public Transform[] ballPositions;

      [SyncVar(hook = "OnSyncPickup1")]
      private PickupIndex Ball1;
      [SyncVar(hook = "OnSyncPickup2")]
      private PickupIndex Ball2;
      [SyncVar(hook = "OnSyncPickup3")]
      private PickupIndex Ball3;

      [SyncVar(hook = "SetIsEmpty")]
      private bool isEmpty = false;

      [SyncVar(hook = "OnSyncBallCount")]
      private int ballCount;
  
      private UnityEngine.Color[] ballColors;
      private GameObject[] loadedBalls;


      [Tooltip("The position from which the drop will be emitted")]
      public Transform dropTransform;

      [Tooltip("The velocity with which the drop will be emitted. Rotates with this object.")]
      public Vector3 dropVelocity;

      [Tooltip("The animation controller that performs the animations for this shop.")]
      public Animator animator;

      [Tooltip("A refrence to the Purchase Interaction on this GameObject")]
      public PurchaseInteraction purchaseInteraction;

      [Tooltip("A reference to the Parent Controller")]
      public GatchaShopController controller;


      // Start is called before the first frame update
      void Start()
      {
			if (NetworkClient.active)
			{
            this.controller = gameObject.transform.GetComponentInParent<GatchaShopController>();
            if (this.controller == null) { Log.LogError("Gatcha Controller is null at" + nameof(Start)); }
            Log.LogWarning(nameof(Start) + ": Current Index " + CurrentIndex().value);
            this.loadBalls();
         }
      }

      
      private void loadBalls()
      {
         loadedBalls = new GameObject[3];

         loadedBalls[0] = handleBalls(1, NetworkIndex1);
         loadedBalls[1] = handleBalls(2, NetworkIndex2);
         loadedBalls[2] = handleBalls(3, NetworkIndex3);
      }

      private GameObject handleBalls(int ballNum, PickupIndex pickup)
		{
         GameObject newBall;
         var color = PickupCatalog.GetPickupDef(this.GetNetIndex(ballNum)).baseColor;
         if (color == null)
         {
            Log.LogError(nameof(loadBalls) + ": Ball Color Returned Null");
            return null;
         }
			else
			{
            newBall = UnityEngine.GameObject.Instantiate<GameObject>(ballPrefab, ballPositions[ballNum - 1]);
				if (newBall != null)
				{
               var light = newBall.transform.Find("Lights").GetComponent<MeshRenderer>().material;
               if(light)
					{
                  light.color = color;
					}
               else
               {
                  Log.LogError(nameof(loadBalls) + ": Light is not stored in the Balls. (Unable to find light material component in prefab).");
               }
            }
            return newBall;
         }
      }

      [Server]
      public void DropPickup()
		{
         if (!NetworkServer.active)
         {
            Debug.LogWarning("[Server] function 'System.Void RoR2.ShopTerminalBehavior::DropPickup()' called on client");
            return;
         }
         doVendAnimation();
         PickupIndex pickup = CurrentIndex();
         PickupDropletController.CreatePickupDroplet(pickup, (this.dropTransform ? this.dropTransform : base.transform).position, base.transform.TransformVector(this.dropVelocity));
      }

      public void PurchaseCycle()
      {
         if (Networkballcount <= 1 )
         {
            Networkisempty = true;
            this.purchaseInteraction.SetAvailable(false);
            this.controller.Networkavailable = false;
            if(Networkballcount != 1)
				{
               Log.LogError(nameof(PurchaseCycle) + ": Attempted vend from empty Gatcha");
               return;
				}
            Log.LogWarning("Vending last item from gatcha. Net count should equal 1: " + Networkballcount);
            DropPickup();
         }
         else
         {
            this.purchaseInteraction.SetUnavailableTemporarily(UNAVAILABLE_TIME);
            DropPickup();
         }
      }
      
      public void RemovePickup(int index)
		{
         if (index == 3)
         {
            NetworkIndex3 = PickupIndex.none;
         }
         else if (index == 2)
         {
            NetworkIndex2 = PickupIndex.none;
         }
         else if (index == 1)
         {
            NetworkIndex1 = PickupIndex.none;
         }
         else
         {
            Log.LogError(nameof(RemovePickup) + ": index out of range.");
         } 
		}

      private void doVendAnimation()
		{
         if (this.animator)
         {
            int layerIndex = this.animator.GetLayerIndex("Body");
            this.animator.PlayInFixedTime("Cycle" + Networkballcount, layerIndex);
         }
		}

      public void purchaseCorrection(Interactor activator)
		{
			if (purchaseInteraction) 
         {
            //this.controller.purchaseCorrection(activator, this.purchaseInteraction);
            Networkballcount--;
         }
			else
			{
            Log.LogError(nameof(purchaseCorrection) + ": PutchaseInteraction is null.");
			}
		}

      public PickupIndex CurrentIndex()
		{
         PickupIndex pickUp;
         if(Networkballcount == 3)
			{
            pickUp = NetworkIndex3;
			}
         else if (Networkballcount == 2)
			{
            pickUp = NetworkIndex2;
         }
         else if(Networkballcount == 1)
			{
            pickUp = NetworkIndex1;
         }
         else 
         {
            Log.LogError(nameof(CurrentIndex) + ": Index error. Netballcount = " + Networkballcount );
            pickUp = PickupIndex.none;
         }
         return pickUp;
		}


      #region NetworkingHelpers

      public PickupIndex NetworkIndex3
		{
			get
			{
            return Ball3;
			}
         [param:In]
			set
			{
            if (NetworkServer.localClientActive && !base.syncVarHookGuard)
            {
               base.syncVarHookGuard = true;
               this.SetNetIndex(value, 3);
               base.syncVarHookGuard = false;

            }
            base.SetSyncVar<PickupIndex>(value, ref this.Ball3, 4U);
         }
		}
      public PickupIndex NetworkIndex2
      {
         get
         {
            return Ball2;
         }
         [param: In]
         set
         {
            if (NetworkServer.localClientActive && !base.syncVarHookGuard)
            {
               base.syncVarHookGuard = true;
               this.SetNetIndex(value, 2);
               base.syncVarHookGuard = false;

            }
            base.SetSyncVar<PickupIndex>(value, ref this.Ball2, 8U);
         }
      }
      public PickupIndex NetworkIndex1
      {
         get
         {
            return Ball1;
         }
         [param: In]
         set
         {
            if (NetworkServer.localClientActive && !base.syncVarHookGuard)
            {
               base.syncVarHookGuard = true;
               this.SetNetIndex(value, 1);
               base.syncVarHookGuard = false;
            
            }
            base.SetSyncVar<PickupIndex>(value, ref this.Ball1, 16U);
               
         }
      }

      public void OnSyncPickup3(PickupIndex pickup)
		{
         Ball3 = pickup;
		}
      public void OnSyncPickup2(PickupIndex pickup)
      {
         Ball2 = pickup;
      }
      public void OnSyncPickup1(PickupIndex pickup)
      {
         Ball1 = pickup;
      }


      public void SetNetIndex(PickupIndex pickup, int index)
		{
         if (index == 3 && NetworkIndex3 != pickup)
         {
            OnSyncPickup3(pickup);
         }
         else if (index == 2 && NetworkIndex2 != pickup)
         {
            OnSyncPickup2(pickup);
         }
         else if (index == 1 && NetworkIndex1 != pickup)
         {
            OnSyncPickup1(pickup);
         }
         else if (index > 3 && index < 1)
         {
            Log.LogError(nameof(SetNetIndex) + ": Index outside of bounds. Given values was: " + index);
         }
      }

      public PickupIndex GetNetIndex(int index)
      {
         if (index == 3)
         {
            return NetworkIndex3;
         }
         else if (index == 2)
         {
            return NetworkIndex2;
         }
         else if (index == 1 )
         {
            return NetworkIndex1;
         }
         else
         {
            Log.LogError(nameof(SetNetIndex) + ": Index outside of bounds. Given values was: " + index);
            return PickupIndex.none;
         }
      }

      //public void ForceSetIndicies(PickupIndex[] indices)
      //{
      //   this.pickupIndicies = indices;
      //}

      //private void OnSyncPickupIndicies(PickupIndex newIndicies, int index)
      //{
      //   this.SetStoredIndicies(newIndicies, index);
      //   if (NetworkClient.active)
      //   {
      //
      //   }
      //}

      //public void SetStoredIndicies(PickupIndex newIndicies, int index)
      //{
      //   if (this.pickupIndicies[index] != newIndicies)
      //   {
      //      this.Networkindicies[index] = newIndicies;
      //   }
      //}


      //private void OnSyncPickupIndicies(PickupIndex[] newIndicies)
      //{
      //   this.SetStoredIndicies(newIndicies);
      //	if (NetworkClient.active)
      //	{
      //      
      //   }
      //}
      //

      //public void LoadStoredIndicides()
      //{
      //   for (int i = 0; i < 3; i++)
      //	{
      //      SetNetIndex(setIndicies[i], i + 1);
      //	}
      //}
      //
      //public void SetStoredIndicies(PickupIndex[] newIndicies)
      //{
      //   if (this.setIndicies != newIndicies)
      //   {
      //      this.setIndicies = newIndicies;
      //   } 
      //}

      public void SetIsEmpty(bool newIsEmpty)
		{
         if (this.isEmpty != newIsEmpty)
         {
            this.Networkisempty = newIsEmpty;
            if (newIsEmpty)
            {
               //Do Things when Empty
            }
         }
      }

      private void OnSyncBallCount(int count)
		{
         if(Networkballcount != count)
			{
            this.Networkballcount = count;
            if(this.Networkballcount == 0)
				{
               SetIsEmpty(true);
				}
			}
		}

      #endregion

      #region NetworkingVariables

      private void UNetVersion() {}

      //public PickupIndex[] Networkindicies
		//{
		//	get
		//	{
      //      return this.pickupIndicies;
		//	}
      //   [param: In]
		//	set
		//	{
      //      if (NetworkServer.localClientActive && !base.syncVarHookGuard)
      //      {
      //         base.syncVarHookGuard = true;
      //         this.OnSyncPickupIndicies(value[0], 0);
      //         this.OnSyncPickupIndicies(value[1], 1);
      //         this.OnSyncPickupIndicies(value[2], 2);
      //         base.syncVarHookGuard = false;
      //
      //      }
      //      base.SetSyncVar<PickupIndex[]>(value, ref this.pickupIndicies, 4U);
      //      base.SetSyncVar<PickupIndex[]>(value, ref this.pickupIndicies, 8U);
      //      base.SetSyncVar<PickupIndex[]>(value, ref this.pickupIndicies, 16U);
      //   }
		//}

      public bool Networkisempty
		{
			get
         {
            return this.isEmpty;
         }
         [param: In]
			set
			{
            if (NetworkServer.localClientActive && !base.syncVarHookGuard)
            {
               base.syncVarHookGuard = true;
               this.SetIsEmpty(value);
               base.syncVarHookGuard = false;
            }
            base.SetSyncVar<bool>(value, ref this.isEmpty, 1U);
         }
		}

      public int Networkballcount
      {
         get
         {
            return this.ballCount;
         }
         [param: In]
         set
         {
            base.SetSyncVar<int>(value, ref this.ballCount, 2U);
         }
      }

		#endregion

		#region Networking
		public override bool OnSerialize(NetworkWriter writer, bool forceAll)
		{
			if (forceAll)
			{
            writer.Write(this.isEmpty);
            writer.WritePackedUInt32((uint)this.ballCount);
            NetworkUtils.WritePickupIndex(writer, this.Ball3);
            NetworkUtils.WritePickupIndex(writer, this.Ball2);
            NetworkUtils.WritePickupIndex(writer, this.Ball1);

            return true;
			}
         bool flag = false;
         if((base.syncVarDirtyBits & 1U) != 0)
			{
				if (!flag)
				{
               writer.WritePackedUInt32(base.syncVarDirtyBits);
               flag = true;
				}
            writer.Write(this.isEmpty);
			}
         if((base.syncVarDirtyBits & 2U) != 0)
			{
				if (!flag)
				{
               writer.WritePackedUInt32(base.syncVarDirtyBits);
               flag = true;
				}
            writer.WritePackedUInt32((uint)this.ballCount);
			}
         if((base.syncVarDirtyBits & 4U) != 0)
			{
				if (!flag)
				{
               writer.WritePackedUInt32(base.syncVarDirtyBits);
               flag = true;
				}
            NetworkUtils.WritePickupIndex(writer, this.Ball3);
         }
         if ((base.syncVarDirtyBits & 8U) != 0)
         {
            if (!flag)
            {
               writer.WritePackedUInt32(base.syncVarDirtyBits);
               flag = true;
            }
            NetworkUtils.WritePickupIndex(writer, this.Ball2);
         }
         if ((base.syncVarDirtyBits & 16U) != 0)
         {
            if (!flag)
            {
               writer.WritePackedUInt32(base.syncVarDirtyBits);
               flag = true;
            }
            NetworkUtils.WritePickupIndex(writer, this.Ball1);
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
            this.isEmpty = reader.ReadBoolean();
            this.ballCount = (int)reader.ReadPackedUInt32();
            this.Ball1 = NetworkUtils.ReadPickupIndex(reader);
            this.Ball2 = NetworkUtils.ReadPickupIndex(reader);
            this.Ball3 = NetworkUtils.ReadPickupIndex(reader);
            //this.pickupIndicies = NetworkUtils.ReadPickupIndicies(reader);
            return;
			}
         int num = (int)reader.ReadPackedUInt32();
         if((num & 1) != 0) 
         {
            this.SetIsEmpty(reader.ReadBoolean());
         }
         if((num & 2) != 0)
			{
            this.OnSyncBallCount((int)reader.ReadPackedUInt32());
			}
         if((num & 4) != 0)
			{
            this.OnSyncPickup3(NetworkUtils.ReadPickupIndex(reader));
			}
         if ((num & 8) != 0)
         {
            this.OnSyncPickup2(NetworkUtils.ReadPickupIndex(reader));
         }
         if ((num & 16) != 0)
         {
            this.OnSyncPickup1(NetworkUtils.ReadPickupIndex(reader));
         }
      }
		#endregion
	}
}