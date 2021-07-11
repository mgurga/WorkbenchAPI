using System.Collections.Generic;
using TMPro;
using HarmonyLib;

namespace WorkbenchAPI { 
public class Patches
{
    [HarmonyPatch(typeof(ItemManager), "InitAllItems")]
    public static class InitAllItems_InjectItems_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref Dictionary<int, InventoryItem> ___allItems, ref InventoryItem[] ___allScriptableItems)
        {
            int basemaxid = ___allItems.Count;

            for (int i = 0; i < WorkbenchAPI.newItems.Count; i++)
            {
                InventoryItem newitem = new InventoryItem();
                newitem.Copy(___allItems[0], 1);
                newitem.type = WorkbenchAPI.newItems[i].gameItem.type;
                newitem.tag = WorkbenchAPI.newItems[i].gameItem.tag;
                newitem.rarity = WorkbenchAPI.newItems[i].gameItem.rarity;
                newitem.important = WorkbenchAPI.newItems[i].gameItem.important;
                newitem.name = WorkbenchAPI.newItems[i].gameItem.name;
                newitem.description = WorkbenchAPI.newItems[i].gameItem.description;
                newitem.tier = WorkbenchAPI.newItems[i].gameItem.tier;
                newitem.requirements = WorkbenchAPI.newItems[i].gameItem.requirements;
                newitem.unlockWithFirstRequirementOnly = true;
                newitem.craftAmount = 1;
                newitem.buildable = false;

                if (WorkbenchAPI.newItems[i].gameItem.requirements == null)
                {
                    newitem.requirements = new InventoryItem.CraftRequirement[0];
                }
                else
                {
                    InventoryItem.CraftRequirement[] outreq = new InventoryItem.CraftRequirement[0];
                    foreach (InventoryItem.CraftRequirement req in WorkbenchAPI.newItems[i].gameItem.requirements)
                    {
                        if (req.item.name.ToCharArray()[0] == ':')
                        {
                            foreach (InventoryItem item in ___allItems.Values)
                            {
                                if (req.item.name.Substring(1) == item.name)
                                {
                                    req.item = item;
                                }
                            }
                        }

                        outreq.AddToArray(req);
                    }
                }

                newitem.requirements = WorkbenchAPI.newItems[i].gameItem.requirements;
                newitem.id = basemaxid + i;

                WorkbenchAPI.newItems[i].gameItem.id = basemaxid + i;
                WorkbenchAPI.newItems[i].assignedId = basemaxid + i;
                ___allItems.Add(basemaxid + i, newitem);
                ___allScriptableItems.AddToArray(newitem);

            }

            WorkbenchAPI.log.LogInfo("injected " + (___allItems.Count - basemaxid) + " item(s)");

            // // save brief item list to file
            //string itemdebug = "";
            //foreach (KeyValuePair<int, InventoryItem> kvp in ___allItems)
            //{
            //    itemdebug += kvp.Key + " : " + kvp.Value.name + "\n";
            //    itemdebug += "L desc: " + kvp.Value.description + "\n";
            //    itemdebug += "L important: " + kvp.Value.important + "\n";
            //    itemdebug += "L rarity: " + kvp.Value.rarity + "\n";
            //    itemdebug += "L tag: " + kvp.Value.tag + "\n";
            //    itemdebug += "L type: " + kvp.Value.type + "\n";
            //    itemdebug += "L tier: " + kvp.Value.tier + "\n";
            //    itemdebug += "L material: " + kvp.Value.material + "\n";
            //    itemdebug += "L crafting: " + kvp.Value.requirements + "\n";
            //}
            // File.WriteAllText("items", itemdebug);
        }
    }

    [HarmonyPatch(typeof(CraftingUI), "Awake")]
    public static class CraftingUI_AddItemsToCraftingUI_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref CraftingUI __instance)
        {
            foreach (WbItem item in WorkbenchAPI.newItems)
            {
                bool isAnvil = __instance.tabParent.GetChild(3).GetComponentInChildren<TextMeshProUGUI>().text.Equals("Armor");

                if ((item.hand && __instance.handCrafts) || (item.anvil && isAnvil) || (item.workbench && !isAnvil && !__instance.handCrafts))
                {
                    WorkbenchAPI.log.LogInfo("Added " + item.gameItem.name + " to station.");
                    __instance.tabs[item.tabId].items.AddToArray(item.gameItem);

                    InventoryItem[] items = __instance.tabs[item.tabId].items;
                    InventoryItem[] array = new InventoryItem[items.Length + 1];
                    items.CopyTo(array, 0);
                    array[items.Length] = item.gameItem;
                    __instance.tabs[item.tabId].items = array;
                }
            }
        }
    }
}
}