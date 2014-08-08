using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace GXAutoShield
{
    class GXAutoShield
    {
        SpellSlot Q_Slot;
        string Q_Effect = "none"; // Heal, Shield, Block (=1 Ability), Barrier (= Magic Only), Wall, EscapeDeath
        string Q_Target = "none"; // Self, Unit, Location
        float Q_Range = 0;
        SpellSlot W_Slot;
        string W_Effect = "none"; // Heal, Shield, Block (=1 Ability), Barrier (= Magic Only), Wall, EscapeDeath 
        string W_Target = "none"; // Self, Unit, Location
        float W_Range = 0;
        SpellSlot E_Slot;
        string E_Effect = "none"; // Heal, Shield, Block (=1 Ability), Barrier (= Magic Only), Wall, EscapeDeath
        string E_Target = "none"; // Self, Unit, Location
        float E_Range = 0;
        SpellSlot R_Slot;
        string R_Effect = "none"; // Heal, Shield, Block (=1 Ability), Barrier (= Magic Only), Wall, EscapeDeath
        string R_Target = "none"; // Self, Unit, Location
        float R_Range = 0;
        SpellSlot _SummonerBarrierSlot;
        SpellSlot _SummonerHealSlot;
        int SummonerHealRange = 0;
        int SommonerBarrierRange = 0;
        bool HasEscapeDeath = false;
        SpellSlot EscapeDeathSlot;
        bool HasHeal = false;
        SpellSlot HealSlot1;
        SpellSlot HealSlot2;
        bool HasShield = false;
        SpellSlot ShieldSlot1;
        SpellSlot ShieldSlot2;
        bool HasWall = false;
        SpellSlot Wallslot;
        bool HasBlock = false;
        SpellSlot BlockSlot;
        bool HasBarrier = false;
        SpellSlot BarrierSlot;
        string ChampName = "None";

        public GXAutoShield()
        {
            Game.OnGameStart += Game_OnGameStart;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs attack)
        {

            var AttackEnd = new Vector3(attack.End.X, attack.End.Y, attack.End.Z);
            var AttackStart = new Vector3(attack.Start.X, attack.Start.Y, attack.Start.Z);
            

            //float AttackDamage = 0;
            // Check if the originating source is an enemy, if you are alive, and if its not a lane minion.
            if ((unit.IsEnemy) && (!ObjectManager.Player.IsDead)  && (!unit.IsMinion))
                Game.PrintChat(string.Format("Being Attacked by {0} using {1} which is attack type {2}!", unit.Name, attack.SData.Name, attack.SData.CastFrame));  // To be commented out at a later date
                
                
                // Then find the TARGETED champion..
                foreach (Obj_AI_Hero champ in ObjectManager.Get<Obj_AI_Hero>())
                {
                    var ChampPosition = new Vector3(champ.Position.X, champ.Position.Y, champ.Position.Z);
                    
                    // is the champ an ally, is the champ still alive, is the champ invulnerable (no point in shielding if it is), and can you actually target the champ (blood pool).
                    if (AttackEnd == ChampPosition)

                        

                        // All true?  then try to predict if the champ is going to be hit.
                        //  I have reached a dead end on hit prediction ATM, its out of my knowledge base..  for now, assuming all attempts hit.
                        
                            // Try to calculate how much damange the champ is going to take
                            Game.PrintChat(string.Format("{0] was attacked by {1} using {2}!", champ.Name, unit.Name, attack.SData.Name));  // To be commented out at a later date
                        

                        //  IF Champ = HIT then its time to do something about it....



                        if (HasWall == true)  //  && additional conditions to check if the attack is wallable
                            Wall(champ, attack);
                        else if (HasShield == true)  //  && additional conditions to check if the attack is shieldable
                            Shield();
                        else if (HasBarrier == true)  //  && additional considtion to check if the attack is barrierable
                            Barrier();
                        else if (HasBlock == true)  //  && additional check to see if blockable
                            Block();
                        else if (HasHeal == true)  //  && additional checks etc etc blah blah
                            Heal();
                        else if (HasEscapeDeath == true)  //  && additional checks to see if this is worth the time
                            EscapeDeath();
                       
                }
        }

        private void Wall(Obj_AI_Hero champ, GameObjectProcessSpellCastEventArgs attack)
        {
            ObjectManager.Player.Spellbook.CastSpell(Wallslot, attack.Start);
        }

        private void Block()
        {

            //throw new NotImplementedException();
        }

        private void Barrier()
        {
            //throw new NotImplementedException();
        }

        private void EscapeDeath()
        {
           // throw new NotImplementedException();
        }

        private void Heal()
        {
           // throw new NotImplementedException();
        }

        private void Shield()
        {
            // Check if your shield spell is ready.
           // SpellState Ready = (ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.Q));
           // if (Ready == SpellState.Ready)
                //do something
                
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            SpellReadyCheck();
        }

        private void SpellReadyCheck()
        {
            bool Q_Ready = false;
            bool W_Ready = false;
            bool E_Ready = false;
            bool R_Ready = false;

            SpellState qstate = ObjectManager.Player.Spellbook.GetSpell(Q_Slot).State;
            SpellState wstate = ObjectManager.Player.Spellbook.GetSpell(W_Slot).State;
            SpellState estate = ObjectManager.Player.Spellbook.GetSpell(E_Slot).State;
            SpellState rstate = ObjectManager.Player.Spellbook.GetSpell(R_Slot).State;

            if (qstate == SpellState.Ready) Q_Ready = true;
            if (wstate == SpellState.Ready) W_Ready = true;
            if (estate == SpellState.Ready) E_Ready = true;
            if (rstate == SpellState.Ready) R_Ready = true;


            if (((Q_Effect == "Heal") && (Q_Ready)) || ((W_Effect == "Heal") && (W_Ready)) || ((E_Effect == "Heal") && (E_Ready)) || ((R_Effect == "Heal") && (R_Ready)))
                HasHeal = true;
            if (((Q_Effect == "Shield") && (Q_Ready)) || ((W_Effect == "Shield") && (W_Ready)) || ((E_Effect == "Shield") && (E_Ready)) || ((R_Effect == "Shield") && (R_Ready)))
                HasShield = true;
            if (((Q_Effect == "Wall") && (Q_Ready)) || ((W_Effect == "Wall") && (W_Ready)) || ((E_Effect == "Wall") && (E_Ready)) || ((R_Effect == "Wall") && (R_Ready)))
                HasWall = true;
            if (((Q_Effect == "Barrier") && (Q_Ready)) || ((W_Effect == "Barrier") && (W_Ready)) || ((E_Effect == "Barrier") && (E_Ready)) || ((R_Effect == "Barrier") && (R_Ready)))
                HasBarrier = true;
            if (((Q_Effect == "Block") && (Q_Ready)) || ((W_Effect == "Block") && (W_Ready)) || ((E_Effect == "Block") && (E_Ready)) || ((R_Effect == "Block") && (R_Ready)))
                HasBlock = true;
            if (((Q_Effect == "EscapeDeath") && (Q_Ready)) || ((W_Effect == "EscapeDeath") && (W_Ready)) || ((E_Effect == "EscapeDeath") && (E_Ready)) || ((R_Effect == "EscapeDeath") && (R_Ready)))
                HasEscapeDeath = true;


        }

        private void FindPlayerItems()
        {
            //throw new NotImplementedException();
        }

        void Game_OnGameStart(EventArgs args)
        {

            Q_Slot = SpellSlot.Q;
            Q_Range = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange[0];
            W_Slot = SpellSlot.W;
            W_Range = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange[0];
            E_Slot = SpellSlot.E;
            E_Range = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange[0];
            R_Slot = SpellSlot.R;
            R_Range = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange[0];
            GetPlayerChampion();
            Game.PrintChat(string.Format("Champion {0} loaded.", ChampName));  //For Debug Purposess, comment out when finalized.
            Game.PrintChat(string.Format("Spell Types: {0} {1} {2} {3}", Q_Effect, W_Effect, E_Effect, R_Effect));
        }

        private void GetPlayerChampion()
        {
            ChampName = ObjectManager.Player.BaseSkinName;

            Game.PrintChat(string.Format("Champion Name:{0}",ChampName));
            Game.PrintChat(string.Format("Spell Types: {0} {1} {2} {3}", Q_Effect, W_Effect, E_Effect, R_Effect));
            
            switch (ChampName)
            {
                case "Sona":
                    Q_Effect = "Heal"; // Heal, Shield, Block (=1 Ability), Barrier (= Magic Only), Wall
                    Q_Target = "SelfAoE"; // Self, Unit, Location, SelfAoE
                    HealSlot1 = SpellSlot.Q;
                    break;
                case "Lulu":
                    E_Effect = "Shield";
                    E_Target = "Unit";
                    ShieldSlot1 = SpellSlot.E;
                    R_Effect = "Heal";
                    R_Target = "Unit";
                    HealSlot1 = SpellSlot.R;
                    break;
                case "Janna":
                    E_Effect = "Shield";
                    E_Target = "Unit";
                    ShieldSlot1 = SpellSlot.E;
                    break;
                case "Karma":
                    E_Effect = "Shield";
                    E_Target = "Unit";
                    ShieldSlot1 = SpellSlot.E;
                    break;
                case "LeeSin":
                    W_Effect = "Shield";
                    W_Target = "Unit";
                    ShieldSlot1 = SpellSlot.W;
                    break;
                case "Orianna":
                    E_Effect = "Shield";
                    E_Target = "Unit";
                    ShieldSlot1 = SpellSlot.E;
                    break;
                case "Lux":
                    W_Effect = "Shield";
                    W_Target = "Location";
                    ShieldSlot1 = SpellSlot.W;
                    break;
                case "Thresh":
                    W_Effect = "Shield";
                    W_Target = "Location";
                    ShieldSlot1 = SpellSlot.W;
                    break;
                case "JarvvanIV":
                    W_Effect = "Shield";
                    W_Target = "Self";
                    ShieldSlot1 = SpellSlot.W;
                    break;
                case "Nautilus":
                    W_Effect = "Shield";
                    W_Target = "self";
                    ShieldSlot1 = SpellSlot.W;
                    break;
                case "Rumble":
                    W_Effect = "Shield";
                    W_Target = "Self";
                    ShieldSlot1 = SpellSlot.W;
                    break;
                case "Shen":
                    W_Effect = "Shield";
                    W_Target = "Self";
                    ShieldSlot1 = SpellSlot.W;
                    R_Effect = "Shield";
                    R_Target = "Unit";
                    ShieldSlot2 = SpellSlot.R;
                    break;
                case "Sion":
                    W_Effect = "Shield";
                    W_Target = "Self";
                    ShieldSlot1 = SpellSlot.W;
                    break;
                case "Skarner":
                    W_Effect = "Shield";
                    W_Target = "Self";
                    ShieldSlot1 = SpellSlot.W;
                    break;
                case "Urgot":
                    W_Effect = "Shield";
                    W_Target = "Self";
                    ShieldSlot1 = SpellSlot.W;
                    break;
                case "Diana":
                    W_Effect = "Shield";
                    W_Target = "Self";
                    ShieldSlot1 = SpellSlot.W;
                    break;
                case "Udyr":
                    W_Effect = "Shield";
                    W_Target = "Self";
                    ShieldSlot1 = SpellSlot.W;
                    break;
                case "Riven":
                    E_Effect = "Shield";
                    E_Target = "Location";
                    ShieldSlot1 = SpellSlot.E;
                    break;
                case "Morgana":
                    E_Effect = "Barrier";
                    E_Target = "Unit";
                    BarrierSlot = SpellSlot.E;
                    break;
                case "Sivir":
                    E_Effect = "Block";
                    E_Target = "Self";
                    BlockSlot = SpellSlot.E;
                    break;
                case "Nocturne":
                    W_Effect = "Block";
                    W_Target = "Self";
                    BlockSlot = SpellSlot.W;
                    break;
                case "Alistar":
                    E_Effect = "Heal";
                    E_Target = "SelfAoE";
                    HealSlot1 = SpellSlot.E;
                    break;
                case "Kayle":
                    W_Effect = "Heal";
                    W_Target = "Unit";
                    HealSlot1 = SpellSlot.W;
                    R_Effect = "Shield";
                    R_Target = "Unit";
                    ShieldSlot1 = SpellSlot.R;
                    break;
                case "Nami":
                    W_Effect = "Heal";
                    W_Target = "Unit";
                    HealSlot1 = SpellSlot.W;
                    break;
                case "Nidalee":
                    E_Effect = "Heal";
                    E_Target = "Unit";
                    HealSlot1 = SpellSlot.E;
                    break;
                case "Soraka":
                    W_Effect = "Heal";
                    W_Target = "Unit";
                    HealSlot1 = SpellSlot.W;
                    R_Effect = "Heal";
                    R_Target = "SelfAoE";
                    HealSlot2 = SpellSlot.R;
                    break;
                case "Taric":
                    Q_Effect = "Heal";
                    Q_Target = "Unit";
                    HealSlot1 = SpellSlot.Q;
                    break;
                case "Gangplank":
                    W_Effect = "Heal";
                    W_Target = "Self";
                    HealSlot1 = SpellSlot.W;
                    break;
                case "Zilean":
                    R_Effect = "EscapeDeath";
                    R_Target = "Unit";
                    EscapeDeathSlot = SpellSlot.R;
                    break;
                case "Tryndamere":
                    R_Effect = "EscapeDeath";
                    R_Target = "Self";
                    EscapeDeathSlot = SpellSlot.R;
                    break;
                case "Yasou":
                    W_Effect = "Wall";
                    W_Target = "Location";
                    Wallslot = SpellSlot.W;
                    break;
                case "Bruam":
                    E_Effect = "Wall";
                    E_Target = "Self";
                    Wallslot = SpellSlot.E;
                    break;
            }
        }
    }
}
