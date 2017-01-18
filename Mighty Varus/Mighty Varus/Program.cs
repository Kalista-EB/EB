using System.Collections.Generic;
using System.Text;
using Font = SharpDX.Direct3D9.Font;
using SharpDX.Direct3D9;
using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace Mighty_Varus
{
    class Program
    {
        private static readonly Vector2 Offset = new Vector2(1, 0);
        private static Menu VarusMenu, comboMenu, waveClear, LastHit, Drawings, harass, miscMenu;
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        
        private static AIHeroClient User = Player.Instance;
        private static Spell.Chargeable Q;
        private static Spell.Active W;
        private static Spell.Skillshot E;
        private static Spell.Skillshot R;
        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            //Gapcloser.OnGapcloser += AntiGapCloser;
            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Q = new Spell.Chargeable(SpellSlot.Q, 1000, 1600, 1300, 0, 1900, 70) { AllowedCollisionCount = int.MaxValue };
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Skillshot(SpellSlot.E, 925, SkillShotType.Circular, 250, 1500, 235) { AllowedCollisionCount = int.MaxValue };
            R = new Spell.Skillshot(SpellSlot.R, 1250, SkillShotType.Linear, 250, 1950, 120) { AllowedCollisionCount = int.MaxValue };
            if (User.ChampionName != "Varus")
            {
                return;
            }
            //Declaring


            //Drawings
            VarusMenu = MainMenu.AddMenu("Mighty Varus", "Mighty Varus");
            comboMenu = VarusMenu.AddSubMenu("Combo");
            comboMenu.Add("Q", new CheckBox("Use Q"));
            comboMenu.Add("E", new CheckBox("Use E"));
            comboMenu.Add("R", new CheckBox("Use R in combo"));
            comboMenu.Add("Rtargets", new Slider("R targets ", 1, 1, 5));
            //miscMenu.Add("Rgapclose", new CheckBox("R Anti-gapcloser"));
            //miscMenu.AddGroupLabel("E");
            //miscMenu.Add("Egapclose", new CheckBox("E Anti-gapcloser"));
            waveClear = VarusMenu.AddSubMenu("Wave Clear");
            waveClear.Add("Qwc", new CheckBox("Use Q"));
            waveClear.Add("Ewc", new CheckBox("Use E"));
            waveClear.Add("manawc", new Slider("Mana manager", 0));
            LastHit = VarusMenu.AddSubMenu("Last hit");
            LastHit.Add("Qlh", new CheckBox("Use Q to Last hit"));
            LastHit.Add("Elh", new CheckBox("Use E to Last hit"));
            LastHit.Add("manalh", new Slider("Mana manager", 0));
            harass = VarusMenu.AddSubMenu("Harass hit");
            harass.Add("QHarass", new CheckBox("Use Q to Harass"));
            harass.Add("QAuto", new CheckBox("Auto Q harass", false));
            harass.Add("EHarass", new CheckBox("Use E to Harass"));
            harass.Add("manaharass", new Slider("Mana manager", 0));
            Drawings = VarusMenu.AddSubMenu("Drawings", "Drawings");
            Drawings.AddGroupLabel("Drawing Settings");
            Drawings.Add("QDraw", new CheckBox("Draw Q Range (green)"));
            Drawings.Add("EDraw", new CheckBox("Draw E Range"));
            Drawings.Add("RDraw", new CheckBox("Draw R Range"));
        }
        private static void Game_OnTick(EventArgs args)
        {
            //if (harass["Qauto"].Cast<CheckBox>().CurrentValue)
            //{
            if (!Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo))
            {
                autoQ();
            }

            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHitFunc();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                wcFunc();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                harassFunc();
            }
        }
        private static void pingkillable()
        {
            foreach (var e in EntityManager.Heroes.Enemies.Where(e => e.Distance(User.Position) < Q.MaximumRange && e.IsVisible && !e.IsDead))
            {
                if (e.Health < Q.GetSpellDamage(e))
                {
                    EloBuddy.TacticalMap.ShowPing(PingCategory.Fallback, e.Position, true);
                }
            }
        }
        private static void autoQ()
        {
            var target = TargetSelector.GetTarget(Q.MaximumRange, DamageType.Physical);
            if (target == null) return;
            foreach (var e in EntityManager.Heroes.Enemies.Where(e => e.Distance(User.Position) < Q.MaximumRange && e.IsVisible && !e.IsDead))
            {
                if (e.Health < Q.GetSpellDamage(e))
                {
                   // pingkillable();
                   // Core.DelayAction(pingkillable, 200);
                   // Core.DelayAction(pingkillable, 500);
                   // Core.DelayAction(pingkillable, 700);
                   // Core.DelayAction(pingkillable, 900);
                    if (Q.IsReady() && e.IsValidTarget(Q.MaximumRange))
                    {
                        if (!Q.IsCharging)
                        {
                            Q.StartCharging();
                        }
                        if (Q.IsCharging)
                        {
                                Q.Cast(target);
                            
                        }
                    }
                }
                if (harass["Qauto"].Cast<CheckBox>().CurrentValue && Player.Instance.ManaPercent > harass["manaharass"].Cast<Slider>().CurrentValue)
                {
                    if (Q.IsReady() && target.IsValidTarget(Q.MaximumRange))
                    {
                        if (!Q.IsCharging)
                        {
                            Q.StartCharging();
                        }
                        if (Q.IsCharging)
                        {
                                Q.Cast(target);
                            
                        }
                    }
                }
            }
        }
        private static void Qtokill()
        {
            foreach (var e in EntityManager.Heroes.Enemies.Where(e => e.Distance(User.Position) < Q.MaximumRange && e.IsVisible && !e.IsDead))
            {
                if (Q.IsInRange(e))
                {
                    var Qdamage = Q.GetSpellDamage(e);
                    var Qhit = Math.Round(e.Health / Qdamage);
                    string Qhittext = "Q to kill : " + Qhit;
                    EloBuddy.Drawing.DrawText((int)e.HPBarPosition.X + 145, (int)e.HPBarPosition.Y + 5, Color.White, Qhittext);
                }
                }
        }
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.MaximumRange, DamageType.Physical);
            if (target == null) return;
            if (comboMenu["R"].Cast<CheckBox>().CurrentValue)
            {
                if (R.IsReady() && target.IsValidTarget(R.Range))
                {
                    var Rcanhit = EntityManager.Heroes.Enemies.Where(x => x.Distance(target) <= 450f).ToList();
                    // Chat.Print("Ennemies for R : " + Rcanhit);
                    if (Rcanhit.Count >= comboMenu["Rtargets"].Cast<Slider>().CurrentValue)
                    {
                        if (!Q.IsCharging) // && R.GetPrediction(target).HitChance >= HitChance.High
                        {
                            R.Cast(target.Position);
                        }
                    }
                }
            }
            if (comboMenu["Q"].Cast<CheckBox>().CurrentValue)
            {
                if (Q.IsReady() && target.IsValidTarget(Q.MaximumRange))
                {
                    if (!Q.IsCharging)
                    {
                        Q.StartCharging();
                    }
                    if (Q.IsCharging)
                    {
                            Q.Cast(target);
                       
                    }
                }
            }
            if (comboMenu["E"].Cast<CheckBox>().CurrentValue)
            {
                if (E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (!Q.IsCharging)
                    {
                        E.Cast(target);
                    }
                }
            }
        }
        private static void LastHitFunc()
        {
            var cstohit = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(a => a.Distance(Player.Instance) <= Q.MaximumRange).OrderBy(a => a.Health).FirstOrDefault();
            if (cstohit != null)
            {
                if (LastHit["Qlh"].Cast<CheckBox>().CurrentValue && Q.IsReady() && Player.Instance.ManaPercent > LastHit["manalh"].Cast<Slider>().CurrentValue && cstohit.IsValidTarget(Q.MaximumRange) && Player.Instance.GetSpellDamage(cstohit, SpellSlot.Q) >= cstohit.TotalShieldHealth())
                {
                    if (!Q.IsCharging)
                    {
                        Q.StartCharging();

                    }
                    if (Q.IsCharging)
                    {
                        Q.Cast(cstohit);
                    }
                }

                if (LastHit["Elh"].Cast<CheckBox>().CurrentValue && E.IsReady() && Player.Instance.GetSpellDamage(cstohit, SpellSlot.E) >= cstohit.TotalShieldHealth() && Player.Instance.ManaPercent > LastHit["manalh"].Cast<Slider>().CurrentValue && !Q.IsCharging)
                {
                    E.Cast(cstohit);
                }
            }
        }
        private static void wcFunc()
        {
            var monsters = EntityManager.MinionsAndMonsters.GetJungleMonsters(User.Position, Q.MaximumRange).OrderByDescending(a => a.MaxHealth).FirstOrDefault();
            var cstohit = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(a => a.Distance(Player.Instance) <= Q.MaximumRange).OrderBy(a => a.Health).FirstOrDefault();
            if (cstohit != null)
            {
                var objAiHero = from x1 in ObjectManager.Get<Obj_AI_Minion>()
                                where x1.IsValidTarget() && x1.IsEnemy
                                select x1 into h
                                orderby h.Distance(User) descending
                                select h into x2
                                where x2.Distance(User) < Q.Range - 20 && !x2.IsDead
                                select x2;
                var aiMinions = objAiHero as Obj_AI_Minion[] ?? objAiHero.ToArray();
                var lastMinion = aiMinions.First();
                if (waveClear["Ewc"].Cast<CheckBox>().CurrentValue && E.IsReady() && cstohit.IsValidTarget(E.Range) && Player.Instance.ManaPercent >= waveClear["manawc"].Cast<Slider>().CurrentValue && !Q.IsCharging) //   && Player.Instance.ManaPercent >= waveClear["manawc"].Cast<Slider>().CurrentValue
                {
                    E.Cast(lastMinion.Position);
                }
                    
                if (waveClear["Qwc"].Cast<CheckBox>().CurrentValue && Q.IsReady() && cstohit.IsValidTarget(Q.MaximumRange) && Player.Instance.ManaPercent >= waveClear["manawc"].Cast<Slider>().CurrentValue) // &&  && Player.Instance.ManaPercent >= waveClear["manawc"].Cast<Slider>().CurrentValue
                {
                    if (!Q.IsCharging)
                    {
                        Q.StartCharging();

                    }
                    if (Q.IsCharging)
                    {
                        Q.Cast(lastMinion.Position);
                    }

                }
                if (monsters != null)
                {
                    if (waveClear["Ewc"].Cast<CheckBox>().CurrentValue && E.IsReady() && monsters.IsValidTarget(E.Range) && Player.Instance.ManaPercent >= waveClear["manawc"].Cast<Slider>().CurrentValue && !Q.IsCharging) //   && Player.Instance.ManaPercent >= waveClear["manawc"].Cast<Slider>().CurrentValue
                    {
                        E.Cast(monsters);
                    }

                    if (waveClear["Qwc"].Cast<CheckBox>().CurrentValue && Q.IsReady() && monsters.IsValidTarget(Q.MaximumRange) && Player.Instance.ManaPercent >= waveClear["manawc"].Cast<Slider>().CurrentValue) // &&  && Player.Instance.ManaPercent >= waveClear["manawc"].Cast<Slider>().CurrentValue
                    {
                        if (!Q.IsCharging)
                        {
                            Q.StartCharging();

                        }
                        if (Q.IsCharging)
                        {
                            Q.Cast(monsters);
                        }

                    }
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            Qtokill();
            if (Drawings["QDraw"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Green, BorderWidth = 1, Radius = Q.MaximumRange }.Draw(User.Position);
            }

            if (Drawings["EDraw"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = E.Range }.Draw(User.Position);
            }
            if (Drawings["RDraw"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = R.Range }.Draw(User.Position);
            }
        }
  
        private static void harassFunc()
        {
            var target = TargetSelector.GetTarget(Q.MaximumRange, DamageType.Physical);
            if (target == null) return;
            if (harass["QHarass"].Cast<CheckBox>().CurrentValue)
            {
                if (Q.IsReady() && target.IsValidTarget(Q.MaximumRange))
                {
                    if (!Q.IsCharging)
                    {
                        Q.StartCharging();
                    }
                    if (Q.IsCharging)
                    {
                            Q.Cast(target);
                      
                    }
                }
            }
            if (harass["EHarass"].Cast<CheckBox>().CurrentValue)
            {
                if (E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (!Q.IsCharging)
                    {
                        E.Cast(target);
                    }
                }
            }
        }
        private static double ComboDamage(Obj_AI_Base target)
        {
            var damage = Player.Instance.GetAutoAttackDamage(target);


            if (Q.IsReady())
            {
                damage += Player.Instance.GetSpellDamage(target, SpellSlot.Q);
            }
            if (E.IsReady())
            {
                damage += Player.Instance.GetSpellDamage(target, SpellSlot.E);
            }

            if (R.IsReady())
            {
                damage += Player.Instance.GetSpellDamage(target, SpellSlot.R);
            }

            return damage;
        }
        private static void Drawing_OnEndScene(EventArgs args)
        {
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(a => !a.IsDead && a.IsHPBarRendered))
                {
                    var damage = ComboDamage(enemy);
                    var damagepercent = (enemy.TotalShieldHealth() - damage > 0 ? enemy.TotalShieldHealth() - damage : 0) /
                                        (enemy.MaxHealth + enemy.AllShield + enemy.AttackShield + enemy.MagicShield);
                    var hppercent = enemy.TotalShieldHealth() /
                                    (enemy.MaxHealth + enemy.AllShield + enemy.AttackShield + enemy.MagicShield);
                    var start = new Vector2((int)(enemy.HPBarPosition.X + Offset.X + damagepercent * 104),
                        (int)(enemy.HPBarPosition.Y + Offset.Y) - 5);
                    var end = new Vector2((int)(enemy.HPBarPosition.X + Offset.X + hppercent * 104) + 2,
                        (int)(enemy.HPBarPosition.Y + Offset.Y) - 5);

                    Drawing.DrawLine(start, end, 9, Color.Chartreuse);
                }
            
        }
    }
}
