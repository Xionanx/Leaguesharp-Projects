#region References

using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SharpDX;
using System.ComponentModel;

#endregion

namespace GX_Automation
{
    class Program
    {
        private const string localversion = "Updated for L# 08.06.2014 and L# Common 08.06.2014";
        internal static bool isInitialized;

        public static List<Spell> SpellList = new List<Spell>();

        public static TargetSelector TS;

        public static Spell Qa;
        public static Spell Wa;
        public static Spell Ea;
        public static Spell Ra;

        private static Menu menu;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            TrelliUpdater();  //With thanks to Trelli for doing the initial code on this!
            
            try
            {

                TS = new TargetSelector(1000, TargetSelector.TargetingMode.AutoPriority);

                Qa = new Spell(SpellSlot.Q, 650);
                Wa = new Spell(SpellSlot.W, 650);
                Ea = new Spell(SpellSlot.E, 650);
                Ra = new Spell(SpellSlot.R, 650);

                SpellList.Add(Qa);
                SpellList.Add(Wa);
                SpellList.Add(Ea);
                SpellList.Add(Ra);

                menu = new Menu("GX Automation", "gxautomation", true);
                menu.AddToMainMenu();

                menu.AddSubMenu(new Menu("Target Selector", "TargetSelector"));
                menu.SubMenu("TargetSelector").AddItem(new MenuItem("TargetingMode", "Mode")).SetValue(new StringList(new[] { "AutoPriority", "Closest", "LessAttack", "LessCast", "LowHP", "MostAD", "MostAP", "NearMouse" }, 1));
                menu.SubMenu("TargetSelector").AddItem(new MenuItem("TargetingRange", "Range")).SetValue(new Slider(750, 2000, 100));


                menu.AddSubMenu(new Menu("Potions", "potions"));
                menu.SubMenu("potions").AddItem(new MenuItem("HealthPotion", "Health Potion").SetValue<bool>(true));
                menu.SubMenu("potions").AddItem(new MenuItem("HealthPotionTrigger", "HP Percent").SetValue(new Slider(50, 100, 0)));
                menu.SubMenu("potions").AddItem(new MenuItem("ManaPotion", "Mana Potion").SetValue<bool>(true));
                menu.SubMenu("potions").AddItem(new MenuItem("ManaPotionTrigger", "MP Percent").SetValue(new Slider(50, 100, 0)));
                menu.SubMenu("potions").AddItem(new MenuItem("TotalBiscuit", "Health Biscuit").SetValue<bool>(true));
                menu.SubMenu("potions").AddItem(new MenuItem("BiscuitTrigger", "HP Percent").SetValue(new Slider(50, 100, 0)));
                menu.SubMenu("potions").AddItem(new MenuItem("CrystallineFlask", "Crystalline Flask").SetValue<bool>(true));
                menu.SubMenu("potions").AddItem(new MenuItem("FlaskHPTrigger", "HP Percent").SetValue(new Slider(50, 100, 0)));
                menu.SubMenu("potions").AddItem(new MenuItem("FlaskMPTrigger", "MP Percent").SetValue(new Slider(50, 100, 0)));
                //menu.SubMenu("potions").AddItem(new MenuItem("ElixerOfBrilliance", "**Elixer of Brilliance").SetValue<bool>(false));
                //menu.SubMenu("potions").AddItem(new MenuItem("ElixerOfFortitude", "**Elixer of Fortitude").SetValue<bool>(false));
                //menu.SubMenu("potions").AddItem(new MenuItem("OraclesExtract", "**Oracle's Extract").SetValue<bool>(false));
                //menu.SubMenu("potions").AddItem(new MenuItem("GXNULL", "** = NOT IMPLEMENTED WIP"));

                menu.AddSubMenu(new Menu("Items", "items"));
                //menu.SubMenu("items").AddItem(new MenuItem("BannerOfCommand", "**Banner of Command").SetValue<bool>(true));
                //menu.SubMenu("items").AddItem(new MenuItem("BilgewaterCutlass", "**Bilgewater Cutlass").SetValue<bool>(true));
                menu.SubMenu("items").AddItem(new MenuItem("GXNULL", "** = NOT IMPLEMENTED WIP"));

                menu.AddSubMenu(new Menu("Summoner Spells", "SummonerSpells"));
                menu.SubMenu("SummonerSpells").AddItem(new MenuItem("DoIgnite", "Ignite").SetValue<bool>(true));
                menu.SubMenu("SummonerSpells").AddItem(new MenuItem("DoSmite", "Smite").SetValue<bool>(true));
                menu.SubMenu("SummonerSpells").AddItem(new MenuItem("DoExhaust", "Exhaust").SetValue<bool>(true));
                //menu.SubMenu("SummonerSpells").AddItem(new MenuItem("Barrier", "**Barrier").SetValue<bool>(true));
                //menu.SubMenu("SummonerSpells").AddItem(new MenuItem("GXNULL", "** = NOT IMPLEMENTED WIP"));

                menu.AddSubMenu(new Menu("Weakest Link", "WeakestLink"));
                menu.SubMenu("WeakestLink").AddItem(new MenuItem("DrawLink", "Draw Link to Weakest Enemy").SetValue<bool>(true));
                menu.SubMenu("WeakestLink").AddItem(new MenuItem("GXNULL", "Weakest link finds the enemy you can do the most damage to"));
                //menu.SubMenu("WeakestLink").AddItem(new MenuItem("DrawLinkColor", "Q range").SetValue(new Drawing.           //     Color.FromArgb(255, 255, 255, 255));

                menu.AddSubMenu(new Menu("Drawing", "Drawing"));
                menu.SubMenu("Drawing").AddItem(new MenuItem("FancyTS", "Target Indicator").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
                //menu.SubMenu("Drawing").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
                //menu.SubMenu("Drawing").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
                //menu.SubMenu("Drawing").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

                Game.OnGameUpdate += Game_OnGameUpdate;
                Drawing.OnDraw += Drawing_OnDraw;

            }
            catch (Exception error)
            {

                Console.WriteLine(error.Message);
                Console.WriteLine("Section 1 Failed");
            }

        }

