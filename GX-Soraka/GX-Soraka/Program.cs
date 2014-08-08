#region References

using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Drawing;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using ClipperLib;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text;

#endregion

namespace GX_Soraka
{
    class Program
    {
        private const string localversion = "Updated for L# 08.06.2014 and L# Common 08.06.2014";
        internal static bool isInitialized;

        internal static bool packets;

        public static TargetSelector TS;
        public static SpellDataInst EXHAUST;
        public static SpellDataInst IGNITE;

        public const string ChampionName = "Soraka";

        public static Obj_AI_Hero MyHero;

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public static Menu Config;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(System.EventArgs args)
        {

            if (ObjectManager.Player.ChampionName != ChampionName) return;

            TS = new TargetSelector(1000, TargetSelector.TargetingMode.AutoPriority);

            //AuotUpdater();

            MyHero = (ObjectManager.Player);

            Q = new Spell(SpellSlot.Q, 530f);
            W = new Spell(SpellSlot.W, 750f);
            E = new Spell(SpellSlot.E, 725f);
            R = new Spell(SpellSlot.R, float.MaxValue);

            IGNITE = GetSummoner("SummonerDot");
            EXHAUST = GetSummoner("SummonerExhaust");

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;

            Config = new Menu(ChampionName, ChampionName, true);
            Config.AddToMainMenu();

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Target Selector", "TargetSelector"));
            Config.SubMenu("TargetSelector").AddItem(new MenuItem("TargetingMode", "Mode")).SetValue(new StringList(new[] { "AutoPriority", "Closest", "LessAttack", "LessCast", "LowHP", "MostAD", "MostAP", "NearMouse" }, 0));
            Config.SubMenu("TargetSelector").AddItem(new MenuItem("TargetingRange", "Range")).SetValue(new Slider(750, 2000, 100));

            //Config.AddSubMenu(new Menu("Auto Ult Heal", "SaveLives"));
            //Config.SubMenu("SaveLives").AddItem(new MenuItem("RSave", "Ult Heal Endangered?").SetValue(true));
            //foreach (Obj_AI_Hero Champ in ObjectManager.Get<Obj_AI_Hero>())
            //    if (Champ.IsAlly)
            //        Config.SubMenu("SaveLives").AddItem(new MenuItem("Save" + Champ.BaseSkinName, string.Format("Save {0}", Champ.BaseSkinName)).SetValue(true));
            //Config.SubMenu("SaveLives").AddItem(new MenuItem("HPPercent", "HP Percent to Trigger").SetValue(new Slider(30, 100, 0)));

            Config.AddSubMenu(new Menu("Rotation EXPERIMENTAL", "Rotation"));
            Config.SubMenu("Rotation").AddItem(new MenuItem("Slot1", "Use:")).SetValue(new StringList(new[] { "Starcall", "Astral Blessing", "Infuse - Silence", "Wish", "Infuse - Mana", "Exhaust", "Ignite", "NONE" }, 1));
            Config.SubMenu("Rotation").AddItem(new MenuItem("Slot2", "Use:")).SetValue(new StringList(new[] { "Starcall", "Astral Blessing", "Infuse - Silence", "Wish", "Infuse - Mana", "Exhaust", "Ignite", "NONE" }, 1));
            Config.SubMenu("Rotation").AddItem(new MenuItem("Slot3", "Use:")).SetValue(new StringList(new[] { "Starcall", "Astral Blessing", "Infuse - Silence", "Wish", "Infuse - Mana", "Exhaust", "Ignite", "NONE" }, 1));
            Config.SubMenu("Rotation").AddItem(new MenuItem("Slot4", "Use:")).SetValue(new StringList(new[] { "Starcall", "Astral Blessing", "Infuse - Silence", "Wish", "Infuse - Mana", "Exhaust", "Ignite", "NONE" }, 1));
            Config.SubMenu("Rotation").AddItem(new MenuItem("Slot5", "Use:")).SetValue(new StringList(new[] { "Starcall", "Astral Blessing", "Infuse - Silence", "Wish", "Infuse - Mana", "Exhaust", "Ignite", "NONE" }, 1));
            Config.SubMenu("Rotation").AddItem(new MenuItem("Slot6", "Use:")).SetValue(new StringList(new[] { "Starcall", "Astral Blessing", "Infuse - Silence", "Wish", "Infuse - Mana", "Exhaust", "Ignite", "NONE" }, 1));
            Config.SubMenu("Rotation").AddItem(new MenuItem("Slot7", "Use:")).SetValue(new StringList(new[] { "Starcall", "Astral Blessing", "Infuse - Silence", "Wish", "Infuse - Mana", "Exhaust", "Ignite", "NONE" }, 1));
            Config.SubMenu("Rotation").AddItem(new MenuItem("Slot8", "Use:")).SetValue(new StringList(new[] { "Starcall", "Astral Blessing", "Infuse - Silence", "Wish", "Infuse - Mana", "Exhaust", "Ignite", "NONE" }, 1));

            Config.SubMenu("Rotation").AddItem(new MenuItem("RotaActive", "Rotation!").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("Rotation").AddItem(new MenuItem("GXNULL", "Set Skills in Order of Priority"));
            Config.SubMenu("Rotation").AddItem(new MenuItem("GXNULL", "Top to Bottom or NONE to disable"));

            //Config.AddSubMenu(new Menu("Harass", "Harass"));
            //Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            //Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //Config.AddSubMenu(new Menu("Lane Clear", "Laneclear"));
            //Config.SubMenu("Laneclear").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
            //Config.SubMenu("Laneclear").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //Config.AddSubMenu(new Menu("Last Hit Q", "LastHit"));
            //Config.SubMenu("LastHit").AddItem(new MenuItem("LastHitQ", "Use Q to Last Hit").SetValue(true));
            //Config.SubMenu("LastHit").AddItem(new MenuItem("LastHitActive", "Last Hit!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Packet Casting", "PacketCast"));
            Config.SubMenu("PacketCast").AddItem(new MenuItem("Packets", "Use Packet Casting").SetValue(true));

            Config.AddSubMenu(new Menu("Drawing", "Drawing"));
            Config.SubMenu("Drawing").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawing").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawing").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawing").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            InternalSetTargetingMode();

            packets = Config.Item("Packets").GetValue<bool>();
            if ((Config.Item("RotaActive").GetValue<KeyBind>().Active))
            {
                Rotation();
            }
        }

