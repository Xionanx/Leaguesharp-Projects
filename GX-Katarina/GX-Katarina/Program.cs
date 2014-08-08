#region References

using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;


#endregion

namespace GX_Katarina
{
    class Program
    {
        private const string localversion = "Updated for L# 08.06.2014 and L# Common 08.06.2014";
        internal static bool isInitialized;
        
        public const string ChampionName = "Katarina";

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        //public static Spell DFG; //Deathfire Grasp
        //public static Spell HXG; //Hextech Gunblade
        //public static Spell BWC; //Bilgewater Cutlass
        //public static Spell Sheen; //Sheen
        //public static Spell Trinity; //Trinity Force
        //public static Spell LB; //Lich Bane
        //public static Spell IG; //Iceborn Gauntlet
        //public static Spell LT; //Liandry's Torment
        //public static Spell BT; //Blackfire Torch
        //public static Spell FQC; //Frost Queens Claim
        //public static Spell RO; //Randuin's Omen
        //public static Spell BotRK; //Blade of the Ruined King

        private static Menu menu;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            //  Check if its the correct champion
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            TrelliUpdater();  //With thanks to Trelli for doing the initial code on this!

            Game.PrintChat(string.Format("{0} v{1} loaded.", Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version));

            Q = new Spell(SpellSlot.Q, 675);
            W = new Spell(SpellSlot.W, 375);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 550);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            menu = new Menu(ChampionName, ChampionName, true);
            menu.AddToMainMenu();

            menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(menu.SubMenu("Orbwalking"));

            var TargetSelectorMenu = new Menu("Target Selector", "TargetSelector");
            SimpleTs.AddToMenu(TargetSelectorMenu);
            menu.AddSubMenu(TargetSelectorMenu);

