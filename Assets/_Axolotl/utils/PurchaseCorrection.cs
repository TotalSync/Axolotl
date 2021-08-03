using System.Collections;
using System.Collections.Generic;
using RoR2;
using RoR2.Stats;
using UnityEngine;

namespace Axolotl
{
	public static class StatsCorrection
	{
		/*
		public static void purchaseCorrection<T>(Interactor activator, PurchaseInteraction interaction, T shop)
		{
			if (!interaction.CanBeAffordedByInteractor(activator))
			{
				return;
			}
			CharacterBody component = activator.GetComponent<CharacterBody>();
			CostTypeDef costTypeDef = CostTypeCatalog.GetCostTypeDef(shop.costType);
			ItemIndex itemIndex = ItemIndex.None;
			if (shop)
			{
				if (shop.CurrentPickupIndex() == PickupIndex.none)
				{
					Log.LogError(nameof(purchaseCorrection) + ": PickupIndex is none.");
				}
				else
				{
					PickupDef pickupDef = PickupCatalog.GetPickupDef(shop.CurrentPickupIndex());
					itemIndex = ((pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None);
				}
			}
			CostTypeDef.PayCostResults payCostResults = costTypeDef.PayCost(interaction.cost, activator, shop.gameObject, Run.instance.runRNG, itemIndex);
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
			/*
			#endregion
			IEnumerable<StatDef> statDefsToIncrement = interaction.purchaseStatNames.Select(new Func<string, StatDef>(StatDef.Find));
			StatsSheetExposer.OnPurchase<IEnumerable<StatDef>>(component, shop.costType, statDefsToIncrement);
			//interaction.onPurchase.Invoke(activator);
			interaction.lastActivator = activator;
		}
			*/
	}
}