        #region Auto Updater

        private static void TrelliUpdater()
        {
            isInitialized = true;
            Game.PrintChat(string.Format("{0} version {1} loaded.", Assembly.GetExecutingAssembly().GetName().Name, localversion));
            var bgw = new BackgroundWorker();
            bgw.DoWork += bgw_DoWork;
            bgw.RunWorkerAsync();
        }

        static void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            var myUpdater = new Updater("https://raw.githubusercontent.com/Xionanx/GX-Automation/master/Release/version.version",
                    "https://github.com/Xionanx/Releases/raw/master/GX%20Automation.exe", localversion);

            if (myUpdater.NeedUpdate)
            {
                Game.PrintChat("<font color='#33FFFF'> .: {0}: Updating ...", Assembly.GetExecutingAssembly().GetName().Name);
                if (myUpdater.Update())
                {
                    Game.PrintChat("<font color='#33FFFF'> .: {0}: Update complete, reload please.", Assembly.GetExecutingAssembly().GetName().Name);
                }
            }
            else
            {
                Game.PrintChat("<font color='#33FFFF'> .: {0}: Most recent version ({1}) loaded!", Assembly.GetExecutingAssembly().GetName().Name, localversion);
            }
        }

        internal class Updater
        {
            private readonly string _updatelink;

            private readonly System.Net.WebClient _wc = new System.Net.WebClient { Proxy = null };
            public bool NeedUpdate = false;

            public Updater(string versionlink, string updatelink, string localversion)
            {
                _updatelink = updatelink;

                NeedUpdate = (_wc.DownloadString(versionlink)) != localversion;
            }

