using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.Stats;

namespace Axolotl
{
	internal static class StatsSheetExposer
	{
		// Start is called before the first frame update
		public static void OnPurchase<T>(CharacterBody characterBody, CostTypeIndex costType, T statDefsToIncrement) where T : IEnumerable<StatDef>
		{
			StatSheet statSheet = PlayerStatsComponent.FindBodyStatSheet(characterBody);
			if (statSheet == null)
			{
				return;
			}
			StatDef statDef = null;
			StatDef statDef2 = null;
			switch (costType)
			{
				case CostTypeIndex.Money:
					statDef = StatDef.totalGoldPurchases;
					statDef2 = StatDef.highestGoldPurchases;
					break;
				case CostTypeIndex.PercentHealth:
					statDef = StatDef.totalBloodPurchases;
					statDef2 = StatDef.highestBloodPurchases;
					break;
				case CostTypeIndex.LunarCoin:
					statDef = StatDef.totalLunarPurchases;
					statDef2 = StatDef.highestLunarPurchases;
					break;
				case CostTypeIndex.WhiteItem:
					statDef = StatDef.totalTier1Purchases;
					statDef2 = StatDef.highestTier1Purchases;
					break;
				case CostTypeIndex.GreenItem:
					statDef = StatDef.totalTier2Purchases;
					statDef2 = StatDef.highestTier2Purchases;
					break;
				case CostTypeIndex.RedItem:
					statDef = StatDef.totalTier3Purchases;
					statDef2 = StatDef.highestTier3Purchases;
					break;
			}
			statSheet.PushStatValue(StatDef.totalPurchases, 1UL);
			statSheet.PushStatValue(StatDef.highestPurchases, statSheet.GetStatValueULong(StatDef.totalPurchases));
			if (statDef != null)
			{
				statSheet.PushStatValue(statDef, 1UL);
				if (statDef2 != null)
				{
					statSheet.PushStatValue(statDef2, statSheet.GetStatValueULong(statDef));
				}
			}
			if (statDefsToIncrement != null)
			{
				foreach (StatDef statDef3 in statDefsToIncrement)
				{
					if (statDef3 != null)
					{
						statSheet.PushStatValue(statDef3, 1UL);
					}
				}
			}
		}
	}
}
