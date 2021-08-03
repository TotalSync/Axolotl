using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System;
using RoR2;
using RoR2.Stats;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace Axolotl
{
	/*
	This class is a bit of a weird class. Its counterpart is the behaviour class. This class will spawn children at given locations
	set in the TargetMultiHolder Prefab object. This script is build for use with prefab objects, so not all fields are set explicitly
	in this class. I also do not initialize certain things in the children when I create them, because they are overwritten when the
	behaviours are recreated.

	Flow:
		1.) Get a new RNG Value
		2.) Generate the Shops
		3.) Wait until a OnPurchase event fires
		4.) Mark all children closed, and their purchase events closed
	 */


   //[BepInDependency("com.bepis.r2api")]
	public class TargetMultiShopController : NetworkBehaviour, IHologramContentProvider
   {
		public GameObject terminalPrefab;
				
		public Transform[] terminalPositions;
		private GameObject[] terminalGameObjects;

		[Tooltip("Whether or not there's a chance the item contents are replaced with a '?'")]
		public bool hideDisplayContent = true;
		
		[SyncVar]
		private int cost;

		[SyncVar]
		private bool available = true;

      public ShopType type;
		public ItemTier itemTier;

		public int baseCost = 10;
		public CostTypeIndex costType = CostTypeIndex.Money;

		private List<PickupIndex> dropList;
		private Xoroshiro128Plus rng;

		private void Awake()
      {
			RollType();
			RollRarity();
			if (NetworkServer.active)
			{
				this.rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
				this.GenerateTerminals();
			}
		}

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

      //This will populate all of the designated locations with selectiveMultiShops
		private void GenerateTerminals()
		{
			this.terminalGameObjects = new GameObject[this.terminalPositions.Length];
			dropList = selectiveDropTableController.getDropList(this.type, this.itemTier);
			//There is a possibility that a list can be empty due to there being no mods.
			//This is a stopgap to prevent that.
			while (dropList.Count == 0)
			{
				this.RollType();
				this.RollRarity();
				dropList = selectiveDropTableController.getDropList(this.type, this.itemTier);
			}
			if (this.terminalPositions.Length == 0)
			    {
				Log.LogError(nameof(GenerateTerminals) + ": terminal positions is empty.");
				return;
			    }
			for (int i = 0; i < this.terminalPositions.Length; i++)
			{
				if ( terminalPositions[i] == null || terminalPositions[i].position == null )
				{
					Log.LogError(nameof(GenerateTerminals) + ": Terminal Position at: " + i + " is null.");
				 	Log.LogError(terminalPositions[i].ToString() + " " + terminalPositions[i].ToString());
				}
				else
				{
					PickupIndex pickupIndex = this.rng.NextElementUniform<PickupIndex>(dropList);
					if (pickupIndex == PickupIndex.none) { Log.LogError(nameof(GenerateTerminals) + ": Terminal attempted to be generated with a None."); }

				 	bool newHidden = this.hideDisplayContent && i != 0;
				 	newHidden = rng.nextBool & newHidden;
				 	if(terminalPrefab != null)
				   {
				 		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.terminalPrefab,
				 			this.terminalPositions[i]);
				 		this.terminalGameObjects[i] = gameObject;
						if (selectiveDropTableController.isModded(this.type))
				      {
				 			gameObject.transform.Find("targetModEmbellishments").gameObject.SetActive(true);
				      }
				 		var component = gameObject.GetComponentInChildren<TargetMultiShopBehaviour>();
				 		if (component != null)
				 		{
							component.SetHasBeenPurchased(false);
				 			component.SetPickupIndex(pickupIndex, newHidden);
				 		}
				 		else 
				 		{
				 			Log.LogError(nameof(GenerateTerminals) + ": TargetMultiShopBehaviour Component not Found.");
				 			
				 		}
				 		if (NetworkServer.active)
				 		{
				 			NetworkServer.Spawn(gameObject);
				 		}	
				 		gameObject.GetComponent<PurchaseInteraction>().onPurchase.AddListener(new UnityAction<Interactor>(gameObject.GetComponent<TargetMultiShopBehaviour>().purchaseCorrection));
				   } 
				 	else
				   {
				 		Log.LogError(nameof(GenerateTerminals) + ": Terminal Prefab is null");
				   }
				}
			}
		}
		
		public void DisableAllTerminals(GameObject caller)
		{
			this.Networkavailable = false;
			foreach (GameObject gameObject in this.terminalGameObjects)
			{
				if (gameObject != caller)
            {
					gameObject.GetComponent<PurchaseInteraction>().Networkavailable = false;
					gameObject.GetComponent<TargetMultiShopBehaviour>().SetNoPickup();
				}
			}
		}

		public void purchaseCorrection(Interactor activator, PurchaseInteraction interaction, TargetMultiShopBehaviour shop)
		{
			if (!interaction.CanBeAffordedByInteractor(activator))
			{
				return;
			}
			CharacterBody component = activator.GetComponent<CharacterBody>();
			CostTypeDef costTypeDef = CostTypeCatalog.GetCostTypeDef(this.costType);
			ItemIndex itemIndex = ItemIndex.None;
			if (shop)
			{
				if (shop.CurrentPickupIndex() == PickupIndex.none)
				{
					Log.LogError(nameof(purchaseCorrection) + ": PickupIndex is none.");
				} else
				{
					PickupDef pickupDef = PickupCatalog.GetPickupDef(shop.CurrentPickupIndex());
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

		private void RollRarity()
        {
            if (!selectiveDropTableController.isEquipment(this.type))
            {
				this.itemTier = selectiveDropTableController.rollItemRarity();	
            }
        }

		private void RollType()
      {
			this.type = selectiveDropTableController.rollShopType();
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
       #endregion

   }
}