            public bool Update()
            {
                try
                {
                    if (
                        System.IO.File.Exists(
                            System.IO.Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location) + ".bak"))
                    {
                        System.IO.File.Delete(
                            System.IO.Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location) + ".bak");
                    }
                    System.IO.File.Move(System.Reflection.Assembly.GetExecutingAssembly().Location,
                        System.IO.Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location) + ".bak");
                    _wc.DownloadFile(_updatelink,
                        System.IO.Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location));
                    return true;
                }
                catch (Exception ex)
                {
                    Game.PrintChat("<font color='#33FFFF'> .: {0} Updater: " + ex.Message, Assembly.GetExecutingAssembly().GetName().Name);
                    return false;
                }
            }
        }

        #endregion

        private static void Game_OnGameUpdate(EventArgs args)
        {


            InternalSetTargetingMode();

            UsePotions();

            //UseItems();

            if ((menu.Item("DoIgnite").GetValue<bool>()) && (GetSummoner("SummonerDot").Cooldown <= 0))
            {
                UseSummonersIgnite();
            }

            if ((menu.Item("DoSmite").GetValue<bool>()) && (GetSummoner("SummonerSmite").Cooldown <= 0))
            {
                UseSummonersSmite();
            }
            if (menu.Item("DoExhaust").GetValue<bool>() && (GetSummoner("SummonerExhaust").Cooldown <= 0))
            {
                UseSummonersExhaust();
            }

        }

        private static void InternalSetTargetingMode()
        {
            //Console.WriteLine("Game TS Update");
            float TSRange = menu.Item("TargetingRange").GetValue<Slider>().Value;
            //Console.WriteLine("Menu Var Got Update");
            //Console.WriteLine("Converted to Float Update");
            TS.SetRange(TSRange);
            //Console.WriteLine("Targeting Mode: {0}", TS.GetTargetingMode());
            //Console.WriteLine("Targeting Range: {0}", TS.GetRange());
            //{ "AutoPriority", "Closest", "LessAttack", "LessCast", "LowHP", "MostAD", "MostAP", "NearMouse" }, 1));
            var mode = menu.Item("TargetingMode").GetValue<StringList>().SelectedIndex;
            //Console.WriteLine("Menu Setting: {0}", mode);

            switch (mode)
            {
                case 0:
                    TS.SetTargetingMode(TargetSelector.TargetingMode.AutoPriority);

                    break;
                case 1:
                    TS.SetTargetingMode(TargetSelector.TargetingMode.Closest);
                    break;
                case 2:
                    TS.SetTargetingMode(TargetSelector.TargetingMode.LessAttack);
                    break;
                case 3:
                    TS.SetTargetingMode(TargetSelector.TargetingMode.LessCast);
                    break;
                case 4:
                    TS.SetTargetingMode(TargetSelector.TargetingMode.LowHP);
                    break;
                case 5:
                    TS.SetTargetingMode(TargetSelector.TargetingMode.MostAD);
                    break;
                case 6:
                    TS.SetTargetingMode(TargetSelector.TargetingMode.MostAP);
                    break;
                case 7:
                    TS.SetTargetingMode(TargetSelector.TargetingMode.NearMouse);
                    break;
            }

            /*            if (mode == 0)
                        {
                            TS.SetTargetingMode(TargetSelector.TargetingMode.AutoPriority);
                        }
                        if (mode == 1)
                        {
                            TS.SetTargetingMode(TargetSelector.TargetingMode.Closest);
                        }
                        if (mode == 2)
                        {
                            TS.SetTargetingMode(TargetSelector.TargetingMode.LessAttack);
                        }
                        if (mode == 3)
                        {
                            TS.SetTargetingMode(TargetSelector.TargetingMode.LessCast);
                        }
                        if (mode == 4)
                        {
                            TS.SetTargetingMode(TargetSelector.TargetingMode.LowHP);
                        }
                        if (mode == 5)
                        {
                            TS.SetTargetingMode(TargetSelector.TargetingMode.MostAD);
                        }
                        if (mode == 6)
                        {
                            TS.SetTargetingMode(TargetSelector.TargetingMode.MostAP);
                        }
                        if (mode == 7)
                        {
                            TS.SetTargetingMode(TargetSelector.TargetingMode.NearMouse);
                        }
                        //Console.WriteLine("Targeting Mode: {0}", TS.GetTargetingMode());
                        //Console.WriteLine("Targeted Champ: {0}", TS.Target.BaseSkinName);  */

        }

        private static Obj_AI_Hero WeakestLink()
        {
            //Console.WriteLine("Weakest Link Called");
            Obj_AI_Hero WeakTarget = null;
            double WeakTDam = 0.0;
            double QWLDam = 0.0;
            double WWLDam = 0.0;
            double EWLDam = 0.0;
            double RWLDam = 0.0;
            double ToTDam = 0.0;
            foreach (Obj_AI_Hero champ in ObjectManager.Get<Obj_AI_Hero>())
            {
                //Console.WriteLine("foreach champ started.");
                if (champ != null)
                {
                    try
                    {
                        if (champ.IsEnemy)
                        {
                            //Console.WriteLine("champ is enemy");
                            QWLDam = DamageLib.getDmg(champ, DamageLib.SpellType.Q);
                            WWLDam = DamageLib.getDmg(champ, DamageLib.SpellType.W);
                            EWLDam = DamageLib.getDmg(champ, DamageLib.SpellType.E);
                            RWLDam = DamageLib.getDmg(champ, DamageLib.SpellType.R);
                            ToTDam = (QWLDam + WWLDam + EWLDam + RWLDam);
                            if (WeakTarget == null)
                            {
                                WeakTarget = champ;
                                WeakTDam = ToTDam;
                            }
                            if (WeakTDam < ToTDam)
                            {
                                WeakTarget = champ;
                                WeakTDam = ToTDam;
                            }
                        }
                    }
                    catch (Exception error)
                    {

                        Console.WriteLine("Error: {0}", error);
                    }


                    //Console.WriteLine("Weakest Link: {0}", WeakTarget.BaseSkinName);


                }
            }
            return WeakTarget;
        }

        private static void UseSummonersSmite()
        {
            //Console.WriteLine("Smite Section Callled");
            int Level = ObjectManager.Player.Level;
            //Console.WriteLine("Player Level {0}", Level);
            float[] SmiteDam = { 20 * Level + 370, 30 * Level + 330, 40 * Level + 240, 50 * Level + 100 };
            //Console.WriteLine("Smite Damage {0}", SmiteDam.Max());
            string[] SmiteNames = { "LizardElder", "AncientGolem", "Worm", "Dragon" };
            //Console.WriteLine("Smite Names Set");
            //Console.WriteLine("Smite Section Callled");
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, GetSummoner("SummonerSmite").SData.CastRange[0], MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
            //Console.WriteLine("Smite minions indexed");
            foreach (var minion in minions)
            {
                if (minion != null)
                {
                    //Console.WriteLine("Smite Minion {0}", minion.BaseSkinName);
                    if ((minion.Health < SmiteDam.Max()) && (SmiteNames.Any(name => minion.BaseSkinName.StartsWith(name))))
                    {
                        //Console.WriteLine("Valid Target Found: {0}", minion.BaseSkinName);
                        ObjectManager.Player.SummonerSpellbook.CastSpell(GetSummoner("SummonerSmite").Slot, minion);
                    }
                }
            }

        }

        private static void UseSummonersIgnite()
        {


            //Console.WriteLine("Ignite Section Called");
            //var IgniteTarget = TS.Target;

            //if (IgniteTarget != null)
            //Console.WriteLine("Target Selector Target: {0}", IgniteTarget.BaseSkinName);
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero != null)
                {
                    //Console.WriteLine("Checking {0}", hero.BaseSkinName);
                    if ((hero.IsEnemy) && (hero.IsValidTarget(GetSummoner("SummonerDot").SData.CastRange[0])))
                    {
                        //Console.WriteLine("Enemy Found {0} with {1} HP and Ignite can do {2} damage!", hero.BaseSkinName, hero.Health, DamageLib.getDmg(hero, DamageLib.SpellType.IGNITE));
                        //Console.WriteLine("Distance to Target: {0}", ObjectManager.Player.ServerPosition.Distance(hero.Position));

                        if (hero.Health < DamageLib.getDmg(hero, DamageLib.SpellType.IGNITE))
                        {
                            ObjectManager.Player.SummonerSpellbook.CastSpell(GetSummoner("SummonerDot").Slot, hero);
                        }
                    }
                }
            }
        }

        private static SpellDataInst GetSummoner(string p)
        {
            var spells = ObjectManager.Player.SummonerSpellbook.Spells;
            return spells.FirstOrDefault(spell => spell.Name == p);
        }

        private static void UseSummonersExhaust()
        {
            //Console.WriteLine("Exhaust Section Called");
            var ExhaustTarget = SimpleTs.GetTarget(GetSummoner("SummonerExhaust").SData.CastRange[0], SimpleTs.DamageType.True);
            if (ExhaustTarget != null)
            {
                //Console.WriteLine("Exhaust Target {0}", ExhaustTarget.BaseSkinName);
                var QEDam = DamageLib.getDmg(ExhaustTarget, DamageLib.SpellType.Q);
                var WEDam = DamageLib.getDmg(ExhaustTarget, DamageLib.SpellType.W);
                var EEDam = DamageLib.getDmg(ExhaustTarget, DamageLib.SpellType.E);
                var REDam = DamageLib.getDmg(ExhaustTarget, DamageLib.SpellType.R);
                double TotDam = 0;
                if (Qa.IsReady()) { TotDam = (TotDam + QEDam); }
                if (Wa.IsReady()) { TotDam = (TotDam + WEDam); }
                if (Ea.IsReady()) { TotDam = (TotDam + EEDam); }
                if (Ra.IsReady()) { TotDam = (TotDam + REDam); }
                //Console.WriteLine("Exhaust Total Dam Check {0}", TotDam);

                if (((ExhaustTarget.Health < (ExhaustTarget.MaxHealth / 3)) || (ExhaustTarget.Health < TotDam)) && (ExhaustTarget.IsValidTarget(GetSummoner("SummonerExhaust").SData.CastRange[0])))
                {
                    ObjectManager.Player.SummonerSpellbook.CastSpell(GetSummoner("SummonerExhaust").Slot, ExhaustTarget);
                }
            }
        }

        private static void UsePotions()
        {
            //LeagueSharp.Common.Interrupter.OnPosibleToInterrupt
            //LeagueSharp.Common.Items.UseItem(2004);
            //var Buffs = ObjectManager.Player.Buffs;
            //foreach (var buff in Buffs)
            //{
            //    Console.WriteLine("Player Has Buff {0}", buff.Name);
            //}
            try
            {
                if (menu.Item("ManaPotion").GetValue<bool>() && (!ObjectManager.Player.Buffs.Any(buff => buff.Name == "FlaskOfCrystalWater")) && (menu.Item("ManaPotionTrigger").GetValue<Slider>().Value > GetPlayerManaPercent()))
                {
                    if (GetInventorySlot(2004) != null)
                        GetInventorySlot(2004).UseItem();
                }
                if (menu.Item("CrystallineFlask").GetValue<bool>() && (!ObjectManager.Player.Buffs.Any(buff => buff.Name == "ItemCrystalFlask")) && ((menu.Item("FlaskHPTrigger").GetValue<Slider>().Value > GetPlayerHealthPercent()) || (menu.Item("FlaskMPTrigger").GetValue<Slider>().Value > GetPlayerManaPercent())))
                {
                    if (GetInventorySlot(2041) != null)
                        GetInventorySlot(2041).UseItem();
                }
                if ((menu.Item("HealthPotion").GetValue<bool>()) && (!ObjectManager.Player.Buffs.Any(buff => buff.Name == "RegenerationPotion")) && (menu.Item("HealthPotionTrigger").GetValue<Slider>().Value > GetPlayerHealthPercent()))
                {
                    if (GetInventorySlot(2003) != null)
                        GetInventorySlot(2003).UseItem();
                }
                if (menu.Item("TotalBiscuit").GetValue<bool>() && (!ObjectManager.Player.Buffs.Any(buff => buff.Name == "ItemMiniRegenPotion")) && (menu.Item("BiscuitTrigger").GetValue<Slider>().Value > GetPlayerHealthPercent()))
                {
                    if (GetInventorySlot(2009) != null)
                        GetInventorySlot(2009 | 2010).UseItem();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Error: {0}", error);
            }
        }

        /*private static bool BuffCheck(string p)
        {
            var Buffs = ObjectManager.Player.Buffs;
            foreach (var buff in Buffs)
            {
                if (buff.Name == p)
                    return true;
            }
            return false;
        }*/

        private static InventorySlot GetInventorySlot(int ID)
        {
            return ObjectManager.Player.InventoryItems.FirstOrDefault(item => (item.Id == (ItemId)ID && item.Stacks >= 1) || (item.Id == (ItemId)ID && item.Charges >= 1));
        }

        private static float GetPlayerManaPercent()
        {
            return ObjectManager.Player.Mana * 100 / ObjectManager.Player.MaxMana;
        }

        private static float GetPlayerHealthPercent()
        {
            return ObjectManager.Player.Health * 100 / ObjectManager.Player.MaxHealth;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            float[] PlayerPos = Drawing.WorldToScreen(ObjectManager.Player.Position);
            float[] WeakLinkPos = Drawing.WorldToScreen(WeakestLink().Position);
            if (menu.Item("DrawLink").GetValue<bool>())
            {
                //Console.WriteLine("Drawing Called");
                Drawing.DrawLine(PlayerPos[0], PlayerPos[1], WeakLinkPos[0], WeakLinkPos[1], 3, System.Drawing.Color.Red);

                //Drawing.DrawLine(ObjectManager.Player.Position.X, ObjectManager.Player.Position.Y, WeakestLink().Position.X, WeakestLink().Position.Y, 2, System.Drawing.Color.AliceBlue);
            }

            //Console.WriteLine("Fancy TS Check");
            var menuItem = menu.Item("FancyTS").GetValue<Circle>();
            if (menuItem.Active)
            {
                //Console.WriteLine("Fancy TS is ON");

                for (int spin = 1; spin <= 10; spin++)
                {
                    var spincircleA = (spin * (100 / 10 * 2) - 100);

                    float TSradius = (float)Math.Sqrt(Math.Pow(100, 2) - Math.Pow(spincircleA, 2));

                    float spincircleB = (spincircleA / 1.3f);

                    Vector3 TSGlobe = new Vector3(TS.Target.Position.X, TS.Target.Position.Y, TS.Target.Position.Z);

                    Drawing.DrawCircle(TSGlobe, TSradius, menuItem.Color);

                }
            }
        }


    }
}
