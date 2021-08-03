using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using RoR2;
using RoR2.Stats;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace Axolotl
{
   public class GatchaShopController : NetworkBehaviour, IHologramContentProvider
   {
      public static readonly int CAPACITY = 3;

      public GameObject shopPrefab;

      public Transform shopPosition;
      private GameObject shopGameObject;

      [SyncVar]
      private bool available = true;
      [SyncVar]
      private int cost;
      

      public ShopType type;
      private PickupIndex[] storedItems;
      private ItemTier[] storedRarities;


      private Xoroshiro128Plus rng;

      public int baseCost = 10;
      public CostTypeIndex costType;

      void Awake()
      {
         this.storedRarities = new ItemTier[CAPACITY];
         RollType();
         RollRarity();
         if (NetworkServer.active)
         {
            this.rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
         }
      }

      // Start is called before the first frame update
      void Start()
      {
         if (Run.instance && NetworkServer.active)
         {
            this.GenerateShop();
            this.Networkcost = Run.instance.GetDifficultyScaledCost(this.baseCost);
            if (this.shopGameObject != null)
            {
               PurchaseInteraction component = this.shopGameObject.GetComponent<PurchaseInteraction>();
               component.Networkcost = this.cost;
               component.costType = this.costType;
            }
         }
      }

      private void OnDestroy()
		{
         UnityEngine.Object.Destroy(this.shopGameObject);
		}

      private void GenerateShop()
		{
         storedItems = new PickupIndex[CAPACITY];
         for(int i = 0; i < storedItems.Length; i++)
			{
            var dropList = selectiveDropTableController.getDropList(this.type, this.storedRarities[i]);
            while (dropList.Count == 0)
				{
               RollRarity(i);
               dropList = selectiveDropTableController.getDropList(this.type, this.storedRarities[i]);
            }
            this.storedItems[i] = this.rng.NextElementUniform<PickupIndex>(dropList);
			}

         if (this.shopPrefab)
			{
            this.shopGameObject = UnityEngine.GameObject.Instantiate<GameObject>(this.shopPrefab, this.shopPosition);
            AddModMetal();
            var component = this.shopGameObject.GetComponentInChildren<GatchaShopBehaviour>();
            if (component)
				{
               //Loop through stored indicies and network required indicies.
               for(int i = 0; i < CAPACITY; i++)
					{
                  component.SetNetIndex(storedItems[i], i + 1);
					}
               component.Networkisempty = false;
               component.Networkballcount = CAPACITY;
				}
				else
				{
               Log.LogError(nameof(GenerateShop) + ": Unable to find GatchaShopBehavior component");
				}
				if (NetworkServer.active)
				{
               NetworkServer.Spawn(this.shopGameObject);
				}
            var newComponent = this.shopGameObject.GetComponent<PurchaseInteraction>();
            if (newComponent)
            {
               newComponent.onPurchase.AddListener(new UnityAction<Interactor>(base.transform.parent.GetComponentInChildren<GatchaShopBehaviour>().purchaseCorrection));
				}
				else
				{
               Log.LogError(nameof(GenerateShop) + ": Unable to find PurchaseInteraction component");
            }
         }
         else
			{
            Log.LogError(nameof(GenerateShop) + ": Unable to find Prefab");
			}
		}

      private GameObject[] loadBalls()
      {
         return null;
      }

      

      public void purchaseCorrection(Interactor activator, PurchaseInteraction interaction)
		{
         var shop = this.shopGameObject.GetComponent<GatchaShopBehaviour>();
         if (!interaction.CanBeAffordedByInteractor(activator))
         {
            return;
         }
         CharacterBody component = activator.GetComponent<CharacterBody>();
         CostTypeDef costTypeDef = CostTypeCatalog.GetCostTypeDef(this.costType);
         ItemIndex itemIndex = ItemIndex.None;
         if (shop)
         {
            if (shop.CurrentIndex() == PickupIndex.none)
            {
               Log.LogError(nameof(purchaseCorrection) + ": PickupIndex is none.");
            }
            else
            {
               PickupDef pickupDef = PickupCatalog.GetPickupDef(shop.CurrentIndex());
               itemIndex = ((pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None);
            }
         }
         CostTypeDef.PayCostResults payCostResults = costTypeDef.PayCost(this.cost, activator, shop.gameObject, this.rng, itemIndex);
         //Leaving this code in case an item is required as a cost from the terminals
         #region ItemConsumeCode
         /*foreach (ItemIndex itemIndex2 in payCostResults.itemsTaken)
      {
         PurchaseInteraction.CreateItemTakenOrb(component.corePosition, base.gameObject, itemIndex2);
         if (itemIndex2 != itemIndex)
         {
            Action<PurchaseInteraction, Interactor> action = PurchaseInteraction.onItemSpentOnPurchase;
            if (action != null)
            {
               action(purchaseInteraction, activator);
            }
         }
      }
      foreach (EquipmentIndex arg in payCostResults.equipmentTaken)
      {
         Action<PurchaseInteraction, Interactor, EquipmentIndex> action2 = PurchaseInteraction.onEquipmentSpentOnPurchase;
         if (action2 != null)
         {
            action2(purchaseInteraction, activator, arg);
         }
      }*/
         #endregion
         IEnumerable<StatDef> statDefsToIncrement = interaction.purchaseStatNames.Select(new Func<string, StatDef>(StatDef.Find));
         StatsSheetExposer.OnPurchase<IEnumerable<StatDef>>(component, this.costType, statDefsToIncrement);
         //Invoking this Actoion Crashes the game. No idea why. Code works as is, will investigate later.
         //interaction.onPurchase.Invoke(activator);
         //--------------------------------------------
         interaction.lastActivator = activator;
      }


      private void AddModMetal()
		{
			if (selectiveDropTableController.isModded(this.type))
			{
            var obj = this.shopGameObject.transform.Find("ModEmbellishments");
            if (obj)
            {
               obj.gameObject.SetActive(true);
            }
			}
		}


      private void RollRarity()
      {
         for (int i = 0; i < 3; i++)
         {
            this.storedRarities[i] = selectiveDropTableController.rollItemRarity();
         }
      }

      private void RollRarity(int i)
      {
         this.storedRarities[i] = selectiveDropTableController.rollItemRarity();
      }

      private void RollType()
      {
         this.type = selectiveDropTableController.rollShopType();
         while (selectiveDropTableController.isEquipment(this.type))
         {
            this.type = selectiveDropTableController.rollShopType();
         }
      }


      #region Holograms
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
      #endregion


      #region Networking

      private void UNetVersion() { }

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
         if((base.syncVarDirtyBits & 1U) != 0U)
			{
				if (!flag)
				{
               writer.WritePackedUInt32(base.syncVarDirtyBits);
               flag = true;
				}
            writer.Write(this.available);
			}
         if((base.syncVarDirtyBits & 2U) != 0U)
			{
            if(!flag)
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
         if((num & 1) != 0)
			{
            this.available = reader.ReadBoolean();
			}
         if((num & 2) != 0)
			{
            this.cost = (int)reader.ReadPackedUInt32();
			}
		}


		#endregion
	}
}