        private static void Rotation()
        {
            Console.WriteLine("Calling Rotation {0}");
            var Slot1 = Config.Item("Slot1").GetValue<StringList>().SelectedIndex;
            Console.WriteLine("Slot 1 Index {0}", Slot1);
            switch (Slot1)
            {
                case 0:
                    Starcall();
                    break;
                case 1:
                    AstralBlessing();
                    break;
                case 2:
                    InfuseSilence();
                    break;
                case 3:
                    Wish();
                    break;
                case 4:
                    InfuseMana();
                    break;
                case 5:
                    Exhaust();
                    break;
                case 6:
                    Ignite();
                    break;
                case 7:
                    //Do Nothing
                    break;
            }
        }

        private static void Ignite()
        {
            Console.WriteLine("Calling Ignite for {0}", TS.Target.BaseSkinName);

            if ((TS.Target != null) && (IGNITE.Cooldown <= 0) && (TS.Target.IsValidTarget(IGNITE.SData.CastRange[0])) && (TS.Target.Health < DamageLib.getDmg(TS.Target, DamageLib.SpellType.IGNITE)))
                MyHero.SummonerSpellbook.CastSpell(IGNITE.Slot, TS.Target);
        }

        private static void Exhaust()
        {
            //Console.WriteLine("Calling Exhaust for {0}", TS.Target.BaseSkinName);
            if (TS.Target != null)
            {
                //Console.WriteLine("Target Not Null");
                if (EXHAUST.Cooldown <= 0)
                {
                    //Console.WriteLine("Exhaust is ready");
                    if (TS.Target.IsValidTarget(EXHAUST.SData.CastRange[0]))
                    {
                        //Console.WriteLine("Target is within Exhaust Range");
                        MyHero.SummonerSpellbook.CastSpell(EXHAUST.Slot, TS.Target);
                    }
                }
            }


        }

