﻿using fBaseXtensions.Game.Hero.Skills.Conditions;
using Zeta.Game.Internals.Actors;

namespace fBaseXtensions.Game.Hero.Skills.SkillObjects
{
	public class WeaponInstantRanged : Skill
	{
		public override SNOPower Power
		{
			get { return SNOPower.Weapon_Ranged_Instant; }
		}
		public override double Cooldown { get { return 5; } }

		public override bool IsRanged { get { return true; } }
		public override bool IsProjectile { get { return true; } }

		public override SkillUseage UseageType { get { return SkillUseage.Combat; } }

		public override SkillExecutionFlags ExecutionType { get { return SkillExecutionFlags.Target; } }

		public override void Initialize()
		{
			Range = 25;
			Priority = SkillPriority.None;
		

			WaitVars = new WaitLoops(0, 0, true);
			PreCast = new SkillPreCast(SkillPrecastFlags.None);
			
		}
	}
}
