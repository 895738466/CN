#region LICENSE

// Copyright 2014 - 2014 Support
// Leona.cs is part of Support.
// Support is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// Support is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with Support. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Support.Plugins
{
    public class Leona : PluginBase
    {
        public Leona()
        {
            Q = new Spell(SpellSlot.Q, AttackRange);
            W = new Spell(SpellSlot.W, AttackRange);
            E = new Spell(SpellSlot.E, 875);
            R = new Spell(SpellSlot.R, 1200);

            E.SetSkillshot(0.25f, 100f, 2000f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1f, 300f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.CastCheck(Target))
                {
                    Orbwalking.ResetAutoAttackTimer();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                }

                if (W.CastCheck(Target, "ComboQWE"))
                {
                    W.Cast();
                }

                if (E.CastCheck(Target, "ComboQWE") && Q.IsReady())
                {
                    // Max Range with VeryHigh Hitchance / Immobile
                    if (E.GetPrediction(Target).Hitchance >= HitChance.VeryHigh)
                    {
                        if (E.Cast(Target, UsePackets) == Spell.CastStates.SuccessfullyCasted)
                            W.Cast();
                    }

                    // Lower Range
                    if (E.GetPrediction(Target, false, 775).Hitchance >= HitChance.High)
                    {
                        if (E.Cast(Target, UsePackets) == Spell.CastStates.SuccessfullyCasted)
                            W.Cast();
                    }
                }

                if (E.CastCheck(Target, "ComboE"))
                {
                    E.Cast(Target);
                }

                if (R.CastCheck(Target, "ComboR"))
                {
                    R.CastIfHitchanceEquals(Target, HitChance.Immobile, UsePackets);
                }
            }
        }

        public override void OnAfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (!unit.IsMe)
                return;

            if (!(target is Obj_AI_Hero) && !target.Name.ToLower().Contains("ward"))
                return;

            if (!Q.IsReady())
                return;

            if (Q.Cast())
            {
                Orbwalking.ResetAutoAttackTimer();
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
                return;

            if (Q.CastCheck(gapcloser.Sender, "GapcloserQ"))
            {
                if (Q.Cast())
                {
                    Orbwalking.ResetAutoAttackTimer();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, gapcloser.Sender);
                }
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (spell.DangerLevel < InterruptableDangerLevel.High || unit.IsAlly)
                return;

            if (Q.CastCheck(unit, "InterruptQ"))
            {
                if (Q.Cast())
                {
                    Orbwalking.ResetAutoAttackTimer();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, unit);
                }

                return;
            }

            if (R.CastCheck(unit, "InterruptR"))
            {
                R.Cast(unit, UsePackets);
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboE", "使用 E 没有 Q", false);
            config.AddBool("ComboQWE", "使用 Q/W/E", true);
            config.AddBool("ComboR", "使用 R", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserQ", "使用 Q 防突进ㄧ", true);

            config.AddBool("InterruptQ", "使用 Q 打断", true);
            config.AddBool("InterruptR", "使用 R 打断", true);
        }
    }
}