        private static void InfuseMana()
        {
            Console.WriteLine("Calling Infuse");
            foreach (Obj_AI_Hero Champ in ObjectManager.Get<Obj_AI_Hero>())
                if ((Champ != null) && (!Champ.IsDead) && (E.IsReady()) && (Champ.IsAlly) && (Champ.Mana <= (Champ.MaxMana * 0.50)) && (Champ.IsValidTarget(E.Range)))
                    E.CastOnUnit(Champ, packets);
        }

        private static void Wish()
        {
            Console.WriteLine("Calling Wish");
            foreach (Obj_AI_Hero Champ in ObjectManager.Get<Obj_AI_Hero>())
                if ((Champ != null) && (!Champ.IsDead) && (R.IsReady()) && (Champ.IsAlly) && (Champ.Health <= (Champ.MaxHealth * 0.30)) && (Champ.IsValidTarget(R.Range)))
                    R.Cast(null, packets);
        }

        private static void InfuseSilence()
        {
            Console.WriteLine("Calling Silence");
            foreach (Obj_AI_Hero Champ in ObjectManager.Get<Obj_AI_Hero>())
                if ((Champ != null) && (E.IsReady()) && (Champ.IsEnemy) && (Champ.IsChannelingImportantSpell()) && (Champ.IsValidTarget(E.Range)))
                    E.CastOnUnit(Champ, packets);
        }

        private static void AstralBlessing()
        {
            Console.WriteLine("Calling Astral Blessing");
            foreach (Obj_AI_Hero Champ in ObjectManager.Get<Obj_AI_Hero>())
                if ((Champ != null) && (!Champ.IsDead) && (W.IsReady()) && (Champ.IsAlly) && (Champ.Health < (Champ.MaxHealth) && (Champ.IsValidTarget(W.Range))))
                {
                    Console.WriteLine("Trying to cast Astral Blessing");
                    W.CastOnUnit(Champ, packets);
                }
        }

        private static void Starcall()
        {
            Console.WriteLine("Calling Starcall");
            foreach (Obj_AI_Hero Champ in ObjectManager.Get<Obj_AI_Hero>())
                if ((Champ != null) && (Champ.IsValidTarget(Q.Range)))
                    Q.Cast(null, packets);
        }

        private static SpellDataInst GetSummoner(string p)
        {
            var summspells = ObjectManager.Player.SummonerSpellbook.Spells;
            return summspells.FirstOrDefault(spell => spell.Name == p);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Spell[] spellList = { Q, W, E, R };
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Utility.DrawCircle(MyHero.Position, spell.Range, menuItem.Color);
                }
            }
        }

        private static void InternalSetTargetingMode()
        {
            float TSRange = Config.Item("TargetingRange").GetValue<Slider>().Value;
            TS.SetRange(TSRange);
            var mode = Config.Item("TargetingMode").GetValue<StringList>().SelectedIndex;

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
        }

        /*
                #region Auto Updater

                private static void AuotUpdater()
                {
                    isInitialized = true;
                    Game.PrintChat(string.Format("{0} version {1} loaded.", Assembly.GetExecutingAssembly().GetName().Name, localversion));
                    var bgw = new BackgroundWorker();
                    bgw.DoWork += bgw_DoWork;
                    bgw.RunWorkerAsync();
                }

                static void bgw_DoWork(object sender, DoWorkEventArgs e)
                {
                    var myUpdater = new Updater("https://raw.githubusercontent.com/Xionanx/GX_Soraka/master/Release/version.version",
                            "https://github.com/Xionanx/Releases/raw/master/GX%20Sona.exe", localversion);

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
                */
    }
}
