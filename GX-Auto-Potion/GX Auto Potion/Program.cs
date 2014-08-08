#region

using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Linq;
using System.Reflection;

#endregion


namespace GXAutoPotion
{

    internal class Program
    {
        private const string localversion = "Updated for L# 08.06.2014 and L# Common 08.06.2014";
        internal static bool isInitialized;

        private static Menu Config;
        
        private static void Main(String[] args)
        {
            CustomEvents.Game.OnGameLoad +=Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {

            Game.PrintChat(string.Format("{0} v{1} loaded.", Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version));

            Config = new Menu("GX Auto Potion", "GXAutoPotion", true);

            Config.AddItem(new MenuItem("HealthPercent", "HP Trigger Percent").SetValue(new Slider(50, 100, 0)));
            Config.AddItem(new MenuItem("ManaPercent", "MP Trigger Percent").SetValue(new Slider(50, 100, 0)));   
            Config.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        
        private static void Game_OnGameUpdate(System.EventArgs args)
        {
        
            if (GetPlayerHealthPercent() < Config.Item("HealthPercent").GetValue<Slider>().Value)
                if (!ObjectManager.Player.Buffs.Any(buff => buff.Name == "RegenerationPotion" || buff.Name == "ItemCrystalFlask" || buff.Name == "ItemMiniRegenPotion"))
                    GetHealthPotionSlot().UseItem();
            
            if (GetPlayerManaPercent() < Config.Item("ManaPercent").GetValue<Slider>().Value)
                if (!ObjectManager.Player.Buffs.Any(buff => buff.Name == "ItemCrystalFlask" || buff.Name == "FlaskOfCrystalWater"))
                    GetManaPotionSlot().UseItem();
        }

        private static float GetPlayerHealthPercent()
        {
            return ObjectManager.Player.Health * 100 / ObjectManager.Player.MaxHealth;
        }
        
        private static InventorySlot GetHealthPotionSlot()
        {
            return ObjectManager.Player.InventoryItems.First(item => (item.Id == (ItemId)2003 && item.Stacks >= 1 )|| (item.Id == (ItemId)2009 && item.Stacks >= 1) || (item.Id == (ItemId)2010 && item.Stacks >= 1) || (item.Id == (ItemId)2041) && item.Charges >=1 );
        }

        private static float GetPlayerManaPercent()
        {
            return ObjectManager.Player.Mana * 100 / ObjectManager.Player.MaxMana;
        }

        private static InventorySlot GetManaPotionSlot()
        {
            return ObjectManager.Player.InventoryItems.First(item => (item.Id == (ItemId)2004 && item.Stacks >= 1) || (item.Id == (ItemId)2041 && item.Charges >=1 ));
        }
    }
}
