using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Stats;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace Axolotl
{
    [BepInDependency("com.bepis.r2api")]
    public class targetMultiShopController : NetworkBehaviour, IHologramContentProvider
    {
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

		public ItemTier itemTier;
		public int baseCost;
		public CostTypeIndex costType;

		private Xoroshiro128Plus rng;


        public targetMultiShopController(Transform[] transforms, GameObject gameObject) 
		{
			this.terminalPrefab = gameObject;
			this.terminalPositions = new Transform[3];
			//Correction for root transform unintentionally added.
			for (int i = 0; i < 3; i++)
            {
				this.terminalPositions[i] = transforms[i+1];
            }
			rollType();
			rollRarity();
			if (Run.instance)
			{
				this.rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
				this.GenerateTerminals();
				if (NetworkServer.active)
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
			if (this.terminalPositions == null)
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
					var dropList = selectiveDropTableController.getDropList(this.type, this.itemTier);
					while (dropList == null)
                    {
						this.rollType();
						this.rollRarity();
						dropList = selectiveDropTableController.getDropList(this.type, this.itemTier);
                    }
					PickupIndex pickupIndex = this.rng.NextElementUniform<PickupIndex>(dropList);

					bool newHidden = this.hideDisplayContent && i != 0;
					newHidden = rng.nextBool & newHidden;
					if(terminalPrefab != null)
                    {
						GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.terminalPrefab,
							this.terminalPositions[i].position, this.terminalPositions[i].rotation);
						this.terminalGameObjects[i] = gameObject;
                        if (selectiveDropTableController.isModded(this.type))
                        {
							gameObject.transform.Find("targetModEmbellishments").gameObject.SetActive(true);
                        }
						var component = gameObject.GetComponentInChildren<targetMultiShopBehavior>();
						if (component != null)
						{
							component.SetPickupIndex(pickupIndex, newHidden);
							component.controller = this;
						}
						else 
						{
							Log.LogError(nameof(GenerateTerminals) + ": Component not Found.");
							
							if (NetworkServer.active)
							{
								NetworkServer.Spawn(gameObject);
							}	
						}
						gameObject.GetComponent<PurchaseInteraction>().onPurchase.AddListener(new UnityAction<Interactor>(gameObject.GetComponent<targetMultiShopBehavior>().purchaseCorrection));
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
			foreach (GameObject gameObject in this.terminalGameObjects)
			{
				if (gameObject != caller)
                {
					var purchase = gameObject.GetComponent<PurchaseInteraction>();
					var behavior = gameObject.GetComponent<targetMultiShopBehavior>();
					purchase.Networkavailable = false;
					behavior.SetNoPickup();
					//component.closeChestAnimation();
                }
			}
			this.Networkavailable = false;
		}

		public void purchaseCorrection(Interactor activator, PurchaseInteraction interaction)
		{
			if (!interaction.CanBeAffordedByInteractor(activator))
			{
				return;
			}
			CharacterBody component = activator.GetComponent<CharacterBody>();
			CostTypeDef costTypeDef = CostTypeCatalog.GetCostTypeDef(this.costType);
			ItemIndex itemIndex = ItemIndex.None;
			ShopTerminalBehavior component2 = base.GetComponent<ShopTerminalBehavior>();
			if (component2)
			{
				PickupDef pickupDef = PickupCatalog.GetPickupDef(component2.CurrentPickupIndex());
				itemIndex = ((pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None);
			}
			CostTypeDef.PayCostResults payCostResults = costTypeDef.PayCost(this.cost, activator, base.gameObject, this.rng, itemIndex);
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
			IEnumerable<StatDef> statDefsToIncrement = interaction.purchaseStatNames.Select(new Func<string, StatDef>(StatDef.Find));
			StatsSheetExposer.OnPurchase<IEnumerable<StatDef>>(component, this.costType, statDefsToIncrement);
			interaction.onPurchase.Invoke(activator);
			interaction.lastActivator = activator;
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

		private void rollRarity()
        {
            if (!selectiveDropTableController.isEquipment(this.type))
            {
				this.itemTier = selectiveDropTableController.rollItemRarity();	
            }
        }

		private void rollType()
        {
			this.type = selectiveDropTableController.rollShopType();
        }

        #region Networking

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
