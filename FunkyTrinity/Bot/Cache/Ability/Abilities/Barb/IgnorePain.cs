﻿using System;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
namespace FunkyTrinity.ability.Abilities.Barb
{
	public class IgnorePain : Ability, IAbility
	{
		public IgnorePain() : base()
		{
		}

		public override SNOPower Power
		{
			get { return SNOPower.Barbarian_IgnorePain; }
		}

		public override int RuneIndex { get { return Bot.Class.RuneIndexCache.ContainsKey(this.Power)?Bot.Class.RuneIndexCache[this.Power]:-1; } }

		public override void Initialize()
		{
			ExecutionType = AbilityUseType.Buff;
			WaitVars = new WaitLoops(0, 0, true);
			Cost = 0;
			UseageType=AbilityUseage.Anywhere;
			IsSpecialAbility = true;
			Priority = AbilityPriority.High;
			PreCastConditions = (AbilityConditions.CheckRecastTimer | AbilityConditions.CheckCanCast);

			Fcriteria = new Func<bool>(() => { return Bot.Character.dCurrentHealthPct <= 0.45; });
		}
		public override void InitCriteria()
		{
			 base.AbilityTestConditions=new AbilityUsablityTests(this);
		}
		#region IAbility
		public override int GetHashCode()
		{
			 return (int)this.Power;
		}
		public override bool Equals(object obj)
		{
			 //Check for null and compare run-time types. 
			 if (obj==null||this.GetType()!=obj.GetType())
			 {
				  return false;
			 }
			 else
			 {
				  Ability p=(Ability)obj;
				  return this.Power==p.Power;
			 }
		}

		
		#endregion
	}
}
