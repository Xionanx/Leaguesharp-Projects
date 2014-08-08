#region

using LeagueSharp;
using LeagueSharp.Common;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System;

#endregion

namespace GX_Karthus
{
    class Program
    {
        private const string localversion = "Updated for L# 08.06.2014 and L# Common 08.06.2014";
        internal static bool isInitialized;

        public const string ChampionName = "Karthus";

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

            MyHero = (ObjectManager.Player);

            Game.PrintChat(string.Format("{0} v{1} loaded.", Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version));

            Q = new Spell(SpellSlot.Q, 875);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 500);
            R = new Spell(SpellSlot.R, 25000);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Config = new Menu(ChampionName, ChampionName, true);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Lane Clear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Last Hit Q", "LastHitQ"));
            Config.SubMenu("LastHitQ").AddItem(new MenuItem("LastHitQ", "Use Q to Last Hit").SetValue(true));
            Config.SubMenu("LastHitQ").AddItem(new MenuItem("LastHitActive", "Last Hit!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Auto Ult", "AutoUlt"));
            Config.SubMenu("AutoUlt").AddItem(new MenuItem("AutoUlt", "Auto Ult for Kills").SetValue(true));

            Config.AddSubMenu(new Menu("Drawing", "Drawing"));
            Config.SubMenu("Drawing").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawing").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawing").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawing").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));

            Config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;

        }

        private static void Game_OnGameUpdate(System.EventArgs args)
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);

            if (Config.Item("AutoUlt").GetValue<bool>() && R.IsReady())
            {
                AutoULt();
            }

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo(target);
            }

            if (Config.Item("LastHitActive").GetValue<KeyBind>().Active)
            {
                LastHit();
            }

            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                Harass(target);
            }

            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
            {
                LaneClear();
            }
            if ((target == null) && (MyHero.Spellbook.GetSpell(SpellSlot.E).ToggleState == 2) && (!Config.Item("LaneClearActive").GetValue<KeyBind>().Active))
            {
                E.Cast();
            }
        }

        private static void LastHit()
        {
            var Minions = MinionManager.GetMinions(MyHero.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
            if (Q.IsReady() && Config.Item("LastHitQ").GetValue<bool>())
                foreach (var minion in Minions)
                    if (minion.IsValidTarget(Q.Range) && HealthPrediction.GetHealthPrediction(minion, (int)Q.Delay) < (DamageLib.getDmg(minion, DamageLib.SpellType.Q, DamageLib.StageType.FirstDamage)) && (Q.GetPrediction(minion, true).HitChance >= Prediction.HitChance.HighHitchance))
                        Q.CastOnUnit(minion, false);
        }

        private static void AutoULt()
        {
            var ultTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            if (DamageLib.getDmg(ultTarget, DamageLib.SpellType.R) > (ultTarget.Health * 1.05))
            {
                MyHero.Spellbook.CastSpell(SpellSlot.R);
            }
        }

        private static void Combo(Obj_AI_Hero target)
        {
            if (target != null)
            {
                if (Q.IsReady() && Config.Item("UseQCombo").GetValue<bool>())
                    if (Q.GetPrediction(target, true).HitChance >= Prediction.HitChance.HighHitchance)
                        if (MyHero.ServerPosition.Distance(Q.GetPrediction(target, true).Position) < Q.Range)
                            Q.Cast(Q.GetPrediction(target, true).Position, false);

                if (W.IsReady() && Config.Item("UseWCombo").GetValue<bool>())
                    if (W.GetPrediction(target, true).HitChance >= Prediction.HitChance.HighHitchance)
                        if (MyHero.ServerPosition.Distance(W.GetPrediction(target, true).Position) < W.Range)
                            W.Cast(W.GetPrediction(target, true).Position, false);

                if (E.IsReady() && Config.Item("UseECombo").GetValue<bool>() && (MyHero.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1))
                    if (MyHero.ServerPosition.Distance(target.Position) < E.Range)
                        E.Cast();

                if (E.IsReady() && Config.Item("UseECombo").GetValue<bool>() && (MyHero.Spellbook.GetSpell(SpellSlot.E).ToggleState == 2))
                    if (MyHero.ServerPosition.Distance(target.Position) > E.Range)
                        E.Cast();
            }
        }

        private static void LaneClear()
        {
            var Minions = MinionManager.GetMinions(MyHero.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
            if ((Q.GetCircularFarmLocation(Minions).MinionsHit >= 1) && Q.IsReady() && Config.Item("UseQFarm").GetValue<bool>())
                Q.Cast(Q.GetCircularFarmLocation(Minions).Position, false);
            if ((E.GetCircularFarmLocation(Minions).MinionsHit >= 2) && E.IsReady() && Config.Item("UseEFarm").GetValue<bool>() && (MyHero.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1))
                E.Cast(MyHero.ServerPosition, false);
            if ((E.GetCircularFarmLocation(Minions).MinionsHit <= 1) && E.IsReady() && Config.Item("UseEFarm").GetValue<bool>() && (MyHero.Spellbook.GetSpell(SpellSlot.E).ToggleState == 2))
                E.Cast(MyHero.ServerPosition, false);
        }

        private static void Harass(Obj_AI_Hero target)
        {
            if (target != null)
            {
                if (Q.IsReady() && Config.Item("UseQHarass").GetValue<bool>())
                    if (Q.GetPrediction(target, true).HitChance >= Prediction.HitChance.HighHitchance)
                        if (MyHero.ServerPosition.Distance(Q.GetPrediction(target, true).Position) < Q.Range)
                            Q.Cast(Q.GetPrediction(target, true).Position, false);

                if (W.IsReady() && Config.Item("UseWHarass").GetValue<bool>())
                    if (W.GetPrediction(target, true).TargetsHit >= 1)
                        if (MyHero.ServerPosition.Distance(W.GetPrediction(target, true).Position) < W.Range)
                            W.Cast(W.GetPrediction(target, true).Position, false);

                if (E.IsReady() && Config.Item("UseEHarass").GetValue<bool>() && (MyHero.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1))
                    if (MyHero.ServerPosition.Distance(target.Position) < E.Range)
                        E.Cast();

                if (E.IsReady() && Config.Item("UseEHarass").GetValue<bool>() && (MyHero.Spellbook.GetSpell(SpellSlot.E).ToggleState == 2))
                    if (MyHero.ServerPosition.Distance(target.Position) > E.Range)
                        E.Cast();
            }

        }

        private static void Drawing_OnDraw(System.EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Utility.DrawCircle(MyHero.Position, spell.Range, menuItem.Color);
                }
            }
        }
    }
}
