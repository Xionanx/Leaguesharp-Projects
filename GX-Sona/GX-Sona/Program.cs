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

namespace GX_Sona
{
    class Program
    {
        private const string localversion = "Updated for L# 08.06.2014 and L# Common 08.06.2014";
        internal static bool isInitialized;

        internal static bool packets;

        public const string ChampionName = "Sona";

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
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            AuotUpdater();

            MyHero = (ObjectManager.Player);

            Q = new Spell(SpellSlot.Q, 700);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R, 1000);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Config = new Menu(ChampionName, ChampionName, true);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            var TargetSelectorMenu = new Menu("Target Selector", "TargetSelector");
            SimpleTs.AddToMenu(TargetSelectorMenu);
            Config.AddSubMenu(TargetSelectorMenu);

            Config.AddSubMenu(new Menu("Auto Target Ult", "AutoTargetUlt"));
            Config.SubMenu("AutoTargetUlt").AddItem(new MenuItem("AtUlt", "Auto Targetting?").SetValue(true));
            foreach (Obj_AI_Hero Champ in ObjectManager.Get<Obj_AI_Hero>())
                if (Champ.IsEnemy)
                    Config.SubMenu("AutoTargetUlt").AddItem(new MenuItem("Ult" + Champ.BaseSkinName, string.Format("Ult {0}", Champ.BaseSkinName)).SetValue(true));
            Config.SubMenu("AutoTargetUlt").AddItem(new MenuItem("GXNULL", "Recommend Turning OFF Ulting for Tanks"));
            Config.SubMenu("AutoTargetUlt").AddItem(new MenuItem("AutoUltActive", "Auto Target Ult!").SetValue(new KeyBind("R".ToCharArray()[0], KeyBindType.Press)));

            /*          Conservation options will require a keyboard hook which I'm not ready to implement at this time.
             * 
                        menu.AddSubMenu(new Menu("Conserve Mana", "Conserve"));
                        menu.SubMenu("Conserve").AddItem(new MenuItem("ConserveQ", "Prevent Q Use").SetValue(true));
                        menu.SubMenu("Conserve").AddItem(new MenuItem("ConserveW", "Prevent W Use").SetValue(true));
                        menu.SubMenu("Conserve").AddItem(new MenuItem("ConserveE", "Prevent E Use").SetValue(true));
                        menu.SubMenu("Conserve").AddItem(new MenuItem("GXNULL", "Prevents skills if no one effected!"));
             */
            Config.AddSubMenu(new Menu("Rotation", "Rotation"));
            Config.SubMenu("Rotation").AddItem(new MenuItem("UseQRota", "Q in Rotation").SetValue(true));
            Config.SubMenu("Rotation").AddItem(new MenuItem("UseWRota", "W in Rotation").SetValue(true));
            Config.SubMenu("Rotation").AddItem(new MenuItem("UseERota", "E in Rotation").SetValue(true));
            Config.SubMenu("Rotation").AddItem(new MenuItem("UseRRota", "R in Rotation").SetValue(true));
            Config.SubMenu("Rotation").AddItem(new MenuItem("RotaActive", "Rotation!").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("Rotation").AddItem(new MenuItem("GXNULL", "Only uses Rotation skills if"));
            Config.SubMenu("Rotation").AddItem(new MenuItem("GXNULL", "an Enemy is in range"));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Lane Clear", "Laneclear"));
            Config.SubMenu("Laneclear").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
            Config.SubMenu("Laneclear").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Last Hit Q", "LastHit"));
            Config.SubMenu("LastHit").AddItem(new MenuItem("LastHitQ", "Use Q to Last Hit").SetValue(true));
            Config.SubMenu("LastHit").AddItem(new MenuItem("LastHitActive", "Last Hit!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Packet Casting", "PacketCast"));
            Config.SubMenu("PacketCast").AddItem(new MenuItem("Packets", "Use Packet Casting").SetValue(true));

            Config.AddSubMenu(new Menu("Drawing", "Drawing"));
            Config.SubMenu("Drawing").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawing").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawing").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawing").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

