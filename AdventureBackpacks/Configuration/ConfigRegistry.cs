using System;
using BepInEx.Configuration;
using UnityEngine;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;

namespace AdventureBackpacks.Configuration
{
    public class ConfigRegistry : ConfigSyncBase
    {
        //Configuration Entry Privates
        internal static ConfigEntry<KeyboardShortcut> HotKeyOpen;
        internal static ConfigEntry<KeyboardShortcut> HotKeyDrop;
        internal static ConfigEntry<bool> OpenWithInventory;
        internal static ConfigEntry<bool> OpenWithHoverInteract;
        internal static ConfigEntry<bool> OpenAlongsideContainer;
        internal static ConfigEntry<float> AlongsidePanelGap;
        internal static ConfigEntry<float> AlongsidePanelOffsetY;
        internal static ConfigEntry<float> AlongsidePanelScale;
        internal static ConfigEntry<bool> CloseInventory;
        internal static ConfigEntry<bool> OutwardMode;
        internal static ConfigEntry<bool> ReplaceShader;
        
        public static Waiting Waiter;

        public ConfigRegistry(IPluginInfo mod): base(mod)
        {
            //Waiting For Startup
            Waiter = new Waiting();

            InitializeConfigurationSettings();
        }

        public sealed override void InitializeConfigurationSettings()
        {
            if (_config == null)
                return;
            
            //User Configs




            UnsyncedConfig("Local Config", "Open Backpack", new KeyboardShortcut(KeyCode.I),
                new ConfigDescription("Hotkey to open backpack.", null, new ConfigurationManagerAttributes{ Order = 3 }), ref HotKeyOpen);
            
            UnsyncedConfig("Local Config", "Quickdrop Backpack", new KeyboardShortcut(KeyCode.Y),
                new ConfigDescription("Hotkey to quickly drop backpack while on the run.",
                    null,
                    new ConfigurationManagerAttributes { Order = 1 }),ref HotKeyDrop);
           
            UnsyncedConfig("Local Config", "Open with Inventory", false,
                new ConfigDescription("If enabled, both backpack and inventory will open when Inventory is opened.",
                    null, new ConfigurationManagerAttributes { Order = 3 }),ref OpenWithInventory);
            
            UnsyncedConfig("Local Config", "Open with Interactive Hover", false,
                new ConfigDescription("If enabled, backpack will only open while hovering over equipped backpack and pressing hotkey.  This option overrides Open with Inventory.",
                    null, new ConfigurationManagerAttributes { Order = 3 }), ref OpenWithHoverInteract);

            UnsyncedConfig("Local Config", "Open Alongside Container", false,
                new ConfigDescription("If enabled, opening the backpack while a chest or other container is open keeps that container open and shows the backpack beside it.",
                    null, new ConfigurationManagerAttributes { Order = 3 }), ref OpenAlongsideContainer);

            UnsyncedConfig("Local Config", "Alongside Panel Gap", 24f,
                new ConfigDescription("Horizontal gap (pixels) between the player inventory and the secondary backpack panel when Open Alongside Container is enabled.",
                    null, new ConfigurationManagerAttributes { Order = 3 }), ref AlongsidePanelGap);

            UnsyncedConfig("Local Config", "Alongside Panel Offset Y", 0f,
                new ConfigDescription("Vertical offset (pixels) for the secondary backpack panel. Positive moves it up.",
                    null, new ConfigurationManagerAttributes { Order = 3 }), ref AlongsidePanelOffsetY);

            UnsyncedConfig("Local Config", "Alongside Panel Scale", 1f,
                new ConfigDescription("Scale of the secondary backpack panel (0.5 - 1.5).",
                    new AcceptableValueRange<float>(0.5f, 1.5f),
                    new ConfigurationManagerAttributes { Order = 3 }), ref AlongsidePanelScale);
            
            UnsyncedConfig("Local Config", "Close Inventory", true,
                new ConfigDescription("If enabled, both backpack and inventory will close with Open Backpack keybind is pressed while Inventory is open.",
                    null, new ConfigurationManagerAttributes { Order = 2 }), ref CloseInventory);
            
            UnsyncedConfig("Local Config", "Outward Mode", false,
                new ConfigDescription("You can use a hotkey to quickly drop your equipped backpack in order to run faster away from danger.",
                    null, new ConfigurationManagerAttributes { Order = 1 }), ref OutwardMode);

            UnsyncedConfig("Local Config", "Replace Shader", true,
                new ConfigDescription("Toggle To use the Material Shader Replacer (Requires Game Restart)",
                    null, new ConfigurationManagerAttributes { Order = 1 }), ref ReplaceShader);
            
            _config.Save();
        }
    }
    
    public class Waiting
    {
        public void ConfigurationComplete(bool configDone)
        {
            if (configDone)
                StatusChanged?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler StatusChanged;            
    }

}