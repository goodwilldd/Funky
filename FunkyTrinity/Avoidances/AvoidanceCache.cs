﻿using System;
using System.Linq;
using Zeta;
using System.Collections.Generic;
using FunkyTrinity.Cache;
using Zeta.Internals.Actors;

namespace FunkyTrinity.Avoidances
{
    public static class AvoidanceCache
    {
		  internal static readonly  AvoidanceValue[] AvoidancesDefault=new AvoidanceValue[]
			  {
				  new AvoidanceValue(AvoidanceType.ArcaneSentry, 1, 14), 
				  new AvoidanceValue(AvoidanceType.AzmodanBodies, 1, 47),
				  new AvoidanceValue(AvoidanceType.AzmodanFireball, 1, 16),
				  new AvoidanceValue(AvoidanceType.AzmodanPool, 1, 54),
				  new AvoidanceValue(AvoidanceType.BeeProjectile, 0.5, 2), 
				  new AvoidanceValue(AvoidanceType.BelialGround, 1, 25),
				  new AvoidanceValue(AvoidanceType.Dececrator, 1, 9),
				  new AvoidanceValue(AvoidanceType.DiabloMetor, 0.80, 28),
				  new AvoidanceValue(AvoidanceType.DiabloPrison, 1, 15),
				  new AvoidanceValue(AvoidanceType.Frozen, 1, 19), 
				  new AvoidanceValue(AvoidanceType.GrotesqueExplosion, 0.50, 20),
				  new AvoidanceValue(AvoidanceType.LacuniBomb, 0.25, 2),
				  new AvoidanceValue(AvoidanceType.MageFirePool, 1, 10), 
				  new AvoidanceValue(AvoidanceType.MoltenCore, 1, 20), 
				  new AvoidanceValue(AvoidanceType.MoltenTrail, 0.75, 6),
				  new AvoidanceValue(AvoidanceType.PlagueCloud, 0.75, 19),
				  new AvoidanceValue(AvoidanceType.PlagueHand, 1, 15),
				  new AvoidanceValue(AvoidanceType.PoisonGas, 0.5, 9), 
				  new AvoidanceValue(AvoidanceType.ShamanFireBall, 0.1, 2), 
				  new AvoidanceValue(AvoidanceType.SuccubusProjectile, 0.25, 2),
				  new AvoidanceValue(AvoidanceType.TreeSpore, 1, 13),
				  //?? value never makes it when deseralized, but is seralized.
				  new AvoidanceValue(AvoidanceType.None,0,0),
			  };

		  // A list of all the SNO's to avoid - you could add 
		  public static readonly HashSet<int> hashAvoidanceSNOList=new HashSet<int>
		  {
				  // Arcane        Arcane 2      Desecrator   Poison Tree    Molten Core   Molten Trail   Plague Cloud   Ice Balls     
				  219702,          221225,       84608,       5482,6578,     4803, 4804,   95868,         108869,        402, 223675,             
				  // Bees-Wasps    Plague-Hands  Azmo Pools   Azmo fireball  Azmo bodies   Belial 1       Belial 2      
				  5212,            3865,         123124,      123842,        123839,       161822,        161833, 
				  // Sha-Ball      Mol Ball      Mage Fire    Diablo Prison  Diablo Meteor Ice-trail      PoisonGas
				  4103,            160154,       432,         168031,        214845,       260377,        4176,
				  //lacuni bomb		Succubus Bloodstar
				  4546,			   164829, 
			  };

		  // A list of SNO's that are projectiles (so constantly look for new locations while avoiding)
		  public static readonly HashSet<int> hashAvoidanceSNOProjectiles=new HashSet<int>
		  {
				  // Bees-Wasps  Sha-Ball   Mol Ball   Azmo fireball
				  5212,          4103,      160154,    123842,		164829, 
			  };

		  internal static AvoidanceType FindAvoidanceUsingName(string Name)
		  {
				Name=Name.ToLower();
				if (Name.StartsWith("monsteraffix_"))
				{
					 if (Name.Contains("dececrator")) return AvoidanceType.Dececrator;
					 if (Name.Contains("frozen")) return AvoidanceType.Frozen;
					 if (Name.Contains("molten"))
					 {
						  if (Name.Contains("trail")) return AvoidanceType.MoltenTrail; else return AvoidanceType.MoltenCore;
					 }
					 if (Name.Contains("plagued")) return AvoidanceType.PlagueCloud;
					 if (Name.Contains("wall")) return AvoidanceType.Wall;
				}
				else if (Name.Contains("azmodan")||Name.Contains("belial")||Name.Contains("diablo"))
				{
					 //Bosses
					 if (Name.StartsWith("belial_armslam_projectile")) return AvoidanceType.BelialGround;
					 if (Name.StartsWith("belial_groundprojectile")) return AvoidanceType.BelialGround;
				}
				else
				{
					 if (Name.StartsWith("skeletonmage_fire_groundpool")) return AvoidanceType.MageFirePool;
					 if (Name.StartsWith("fallenshaman_fireball_projectile")) return AvoidanceType.ShamanFireBall;
					 if (Name.StartsWith("woodwraith_sporecloud_emitter")) return AvoidanceType.TreeSpore;
				}

				return AvoidanceType.None;
		  }

