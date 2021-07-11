using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;


namespace WorkbenchAPI
{
    public class WbItem
    {
        public WbItem(InventoryItem i) { this.gameItem = i; }

        public InventoryItem gameItem; // Muck item
        public int tabId = 0; // station tab 
                              // workbench: 0 = Basic, 1 = Tools, 2 = Stations, 3 = Build
                              // anvil: 0 = Other, 1 = Tools, 2 = Weapons, 3 = Armor
        public int assignedId = -1; // id of item when it gets added, only check after PatchItems
        public bool workbench = true; // if the item is craftable in a workbench
        public bool anvil = false; // if the item is craftable in a anvil
        public bool hand = false; // if the item is craftable in your hand (like the workbench)
    }

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class WorkbenchAPI : BaseUnityPlugin
    {
        public const string pluginGuid = "xyz.mgurga.workbenchapi";
        public const string pluginName = "WorkbenchAPI";
        public const string pluginVersion = "1.0.0";
        public static ManualLogSource log;

        public static List<WbItem> newItems = new List<WbItem>();

        void Awake()
        {
            log = Logger;
            log.LogInfo("Workbench API loaded");
            //CreateItem("Test Item", "Just a test item");
            //PatchItems();
        }

        public void PatchItems()
        {
            var harmony = new Harmony(pluginGuid);
            harmony.PatchAll();
        }

        public void CreateItem(string name,
                               string description,
                               int tabId = 0,
                               InventoryItem.ItemType type = InventoryItem.ItemType.Item,
                               InventoryItem.ItemTag tag = InventoryItem.ItemTag.None,
                               InventoryItem.ItemRarity rarity = InventoryItem.ItemRarity.Common,
                               bool important = false,
                               int tier = 0,
                               InventoryItem.CraftRequirement[] craftRequirements = null)
        {
            InventoryItem i = ScriptableObject.CreateInstance<InventoryItem>();
            i.type = type;
            i.tag = tag;
            i.rarity = rarity;
            i.important = important;
            i.name = name;
            i.description = description;
            i.tier = tier;
            i.craftable = craftRequirements != null;
            i.craftAmount = 1;
            i.buildable = false;
            i.requirements = craftRequirements ?? new InventoryItem.CraftRequirement[0];
            i.unlockWithFirstRequirementOnly = true;
            if (craftRequirements == null)
            {
                i.requirements = new InventoryItem.CraftRequirement[] { new InventoryItem.CraftRequirement() { item = new InventoryItem() { name = ":Wood" }, amount = 1 } };
            }
            else
            {
                i.requirements = craftRequirements;
            }

            WbItem wbi = new WbItem(i) { tabId = tabId };

            newItems.Add(wbi);
        }

        public void CreateItem(WbItem i)
        {
            newItems.Add(i);
        }

        public void CreateItem(InventoryItem i)
        {
            newItems.Add(new WbItem(i));
        }
    }
}