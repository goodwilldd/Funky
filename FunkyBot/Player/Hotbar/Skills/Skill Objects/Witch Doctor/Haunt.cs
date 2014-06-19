﻿using System;
using FunkyBot.Player.HotBar.Skills.Conditions;
using Zeta.Game.Internals.Actors;

namespace FunkyBot.Player.HotBar.Skills.WitchDoctor
{
	public class Haunt : Skill
	{
		public override void Initialize()
		{
			bool hotbarContainsLoctusSwarm = Bot.Character.Class.HotBar.HotbarPowers.Contains(SNOPower.Witchdoctor_Locust_Swarm);

			Cooldown = 2000;
			ExecutionType = SkillExecutionFlags.Target;

			//since we can only track one DOTDPS, we track locus swarm and cast this 
			if (hotbarContainsLoctusSwarm)
			{
				SingleUnitCondition.Add(new UnitTargetConditions(TargetProperties.None, maxdistance: 25));
				SingleUnitCondition.Add(new UnitTargetConditions(TargetProperties.None, maxdistance: -1, MinimumHealthPercent: 0.95d, falseConditionalFlags: TargetProperties.Normal));
			}
			else
			{
				SingleUnitCondition.Add(new UnitTargetConditions(TargetProperties.None, maxdistance: 25, falseConditionalFlags: TargetProperties.DOTDPS));
				SingleUnitCondition.Add(new UnitTargetConditions(TargetProperties.None, maxdistance: -1, MinimumHealthPercent: 0.95d, falseConditionalFlags: TargetProperties.Normal|TargetProperties.DOTDPS));
			}

			WaitVars = new WaitLoops(0, 0, false);
			Cost = 50;
			Range = 45;
			IsRanged = true;
			IsProjectile=true;
			UseageType = SkillUseage.Combat;
			Priority = SkillPriority.High;
			ShouldTrack = true;

			var precastflags = SkillPrecastFlags.CheckPlayerIncapacitated | SkillPrecastFlags.CheckCanCast;
			if (hotbarContainsLoctusSwarm) precastflags |= SkillPrecastFlags.CheckRecastTimer;

			PreCast = new SkillPreCast(precastflags);

			PreCast.Criteria += (s) => !Bot.Character.Class.HotBar.HasDebuff(SNOPower.Succubus_BloodStar);
			
			FcriteriaCombat = () =>
			{
				if (Bot.Targeting.Cache.CurrentTarget.SkillsUsedOnObject.ContainsKey(Power))
				{
					//If we have Creeping Death, then we ignore any units that we already cast upon.
					if (Bot.Character.Class.HotBar.PassivePowers.Contains(SNOPower.Witchdoctor_Passive_CreepingDeath)) return false;

					return DateTime.Now.Subtract(Bot.Targeting.Cache.CurrentTarget.SkillsUsedOnObject[Power]).TotalSeconds > 11;
				}

				return true;
			};
		}

		#region IAbility

		public override int RuneIndex
		{
			get { return Bot.Character.Class.HotBar.RuneIndexCache.ContainsKey(Power) ? Bot.Character.Class.HotBar.RuneIndexCache[Power] : -1; }
		}

		public override int GetHashCode()
		{
			return (int)Power;
		}

		public override bool Equals(object obj)
		{
			//Check for null and compare run-time types. 
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			Skill p = (Skill)obj;
			return Power == p.Power;
		}

		#endregion

		public override SNOPower Power
		{
			get { return SNOPower.Witchdoctor_Haunt; }
		}
	}
}
