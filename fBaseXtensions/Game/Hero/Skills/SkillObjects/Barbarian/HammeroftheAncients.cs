﻿using fBaseXtensions.Game.Hero.Skills.Conditions;
using Zeta.Game.Internals.Actors;

namespace fBaseXtensions.Game.Hero.Skills.SkillObjects.Barbarian
{
	public class HammeroftheAncients : Skill
	{
		public override SNOPower Power { get { return SNOPower.Barbarian_HammerOfTheAncients; } }


		public override double Cooldown { get { return 0; } }


		public override WaitLoops WaitVars { get { return WaitLoops.Default; } }

		public override SkillExecutionFlags ExecutionType { get { return SkillExecutionFlags.ClusterTarget | SkillExecutionFlags.Target; } }

		public override SkillUseage UseageType { get { return SkillUseage.Combat; } }

		public override void Initialize()
		{
			Range = RuneIndex == 0 ? 13 : RuneIndex == 1 ? 20 : 16;
			Cost = 20;
			Priority = SkillPriority.Medium;
			PreCast = new SkillPreCast(SkillPrecastFlags.CheckCanCast | SkillPrecastFlags.CheckPlayerIncapacitated);
			ClusterConditions.Add(new SkillClusterConditions(6d, 30f, 3, true));
			SingleUnitCondition.Add(new UnitTargetConditions(TargetProperties.None, maxdistance: Range, MinimumHealthPercent: 0.95d, falseConditionalFlags: TargetProperties.Normal));
			SingleUnitCondition.Add(new UnitTargetConditions
			{
				Criteria = () => FunkyGame.Hero.dCurrentEnergyPct > 0.80d,
				MaximumDistance = Range,
				FalseConditionFlags = TargetProperties.LowHealth,
			});
			
			FcriteriaCombat = () => !FunkyGame.Hero.Class.bWaitingForSpecial;
		}

	}
}
