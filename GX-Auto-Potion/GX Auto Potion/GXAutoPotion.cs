//  Be sure to acknowledge anyone who's code you used or helped make the program you are writing possible through contributions of their
//  time or knowledge, by adding some thanks to the comments.
//
//  GX Auto Potion by GXion
//  Made with base code from from Appril on IRC Rizon #leaguesharp and the help of and advice of Trelli, FlapperDoodle, and Adam as 
//  well as other members of the #leaguesharp channel.
//
//  List all the references you will be using
using LeagueSharp;
using System;
using System.Linq;
using System.Reflection;

//  Assign the same namespace to your program
namespace GXAutoPotion
{
    //  Set the class that was called from the Program Class
    class GXAutoPotion
    {
        //  Initialize any variable that will be used over the entire class
        //  Since we know we are making an Auto Potion drinking program, we know we need to know at what point to trigger drinking a potion.
        //  So we set two triggers, one for drinking a Health Potion, and one for drinking a mana potion.
        private int PlayerHealthPercentTrigger = 50;
        private int PlayerManaPercentTrigger = 50;

        //  Write the first method that is called in your method.  You will get errors if the first method isn't named the same as the class it is in.  (needs a return type)
        public GXAutoPotion()
        {
            //  Set game event triggers that will trigger methods within your class.
            Game.OnGameStart += Game_OnGameStart;  //  Trigger the Game_OnGameStart method in your class, when the game finishes loading
            Game.OnGameUpdate += Game_OnGameUpdate;  //  Trigger the Game_OnGameUpdate method in your class, then the game update on tick (1000 ticks a second)
        }

        //  Write the method that is called on game start, remember this is triggerd from the previous method
        void Game_OnGameStart(EventArgs args)
        {
 	        // What do you want to happen on game start?  Write something to the chat?  See what champion you are playing?  Set some variables?
            // Here we are using System.Reflection to pull the Program Name and Version number from the executable and post it into the game chat.
            Game.PrintChat(string.Format("{0} v{1} loaded.", Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version));

        }
        //  Write the method that is called on game updated, remember this is triggerd from the first method in the program.
        void Game_OnGameUpdate(EventArgs args)
        {
            //  What do you want to happen with ever game update?  Usually the primary method that your program runs off of will be called from here.
            //  Here, I am going to call the DoINeedToDrinkAPotion mehod.
            DoINeedToDrinkAPotion();
        }
        //  Write a method for every part of the program, here I am writing the method that checks if your champion needs to drink a potion.
        private void DoINeedToDrinkAPotion()
        {
            // First, the program checks if your champions health if less then the pre-defined percent health trigger you set earlier
            // We do this by comparing a Method that returns the players current health, to the trigger we set earlier.
            if (GetPlayerHealthPercent() < PlayerHealthPercentTrigger)

                //  Here we write what happens next if the above IF statement is true, in this case we are going to make sure the player hasn't already drank a potion
                // by checking for the buff that potions give.  Don't worry about where to get the buff names for now, you'll find resources for those later.
                if (!ObjectManager.Player.Buffs.Any(buff => buff.Name == "RegenerationPotion" || buff.Name == "ItemCrystalFlask" || buff.Name == "ItemMiniRegenPotion"))

                    // Here we write what happens next if the aboce statement is also true, now that we know your players Health is low enough, and your player isn't already 
                    // regenating from drinking a potion, its time to actually drink a potion.  We do this by calling yet another method, that will scan all your inventory slots
                    // for a health potion and return the SLOT and then using information in yet another method, the UseItem() method, that is part of the #LeagueSharp.DLL
                    GetHealthPotionSlot().UseItem();

            //  We now repeat the process above, only this time we do it for Mana!  Here I am just going to repeat the code above, and replace only the parts that need to be replaced
            //  to check for Mana.
            if (GetPlayerManaPercent() < PlayerManaPercentTrigger)
                if (!ObjectManager.Player.Buffs.Any(buff => buff.Name == "ItemCrystalFlask" || buff.Name == "FlaskOfCrystalWater"))
                    GetManaPotionSlot().UseItem();
            
        }

        //  Now its time to write the method we called previously to check for player health percent.
        private float GetPlayerHealthPercent()
        {
            //  We use the ObjectManaget to find the players current health, multiplied by 100, and then divided by max health,  This will give us a float in the range of 0 to 100.
            //  This number is then RETURNED so it can be compared in the method that called it.
            return ObjectManager.Player.Health * 100 / ObjectManager.Player.MaxHealth;
        }
        
        //  Now we write the method that finds which slot our health potions are in.  You can see that the OBJECT to be returned in this method is "InventorySlot"
        private InventorySlot GetHealthPotionSlot()
        {
            //  We find the slot by going through all inventory slots, looking for the First slot that matches the Item ID we are looking for and can be used.  Again, don't worry about where
            //  to get the ID's from, there are references for this available.
                       
            return ObjectManager.Player.InventoryItems.First(item => (item.Id == (ItemId)2003 && item.Stacks >= 1 )|| (item.Id == (ItemId)2009 && item.Stacks >= 1) || (item.Id == (ItemId)2010 && item.Stacks >= 1) || (item.Id == (ItemId)2041) && item.Charges >=1 );
        }

        //  Now, we repeast the two above methods, only this time we change them slightly to be used for Mana instead of Health.
        private float GetPlayerManaPercent()
        {
            return ObjectManager.Player.Mana * 100 / ObjectManager.Player.MaxMana;
        }

        private InventorySlot GetManaPotionSlot()
        {
            return ObjectManager.Player.InventoryItems.First(item => (item.Id == (ItemId)2004 && item.Stacks >= 1) || (item.Id == (ItemId)2041 && item.Charges >=1 ));
        }
       
    }
}
