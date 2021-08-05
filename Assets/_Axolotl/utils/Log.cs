using BepInEx.Logging;
using RoR2;

namespace Axolotl
{
    internal static class Log
    {
        internal static ManualLogSource _logSource;
        internal static int debug_count = 0;

        internal static void Init(ManualLogSource logSource)
        {
            _logSource = logSource;
        }

		#region Initial Functions
        internal static void LogDebug(object data) => _logSource.LogDebug(data);
        internal static void LogError(object data) => _logSource.LogError(data);
        internal static void LogFatal(object data) => _logSource.LogFatal(data);
        internal static void LogInfo(object data) => _logSource.LogInfo(data);
        internal static void LogMessage(object data) => _logSource.LogMessage(data);
        internal static void LogWarning(object data) => _logSource.LogWarning(data);
		#endregion

		#region Added Functions
		internal static bool LogInfoBool(object data) 
        {
            _logSource.LogDebug(data);
            return true;
        }
		internal static bool printItemBool(Item_Base item)
        {
            Log.LogDebug(nameof(printItemBool) + " for " + item.item_def.nameToken);
            Log.LogDebug("----------------------------------");
            Log.LogDebug("ID________: \"" + item.id + "\"");
            Log.LogDebug("NAME TOKEN: \"" + item.item_def.nameToken + "\"");
            Log.LogDebug("Bool: " + ((item.id == item.item_def.nameToken)? "true": "false"));
            //Log.LogInfo("IDR: " + item.idr.ToString());
            Log.LogDebug("----------------------------------\n");
            return true;
        }
        internal static void Here()
        {
            Log.LogWarning("Here at: " + debug_count);
            debug_count++;
        }

        internal static void Here(Interactor interactor)
        {
            Here();
        }
		#endregion
	}
}