            menu.AddSubMenu(new Menu("Combo", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Q in Combo").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "W in Combo").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "E in Combo").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "R in Combo").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("StopRCombo", "Stop R if Killable").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("ProcQCombo", "Q Proc if Possible").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            menu.AddSubMenu(new Menu("Harass", "Harass"));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Q to Harass").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "W to Harass").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "E to Harass").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("ProcQHarass", "Q Proc if Possible").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            menu.AddSubMenu(new Menu("Last Hit Options", "LastHit"));
            menu.SubMenu("LastHit").AddItem(new MenuItem("LastHitQ", "Q to Last Hit").SetValue(true));
            menu.SubMenu("LastHit").AddItem(new MenuItem("LastHitW", "W to Last Hit").SetValue(true));
            menu.SubMenu("LastHit").AddItem(new MenuItem("LastHitE", "E to Last Hit").SetValue(true));
            menu.SubMenu("LastHit").AddItem(new MenuItem("ProcQFarm", "Q Proc Farming?").SetValue(true));
            menu.SubMenu("LastHit").AddItem(new MenuItem("LastHitActive", "Last Hit!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            menu.AddSubMenu(new Menu("Lane Clearing", "LaneClear"));
            menu.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearQ", "Q to Lane Clear").SetValue(true));
            menu.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearW", "W to Lane Clear").SetValue(true));
            menu.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearE", "E to Lane Clear").SetValue(true));
            menu.SubMenu("LaneClear").AddItem(new MenuItem("ProcQClear", "Q Proc Clearing?").SetValue(true));
            menu.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            menu.AddSubMenu(new Menu("Item Use", "ItemUse"));
            menu.SubMenu("ItemUse").AddItem(new MenuItem("DFG", "Deathfire Grasp").SetValue(true));
            menu.SubMenu("ItemUse").AddItem(new MenuItem("HXG", "Hextech Gunblade").SetValue(true));
            menu.SubMenu("ItemUse").AddItem(new MenuItem("BWC", "Bilgewater Cutlass").SetValue(true));
            menu.SubMenu("ItemUse").AddItem(new MenuItem("BotRK", "Blade of the Ruined King").SetValue(true));
            menu.SubMenu("ItemUse").AddItem(new MenuItem("FQQ", "Frost Queen's Claim").SetValue(true));
            menu.SubMenu("ItemUse").AddItem(new MenuItem("RO", "Randuin's Omen").SetValue(true));
            //menu.SubMenu("ItemUse").AddItem(new MenuItem("WardJump", "Ward Jump!!").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));

            menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            menu.SubMenu("Drawing").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            menu.SubMenu("Drawing").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            menu.SubMenu("Drawing").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            menu.SubMenu("Drawing").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            //BuffCheck(ObjectManager.Player, "blah");
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            {
                //Orbwalker.SetAttacks(true);
                if ((menu.Item("ComboActive").GetValue<KeyBind>().Active) && (target != null))
                {
                    Combo(target);
                }

                if (menu.Item("LastHitActive").GetValue<KeyBind>().Active)
                {
                    LastHit();
                }

                if ((menu.Item("HarassActive").GetValue<KeyBind>().Active) && (target != null))
                {
                    Harass(target);
                }

                if (menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
                {
                    LaneClear();
                }
                //                if ((menu.Item("WardJump").GetValue<KeyBind>().Active) && (E.IsReady()))
                //                {
                //                    WardJump();
                //                }
            }
        }

        /*        private static void WardJump()
                {
                    try
                    {

                        Vector3 wardpos = Game.CursorPos;
                        {
                            GetInventorySlot(3340).UseItem(wardpos);   // 2044 | 2043 | 3340 | 3361 | 3362 | 2045 | 2049).UseItem(jumppos);
                                {
                                    E.Cast(wardpos);
                                }
                        }
                    }
                    catch (Exception error)
                    {

                        Console.WriteLine("Error: {0}", error);
                    }

                }

         */

        private static void Harass(Obj_AI_Hero target)
        {
            if (target.IsValid)
            {
                if (menu.Item("UseQHarass").GetValue<bool>() && (target.IsValidTarget(Q.Range)))
                {
                    Q.CastOnUnit(target);
                }
                if (menu.Item("ProcQHarass").GetValue<bool>())
                {
                    if (BuffCheck(target, "katarinaqmark") && (menu.Item("UseEHarass").GetValue<bool>()) && (target.IsValidTarget(E.Range)))
                    {
                        E.CastOnUnit(target);
                    }
                    if (BuffCheck(target, "katarinaqmark") && (menu.Item("UseWHarass").GetValue<bool>()) && (target.IsValidTarget(W.Range)))
                    {
                        W.CastOnUnit(target);
                    }
                }
                if (!menu.Item("ProcQHarass").GetValue<bool>())
                {
                    if (menu.Item("UseEHarass").GetValue<bool>() && (target.IsValidTarget(E.Range)))
                    {
                        E.CastOnUnit(target);
                    }
                    if (menu.Item("UseWHarass").GetValue<bool>() && (target.IsValidTarget(W.Range)))
                    {
                        W.CastOnUnit(target);
                    }
                }
            }
        }

        private static void LaneClear()
        {
            var Minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
            foreach (var minion in Minions)
            {
                if (minion != null)
                {
                    var minionQDam = DamageLib.getDmg(minion, DamageLib.SpellType.Q, DamageLib.StageType.FirstDamage);
                    var minionEDam = DamageLib.getDmg(minion, DamageLib.SpellType.E, DamageLib.StageType.FirstDamage);
                    var minionWDam = DamageLib.getDmg(minion, DamageLib.SpellType.W, DamageLib.StageType.FirstDamage);
                    var minionQEWDam = (minionQDam + minionEDam + minionWDam);
                    var minionQEWProcDam = ((DamageLib.getDmg(minion, DamageLib.SpellType.Q, DamageLib.StageType.Default)) + minionEDam + minionWDam);

                    if (minion.IsValidTarget(Q.Range) && HealthPrediction.GetHealthPrediction(minion, (int)Q.Delay) < minionQEWProcDam && Q.IsReady() && E.IsReady() && W.IsReady())
                    {
                        if (menu.Item("ProcQClear").GetValue<bool>() && menu.Item("LaneClearQ").GetValue<bool>())
                        {
                            Q.CastOnUnit(minion);

                            if ((!Q.IsReady()) && (menu.Item("LaneClearE").GetValue<bool>()) && (minion.IsValidTarget(E.Range)))
                            {
                                E.CastOnUnit(minion);
                            }
                            if ((!E.IsReady()) && (menu.Item("LaneClearW").GetValue<bool>()) && (minion.IsValidTarget(W.Range)))
                            {
                                W.CastOnUnit(minion);
                            }
                        }
                    }

                    if (minion.IsValidTarget(Q.Range) && HealthPrediction.GetHealthPrediction(minion, (int)Q.Delay) < minionQEWDam && Q.IsReady() && E.IsReady() && W.IsReady())
                    {
                        if (menu.Item("LaneClearQ").GetValue<bool>() && menu.Item("LaneClearW").GetValue<bool>() && menu.Item("LaneClearE").GetValue<bool>())
                        {
                            Q.CastOnUnit(minion);
                            E.CastOnUnit(minion);
                            W.CastOnUnit(minion);
                        }
                    }

                    if (minion.IsValidTarget(Q.Range) && (HealthPrediction.GetHealthPrediction(minion, (int)Q.Delay) < minionQDam) && (Q.IsReady()))
                    {
                        if (menu.Item("LaneClearQ").GetValue<bool>())
                        {
                            Q.CastOnUnit(minion);
                        }

                    }
                    if (minion.IsValidTarget(E.Range) && (HealthPrediction.GetHealthPrediction(minion, (int)Q.Delay) < minionEDam) && (E.IsReady()))
                    {
                        if (menu.Item("LaneClearE").GetValue<bool>())
                        {
                            E.CastOnUnit(minion);
                        }

                    }
                    if (minion.IsValidTarget(W.Range) && (HealthPrediction.GetHealthPrediction(minion, (int)Q.Delay) < minionWDam) && (W.IsReady()))
                    {
                        if (menu.Item("LaneClearW").GetValue<bool>())
                        {
                            W.CastOnUnit(minion);
                        }

                    }
                    if (minion.IsValidTarget(Q.Range) && (Q.IsReady()))
                    {
                        Q.CastOnUnit(minion);
                    }
                    if (minion.IsValidTarget(E.Range) && (E.IsReady()))
                    {
                        E.CastOnUnit(minion);
                    }
                    if (minion.IsValidTarget(W.Range) && (W.IsReady()))
                    {
                        W.CastOnUnit(minion);
                    }
                }
            }
        }

        private static void LastHit()
        {
            var Minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
            foreach (var minion in Minions)
            {
                if (minion != null)
                {
                    var minionQDam = DamageLib.getDmg(minion, DamageLib.SpellType.Q, DamageLib.StageType.FirstDamage);
                    var minionEDam = DamageLib.getDmg(minion, DamageLib.SpellType.E, DamageLib.StageType.FirstDamage);
                    var minionWDam = DamageLib.getDmg(minion, DamageLib.SpellType.W, DamageLib.StageType.FirstDamage);
                    var minionQEWDam = (minionQDam + minionEDam + minionWDam);
                    var minionQEWProcDam = ((DamageLib.getDmg(minion, DamageLib.SpellType.Q, DamageLib.StageType.Default)) + minionEDam + minionWDam);

                    if ((menu.Item("ProcQFarm").GetValue<bool>()) && minion.IsValidTarget(Q.Range) && HealthPrediction.GetHealthPrediction(minion, (int)Q.Delay) < minionQEWProcDam && Q.IsReady() && E.IsReady() && W.IsReady())
                    {
                        if (menu.Item("LastHitQ").GetValue<bool>())
                        {
                            Q.CastOnUnit(minion);

                            if (MinionBuffCheck(minion, "katarinaqmark") && (menu.Item("LastHitE").GetValue<bool>()) && (minion.IsValidTarget(E.Range)))
                            {
                                E.CastOnUnit(minion);
                            }
                            if (MinionBuffCheck(minion, "katarinaqmark") && (menu.Item("LastHitW").GetValue<bool>()) && (minion.IsValidTarget(W.Range)))
                            {
                                W.CastOnUnit(minion);
                            }
                        }
                    }

                    if (minion.IsValidTarget(Q.Range) && HealthPrediction.GetHealthPrediction(minion, (int)Q.Delay) < minionQEWDam && Q.IsReady() && E.IsReady() && W.IsReady())
                    {
                        if (menu.Item("LastHitQ").GetValue<bool>() && menu.Item("LastHitE").GetValue<bool>() && menu.Item("LastHitW").GetValue<bool>())
                        {
                            Q.CastOnUnit(minion);
                            E.CastOnUnit(minion);
                            W.CastOnUnit(minion);
                        }
                    }

                    if (minion.IsValidTarget(Q.Range) && (HealthPrediction.GetHealthPrediction(minion, (int)Q.Delay) < minionQDam) && (Q.IsReady()) && (menu.Item("LastHitQ").GetValue<bool>()))
                    {
                        Q.CastOnUnit(minion);
                    }
                    if (minion.IsValidTarget(E.Range) && (HealthPrediction.GetHealthPrediction(minion, (int)E.Delay) < minionEDam) && (E.IsReady()) && (menu.Item("LastHitE").GetValue<bool>()))
                    {
                        E.CastOnUnit(minion);
                    }
                    if (minion.IsValidTarget(W.Range) && (HealthPrediction.GetHealthPrediction(minion, (int)W.Delay) < minionWDam) && (W.IsReady()) && (menu.Item("LastHitW").GetValue<bool>()))
                    {
                        W.CastOnUnit(minion);
                    }
                }
            }
        }

        private static bool MinionBuffCheck(Obj_AI_Base minion, string p)
        {
            var Buffs = minion.Buffs;
            foreach (var buff in Buffs)
            {
                Console.WriteLine("Minion has Buff {0}", buff.Name);
                if (buff.Name == p)
                    return true;
            }
            return false;
        }

        private static void Combo(Obj_AI_Hero target)
        {
            BuffCheck(ObjectManager.Player, "blah");
            if (target.IsValid)
            {
                var targetQDam = DamageLib.getDmg(target, DamageLib.SpellType.Q, DamageLib.StageType.FirstDamage);
                var targetEDam = DamageLib.getDmg(target, DamageLib.SpellType.E, DamageLib.StageType.FirstDamage);
                var targetWDam = DamageLib.getDmg(target, DamageLib.SpellType.W, DamageLib.StageType.FirstDamage);
                var targetQEWDam = (targetQDam + targetEDam + targetWDam);
                var targetQEWProcDam = ((DamageLib.getDmg(target, DamageLib.SpellType.Q, DamageLib.StageType.Default)) + targetEDam + targetWDam);

                if (menu.Item("StopRCombo").GetValue<bool>() && (Q.IsReady() || E.IsReady() || W.IsReady()))
                {
                    Orbwalker.SetMovement(true);
                    if ((target.Health < targetQDam) && (Q.IsReady()) && target.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(target);
                    }
                    if ((target.Health < targetEDam) && (E.IsReady()) && target.IsValidTarget(E.Range))
                    {
                        E.CastOnUnit(target);
                    }
                    if ((target.Health < targetWDam) && (W.IsReady()) && target.IsValidTarget(W.Range))
                    {
                        W.CastOnUnit(target);
                    }
                    if ((target.Health < targetQEWDam) && (Q.IsReady()) && target.IsValidTarget(Q.Range) && (W.IsReady()) && (E.IsReady()))
                    {
                        Q.CastOnUnit(target);
                        E.CastOnUnit(target);
                        W.CastOnUnit(target);
                    }
                    if ((target.Health < targetQEWProcDam) && (Q.IsReady()) && target.IsValidTarget(Q.Range) && (W.IsReady()) && (E.IsReady()) && (menu.Item("ProcQCombo").GetValue<bool>()))
                    {
                        Q.CastOnUnit(target);

                        if (BuffCheck(target, "katarinaqmark") && (menu.Item("UseECombo").GetValue<bool>()) && (target.IsValidTarget(E.Range)))
                        {
                            E.CastOnUnit(target);
                            W.CastOnUnit(target);
                        }
                        if (BuffCheck(target, "katarinaqmark") && (menu.Item("UseWCombo").GetValue<bool>()) && (target.IsValidTarget(W.Range)))
                        {
                            W.CastOnUnit(target);

                        }

                    }

                }

                if (menu.Item("DFG").GetValue<bool>() && (GetInventorySlot(3128) != null) && (ObjectManager.Player.ServerPosition.Distance(target.Position) < Q.Range))
                {
                    GetInventorySlot(3128).UseItem(target);
                }
                if (menu.Item("HXG").GetValue<bool>() && (GetInventorySlot(3146) != null) && (ObjectManager.Player.ServerPosition.Distance(target.Position) < Q.Range))
                {
                    GetInventorySlot(3146).UseItem(target);
                }
                if (menu.Item("BWC").GetValue<bool>() && (GetInventorySlot(3144) != null) && (ObjectManager.Player.ServerPosition.Distance(target.Position) < Q.Range))
                {
                    GetInventorySlot(3144).UseItem(target);
                }
                if (menu.Item("BotRK").GetValue<bool>() && (GetInventorySlot(3153) != null) && (ObjectManager.Player.ServerPosition.Distance(target.Position) < Q.Range))
                {
                    GetInventorySlot(3153).UseItem(target);
                }
                if (menu.Item("FQQ").GetValue<bool>() && (GetInventorySlot(3092) != null) && (ObjectManager.Player.ServerPosition.Distance(target.Position) < 380))
                {
                    GetInventorySlot(3092).UseItem(target);
                }
                if (menu.Item("RO").GetValue<bool>() && (GetInventorySlot(3143) != null) && (ObjectManager.Player.ServerPosition.Distance(target.Position) < 500))
                {
                    GetInventorySlot(3143).UseItem();
                }
                if (menu.Item("ProcQCombo").GetValue<bool>() && (Q.IsReady()) && target.IsValidTarget(Q.Range) && (W.IsReady()) && (E.IsReady()))
                {
                    if (menu.Item("UseQCombo").GetValue<bool>() && (target.IsValidTarget(Q.Range)))
                    {
                        Q.CastOnUnit(target);
                        // katarinaqmark
                    }
                    if (BuffCheck(target, "katarinaqmark") && (menu.Item("UseECombo").GetValue<bool>()) && (target.IsValidTarget(E.Range)))
                    {
                        E.CastOnUnit(target);
                        W.CastOnUnit(target);
                    }
                    if (BuffCheck(target, "katarinaqmark") && (menu.Item("UseWCombo").GetValue<bool>()) && (target.IsValidTarget(W.Range)))
                    {
                        W.CastOnUnit(target);
                    }
                }

                if (menu.Item("UseQCombo").GetValue<bool>() && (target.IsValidTarget(Q.Range)))
                {
                    Q.CastOnUnit(target);
                    // katarinaqmark
                }
                if (menu.Item("UseECombo").GetValue<bool>() && (target.IsValidTarget(E.Range)))
                {
                    E.CastOnUnit(target);
                }
                if (menu.Item("UseWCombo").GetValue<bool>() && (target.IsValidTarget(W.Range)))
                {
                    W.CastOnUnit(target);
                }
                if (menu.Item("UseRCombo").GetValue<bool>() && (target.IsValidTarget(R.Range)))
                {
                    if (!Q.IsReady() && !E.IsReady())
                        R.CastOnUnit(target);
                }
            }
        }

        private static bool BuffCheck(Obj_AI_Hero target, string p)
        {
            var Buffs = target.Buffs;
            foreach (var buff in Buffs)
            {
                Console.WriteLine("Target has Buff {0}", buff.Name);
                if (buff.Name == p)
                    return true;
            }
            return false;
        }

        private static InventorySlot GetInventorySlot(int ID)
        {
            Console.WriteLine("Looking for item ID: {0}", ID);
            return ObjectManager.Player.InventoryItems.FirstOrDefault(item => (item.Id == (ItemId)ID && item.Stacks >= 1) || (item.Id == (ItemId)ID && item.Charges >= 1));
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
                }
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
            var myUpdater = new Updater("https://raw.githubusercontent.com/Xionanx/GX-Katarina/master/Release/version.version",
                    "https://github.com/Xionanx/Releases/raw/master/GX%20Katarina.exe", localversion);

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
    }
}