            Config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            packets = Config.Item("Packets").GetValue<bool>();

            {
                if ((Config.Item("AutoUltActive").GetValue<KeyBind>().Active) && (target != null))
                {
                    AutoUlt(target);
                }
                if ((Config.Item("RotaActive").GetValue<KeyBind>().Active) && (target != null))
                {
                    Rotation(target);
                }

                if (Config.Item("LastHitActive").GetValue<KeyBind>().Active)
                {
                    LastHit();
                }

                if ((Config.Item("HarassActive").GetValue<KeyBind>().Active) && (target != null))
                {
                    Harass(target);
                }

                if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
                {
                    LaneClear();
                }
            }
        }

        private static void AutoUlt(Obj_AI_Hero target)
        {
            if ((target.IsValidTarget(R.Range)) && (R.IsReady()) && (Config.Item("AtUlt").GetValue<bool>()))
            {
                if ((R.GetPrediction(target, true).HitChance >= Prediction.HitChance.HighHitchance) && (Config.Item("Ult" + target.BaseSkinName).GetValue<bool>()))
                {
                    R.Cast(R.GetPrediction(target, packets).Position, false);
                }
            }
        }

        private static void Rotation(Obj_AI_Hero target)
        {
            if ((target.IsValidTarget(Q.Range)) && (Q.IsReady()) && (Config.Item("UseQRota").GetValue<bool>()))
            {
                {
                    Q.Cast(Q.GetPrediction(target, packets).Position, false);
                }
            }
            if ((target.IsValidTarget(W.Range)) && (W.IsReady()) && (Config.Item("UseQRota").GetValue<bool>()))
            {
                {
                    W.Cast(W.GetPrediction(target, packets).Position, false);
                }
            }
            if ((target.IsValidTarget(E.Range)) && (E.IsReady()) && (Config.Item("UseQRota").GetValue<bool>()))
            {
                {
                    E.Cast(E.GetPrediction(target, packets).Position, false);
                }
            }
            if ((target.IsValidTarget(R.Range)) && (R.IsReady()) && (Config.Item("UseQRota").GetValue<bool>()))
            {
                if ((R.GetPrediction(target, true).HitChance >= Prediction.HitChance.HighHitchance) && (Config.Item("Ult" + target.BaseSkinName).GetValue<bool>()))
                {
                    R.Cast(R.GetPrediction(target, true).Position, packets);
                }
            }
        }

        private static void LastHit()
        {
            var Minions = MinionManager.GetMinions(MyHero.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
            foreach (var minion in Minions)
                if ((Config.Item("LastHitQ").GetValue<bool>()) && (minion.IsValidTarget(Q.Range)) && (HealthPrediction.GetHealthPrediction(minion, (int)Q.Delay) < (DamageLib.getDmg(minion, DamageLib.SpellType.Q) * 0.9)))
                    Q.Cast(minion, packets);

        }

        private static void Harass(Obj_AI_Hero target)
        {
            if ((target.IsValidTarget(Q.Range)) && (Q.IsReady()) && (Config.Item("UseQHarass").GetValue<bool>()))
            {

                Q.Cast(target, packets);

            }
        }

        private static void LaneClear()
        {
            var Minions = MinionManager.GetMinions(MyHero.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
            foreach (var minion in Minions)
                if ((Config.Item("UseQFarm").GetValue<bool>()) && (minion.IsValidTarget(Q.Range)) && (HealthPrediction.GetHealthPrediction(minion, (int)Q.Delay) < (DamageLib.getDmg(minion, DamageLib.SpellType.Q) * 0.9)))
                    Q.Cast(minion, packets);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
                }
            }
        }

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
            var myUpdater = new Updater("https://raw.githubusercontent.com/Xionanx/GX-Sona/master/Release/version.version",
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
    }
}
