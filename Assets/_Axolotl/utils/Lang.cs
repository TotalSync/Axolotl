using R2API;
using R2API.Utils;

[R2APISubmoduleDependency(nameof(LanguageAPI))]
internal static class Lang
{

	internal static readonly string modPrefix = "AXOLOTL_";


   internal static void LoadLanguage()
	{

		#region Items
		//Items should be overwritten in their custom object
		#endregion

		#region Equipment
		#endregion

		#region Interactables
		#region Target Multi Shop
		Init("TARGET_MULTISHOP", "Target Multishop");
		Init("TARGET_MULTISHOP_CONTEXT", "Purchase");
		#endregion
		#region Gatcha Shop
		Init("GATCHA_SHOP", "Item Gatcha Machine");
		Init("GATCHA_SHOP_CONTEXT", "Buy a capsul. If only there was a way to tell what you will get...");
		#endregion
		#region Busted Scrapper
		#endregion
		#endregion

		#region Tanker
		#region Skins
		#endregion
		#region Passives
		#endregion
		#region Primary
		#endregion
		#region Secondary
		#endregion
		#region Utility
		#endregion
		#region Special
		#endregion

		#endregion

		#region Acievements
		#endregion

	}

	internal static void Init(string token, string content)
	{
		LanguageAPI.Add(modPrefix + token, content);
	}
}
