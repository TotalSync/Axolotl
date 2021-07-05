using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using BepInEx;
using BepInEx.Configuration;

namespace Axolotl
{
    [BepInDependency("com.bepis.r2api")]
    public class ConfigHandler : MonoBehaviour
    {
        // TODO: integrate item balance values into the cfg.

        private static ConfigFile config { get; set; }
        private List<string> item_ban_list = new List<string>();
        //private List<ConfigEntry<bool>> entry_list = new List<ConfigEntry<bool>>();
        
        private static string item_section = "Item Section";
        //private static string item_enable = "Enable Item";

        public ConfigHandler()
        {
            config = new ConfigFile(Paths.ConfigPath + "\\sync_cache.cfg", true);
        }


        public List<string> loadConfig(List<Item_Base> item_list)
        {   //config.Bind<T>(section, key, value, comment);
            foreach (var item in item_list)
            {
                trySetItemConfig<bool>(item_section, item.id, true, item.name_long);
            }
            foreach (var entry in item_ban_list)
            {
                var ind = item_list.FindIndex(x => x.id == entry);
                if (ind != -1) {
                    item_list.RemoveAt(ind);
                }
            }
            return item_ban_list;
        }

        internal void trySetItemConfig<T>(string section, string key, bool value, string comment)
        {
            var item_config = config.Bind<bool>(section, key, value, comment);
            if (!item_config.Value)
            { 
                item_ban_list.Add(item_config.Definition.Key);
                Log.LogInfo(item_config.Definition.Key);
            }
        }

        private void createDefConfig()
        {

        }
    }
}