		  internal static AvoidanceType FindAvoidanceUsingSNOID(int SNOID)
		  {
				switch (SNOID)
				{
					 case 219702:
					 case 221225:
						  return AvoidanceType.ArcaneSentry;
					 case 84608:
						  return AvoidanceType.Dececrator;
					 case 5482:
					 case 6578:
						  return AvoidanceType.TreeSpore;
					 case 4803:
					 case 4804:
						  return AvoidanceType.MoltenCore;
					 case 95868:
						  return AvoidanceType.MoltenTrail;
					 case 108869:
						  return AvoidanceType.PlagueCloud;
					 case 402:
					 case 223675:
						  return AvoidanceType.Frozen;
					 case 5212:
						  return AvoidanceType.BeeProjectile;
					 case 3865:
						  return AvoidanceType.PlagueHand;
					 case 123124:
						  return AvoidanceType.AzmodanPool;
					 case 123842:
						  return AvoidanceType.AzmodanFireball;
					 case 123839:
						  return AvoidanceType.AzmodanBodies;
					 case 161822:
					 case 161833:
					 case 60108:
						  return AvoidanceType.BelialGround;
					 case 168031:
						  return AvoidanceType.DiabloPrison;
					 case 214845:
						  return AvoidanceType.DiabloMetor;
					 case 432:
						  return AvoidanceType.MageFirePool;
					 case 4546:
						  return AvoidanceType.LacuniBomb;
					 case 4176:
						  return AvoidanceType.PoisonGas;
					 case 164829:
						  return AvoidanceType.SuccubusProjectile;
				}
				return AvoidanceType.None;
		  }

	    internal static bool IgnoringAvoidanceType(AvoidanceType thisAvoidance)
	    {
		    if (!Bot.SettingsFunky.Avoidance.AttemptAvoidanceMovements)
			    return true;

		    double dThisHealthAvoid=Bot.SettingsFunky.Avoidance.Avoidances[(int)thisAvoidance].Health;
		    if (dThisHealthAvoid==0d)
			    return true;

		    return false;
	    }

	    ///<summary>
	    ///Tests the given avoidance type to see if it should be ignored either due to a buff or if health is greater than the avoidance HP.
	    ///</summary>
	    internal static bool IgnoreAvoidance(AvoidanceType thisAvoidance)
	    {
		    double dThisHealthAvoid=Bot.SettingsFunky.Avoidance.Avoidances[(int)thisAvoidance].Health;

		    if (!Bot.Combat.CriticalAvoidance)
		    {//Not Critical Avoidance, should we be in total ignorance because of a buff?

			    // Monks with Serenity up ignore all AOE's
			    if (Bot.Class.AC==ActorClass.Monk&&Bot.Class.HotbarPowers.Contains(SNOPower.Monk_Serenity)&&Bot.Class.HasBuff(SNOPower.Monk_Serenity))
			    {
				    // Monks with serenity are immune
				    return true;

			    }// Witch doctors with spirit walk available and not currently Spirit Walking will subtly ignore ice balls, arcane, desecrator & plague cloud
			    else if (Bot.Class.AC==ActorClass.WitchDoctor
			             &&Bot.Class.HotbarPowers.Contains(SNOPower.Witchdoctor_SpiritWalk)
							 &&(!Bot.Class.HasBuff(SNOPower.Witchdoctor_SpiritWalk)&&Bot.Class.Abilities[SNOPower.Witchdoctor_SpiritWalk].AbilityUseTimer())||Bot.Class.HasBuff(SNOPower.Witchdoctor_SpiritWalk))
			    {
				    switch (thisAvoidance)
				    {
					    case AvoidanceType.Frozen:
					    case AvoidanceType.ArcaneSentry:
					    case AvoidanceType.Dececrator:
					    case AvoidanceType.PlagueCloud:
						    return true;
				    }
			    }
			    else if (Bot.Class.AC==ActorClass.Barbarian&&Bot.Class.HotbarPowers.Contains(SNOPower.Barbarian_WrathOfTheBerserker)&&Bot.Class.HasBuff(SNOPower.Barbarian_WrathOfTheBerserker))
			    {
				    switch (thisAvoidance)
				    {
					    case AvoidanceType.Frozen:
					    case AvoidanceType.ArcaneSentry:
					    case AvoidanceType.Dececrator:
					    case AvoidanceType.PlagueCloud:
						    return true;
				    }
			    }
		    }

		    //Only procedee if health percent is necessary for avoidance!
		    return dThisHealthAvoid<Bot.Character.dCurrentHealthPct;
	    }
    }
}