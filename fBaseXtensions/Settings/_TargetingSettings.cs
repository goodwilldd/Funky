﻿namespace fBaseXtensions.Settings
{
	public class SettingTargeting
	{
		 public bool IgnoreAboveAverageMobs { get; set; }
		 public bool IgnoreCorpses { get; set; }
		 public bool IgnoreWeaponRacks { get; set; }
		 public bool IgnoreArmorRacks { get; set; }
		 public bool IgnoreFloorContainers { get; set; }
		 public bool IgnoreNormalChests { get; set; }
		 public bool IgnoreRareChests { get; set; }

		 public bool MissleDampeningEnforceCloseRange { get; set; }
		 public int GoblinPriority { get; set; }
		 public bool[] UseShrineTypes { get; set; }
		 public bool UseExtendedRangeRepChest { get; set; }
		 public bool PrioritizeCloseRangeUnits { get; set; }
		 public int PrioritizeCloseRangeMinimumUnits { get; set; }

		 public SettingTargeting()
		 {
			  GoblinPriority=2;
			  UseShrineTypes=new [] { true, true, true, true, true, true };
			  IgnoreAboveAverageMobs=false;
			  PrioritizeCloseRangeUnits=true;
			  PrioritizeCloseRangeMinimumUnits=3;
			  IgnoreCorpses=true;
			  UseExtendedRangeRepChest=false;
			  MissleDampeningEnforceCloseRange=true;
			  IgnoreWeaponRacks = false;
			  IgnoreArmorRacks = false;
			  IgnoreFloorContainers = false;
			  IgnoreNormalChests = false;
			  IgnoreRareChests = false;
		 }

	}
}
