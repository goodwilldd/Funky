﻿using System;
using System.Linq;
using Zeta;
using Zeta.Common;
using Zeta.Internals.Actors;
using System.Collections.Generic;
using Zeta.Internals.SNO;
using Zeta.CommonBot;

namespace FunkyTrinity
{
	 public partial class Funky
	 {
		  private static List<int> ValidClusterUnits=new List<int>();

		  public partial class dbRefresh
		  {
				

				public static int RefreshRateMilliseconds=150;
				public static bool ShouldRefreshObjectList
				{
					 get
					 {
						  return DateTime.Now.Subtract(lastRefreshedObjects).TotalMilliseconds>=RefreshRateMilliseconds;
					 }
				}

				///<summary>
				///Tracks the current Level ID
				///</summary>
				private static int LastLevelID=-1;
				///<summary>
				///Used to flag when Init should iterate and remove the objects
				///</summary>
				internal static bool RemovalCheck=false;
				public int AvoidanceRecheckMaximumRate { get; set; }//=1250;
				public int AvoidanceRecheckMinimumRate { get; set; }//=500;
				public int KitingRecheckMaximumRate { get; set; }//=4000;
				public int KitingRecheckMinimumRate { get; set; }//=2000;
				private static void UpdateAvoidKiteRates()
				{
					 double extraWaitTime=SettingsFunky.AvoidanceRecheckMaximumRate*Bot.Character.dCurrentHealthPct;
					 if (extraWaitTime<SettingsFunky.AvoidanceRecheckMinimumRate) extraWaitTime=SettingsFunky.AvoidanceRecheckMinimumRate;
					 Bot.Combat.iMillisecondsCancelledEmergencyMoveFor=(int)extraWaitTime;

					 extraWaitTime=SettingsFunky.KitingRecheckMaximumRate*Bot.Character.dCurrentHealthPct;
					 if (extraWaitTime<SettingsFunky.KitingRecheckMinimumRate) extraWaitTime=SettingsFunky.KitingRecheckMinimumRate;
					 Bot.Combat.iMillisecondsCancelledKiteMoveFor=(int)extraWaitTime;
				}

				///<summary>
				///Resets/Updates cache and misc vars
				///</summary>
				private static void InitObjectRefresh()
				{
					 //Cache last target only if current target is not avoidance (Movement).
					 if (!Bot.Target.Equals(null)&&Bot.Target.ObjectData.targetType.HasValue&&Bot.Target.ObjectData.targetType.Value!=TargetType.Avoidance)
						  Bot.Character.LastCachedTarget=Bot.Target.ObjectData!=null?Bot.Target.ObjectData.Clone():FakeCacheObject;

					 if (!Bot.Target.Equals(null)&&Bot.Target.ObjectData.targetType.HasValue&&Bot.Target.ObjectData.targetType.Value==TargetType.Avoidance
						  &&!String.IsNullOrEmpty(Bot.Target.ObjectData.InternalName)&&Bot.Target.ObjectData.InternalName.Contains("Kitespot"))
					 {
						  Bot.Combat.LastKiteAction=DateTime.Now;
						  Bot.Combat.KitedLastTarget=true;
					 }
					 else
						  Bot.Combat.KitedLastTarget=false;


					 Bot.Target.ObjectData=null;
					 thisFakeObject=null;

					 #region KillLootRangeChecks
					 iCurrentMaxKillRadius=Zeta.CommonBot.Settings.CharacterSettings.Instance.KillRadius;
					 iCurrentMaxLootRadius=Zeta.CommonBot.Settings.CharacterSettings.Instance.LootRadius;
					 // Not allowed to kill monsters due to profile/routine/combat targeting settings - just set the kill range to a third
					 if (!ProfileManager.CurrentProfile.KillMonsters)
					 {
						  iCurrentMaxKillRadius/=3;
					 }
					 // Always have a minimum kill radius, so we're never getting whacked without retaliating
					 if (iCurrentMaxKillRadius<10||SettingsFunky.IgnoreCombatRange)
						  iCurrentMaxKillRadius=10;

					 if (shouldPreformOOCItemIDing||FunkyTPBehaviorFlag||bWantToTownRun)
						  iCurrentMaxKillRadius=50;

					 // Not allowed to loots due to profile/routine/loot targeting settings - just set range to a quarter
					 if (!ProfileManager.CurrentProfile.PickupLoot)
					 {
						  iCurrentMaxLootRadius/=4;
					 }

					 //Ignore Loot Range Setting
					 if (SettingsFunky.IgnoreLootRange)
						  iCurrentMaxLootRadius=10;


					 // Counter for how many cycles we extend or reduce our attack/kill radius, and our loot radius, after a last kill
					 if (iKeepKillRadiusExtendedFor>0)
						  iKeepKillRadiusExtendedFor--;
					 if (iKeepLootRadiusExtendedFor>0)
						  iKeepLootRadiusExtendedFor--;
					 #endregion
					 // Refresh buffs (so we can check for wrath being up to ignore ice balls and anything else like that)
					 Bot.Class.RefreshCurrentBuffs();

					 // Clear forcing close-range priority on mobs after XX period of time
					 if (Bot.Combat.bForceCloseRangeTarget&&DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds>iMillisecondsForceCloseRange)
					 {
						  Bot.Combat.bForceCloseRangeTarget=false;
					 }

					 // Bunch of variables used throughout
					 #region VariableReset
					 bCheckGround=false;
					 // Reset the counters for player-owned things
					 Bot.Character.PetData.Reset();
					 // Reset the counters for monsters at various ranges
					 Bot.Combat.Reset();
					 #endregion

					 //clear GPCache (Only melee uses this)
					 if (Bot.Class.IsMeleeClass)
						  GridPointAreaCache.CheckClearedSearchRefresh();


					 //Check if we should trim our SNO cache..
					 if (DateTime.Now.Subtract(ObjectCache.cacheSnoCollection.lastTrimming).TotalMinutes>3)
						  ObjectCache.cacheSnoCollection.TrimOldUnusedEntries();


					 //Check Level ID changes and clear cache objects.
					 if (ZetaDia.CurrentLevelAreaId!=LastLevelID&&
								(!ZetaDia.Me.IsInTown))
					 {
						  LastLevelID=ZetaDia.CurrentLevelAreaId;

						  //Clear our current collection since we changed levels.
						  ObjectCache.Objects.Clear();
						  RemovalCheck=false;

						  //Reset Playermover Backtrack Positions
						  GridPointAreaCache.cacheMovementGPRs.Clear();

						  //This is the only time we should call this. MGP only needs updated every level change!
						  UpdateSearchGridProvider(true);
					 }



					 //Check Cached Object Removal flag
					 if (RemovalCheck)
					 {
						  //Remove flagged objects
						  List<int> RemovalObjs=(from objs in ObjectCache.Objects.Values
														 where objs.NeedsRemoved
														 select objs.RAGUID).ToList();

						  if (RemovalObjs.Count>0)
						  {
								List<int> removalList=RemovalObjs.ToList();

								for (int i=0; i<removalList.Count; i++)
								{
									 CacheObject thisObj=ObjectCache.Objects[removalList[i]];

									 //Blacklist..?
									 if (thisObj.BlacklistFlag!=BlacklistType.None)
										  AddObjectToBlacklist(thisObj.RAGUID, thisObj.BlacklistFlag);

									 //Special Tresure goblin check
									 /*
									 if (thisObj.IsTreasureGoblin&&
										  thisObj.CurrentHealthPct.Value==1d&&
										  settings.iTreasureGoblinPriority>=2&&
										  thisObj.CentreDistance<200f&&
										  thisObj.IsStillValid()&&thisObj.ref_DiaObject.CurrentScene.Visibility!=Zeta.Internals.SceneVisibility.Hidden) //Verify we can stand
									 {
										  //We are removing a goblin that is not "dead"..
										  GoblinRewind.ShouldGoblinRewind=true;
										  GoblinRewind.Goblin=thisObj.Clone();
									 }
									 */
									 ObjectCache.Objects.Remove(thisObj.RAGUID);
								}
						  }

						  RemovalCheck=false;
					 }

					 //Increase counter, clear entries if overdue.
					 ObjectCache.Obstacles.AttemptToClearEntries();

					 //Non-Combat behavior we reset temp blacklist so we don't get killed by "ignored" units..
					 if (Bot.Combat.IsInNonCombatBehavior&&DateTime.Now.Subtract(dateSinceTemporaryBlacklistClear).TotalSeconds>10)
					 {
						  dateSinceTemporaryBlacklistClear=DateTime.Now;
						  hashRGUIDTemporaryIgnoreBlacklist=new HashSet<int>();
					 }
				}
				///<summary>
				///Refreshes Cache and updates current target
				///</summary>
				public static void RefreshDiaObjects()
				{
					 //Reset key targeting vars always!
					 InitObjectRefresh();

					 //Update Character (just incase it wasnt called before..)
					 Bot.Character.Update(false, true);

					 //Update object cache collection
					 UpdateCacheObjectCollection();

					 //Generate a vaild object list using our cached collection!
					 List<CacheObject> listObjectCache=ReturnUsableList();

					 //Cluster Target Logic
					 if (SettingsFunky.EnableClusteringTargetLogic
						  &&(!SettingsFunky.IgnoreClusteringWhenLowHP||Bot.Character.dCurrentHealthPct>SettingsFunky.IgnoreClusterLowHPValue)
						  &&DateTime.Now.Subtract(Bot.Combat.dateSincePickedTarget).TotalMilliseconds>150)
					 {
						  ValidClusterUnits=new List<int>();

						  if (Bot.Combat.UnitRAGUIDs.Count>=SettingsFunky.ClusterMinimumUnitCount)
						  {
								List<CacheObject> listObjectUnits=listObjectCache.Where(u => Bot.Combat.UnitRAGUIDs.Contains(u.RAGUID)).ToList();

								if (listObjectUnits.Count>0)
								{
									 List<Cluster> surroundingClusters=RunKmeans(listObjectUnits, SettingsFunky.ClusterDistance);
									 foreach (var item in surroundingClusters)
									 {
										  if (item.ListUnits.Count>=SettingsFunky.ClusterMinimumUnitCount)
										  {
												ValidClusterUnits.AddRange((from units in item.ListUnits
																					 select units.RAGUID).ToArray());
												//Logging.WriteVerbose("Cluster has total units {0} with accumulated weight {1}", item.ListUnits.Count.ToString(), item.AccumulatedWeight.ToString());
										  }
									 }
								}
						  }
					 }

					 // If we have an avoidance under our feet, then create a new object which contains a safety point to move to
					 // But only if we aren't force-cancelling avoidance for XX time
					 bool bFoundSafeSpot=false;

					 //Check avoidance requirement still valid
					 if (Bot.Combat.RequiresAvoidance&&Bot.Combat.TriggeringAvoidances.Count==0)
						  Bot.Combat.RequiresAvoidance=false;

					 //Find any units that we should kite, sorted by distance.
					 var nearbyUnits=ObjectCache.Objects.Values.OfType<CacheUnit>().Where(unit => unit.ShouldBeKited
																					 &&unit.RadiusDistance<=Bot.Class.KiteDistance)
																					 .OrderBy(unit => unit.Weight).ThenBy(unit => unit.CentreDistance); ;

					 Vector3 LOS=vNullLocation;

					 #region AvoidanceMovement
					 // Note that if treasure goblin level is set to kamikaze, even avoidance moves are disabled to reach the goblin!
					 if (Bot.Combat.RequiresAvoidance&&(!Bot.Combat.bAnyTreasureGoblinsPresent||SettingsFunky.GoblinPriority<2)&&
						 DateTime.Now.Subtract(Bot.Combat.timeCancelledEmergencyMove).TotalMilliseconds>=Bot.Combat.iMillisecondsCancelledEmergencyMoveFor)
					 {//Bot requires avoidance movement.. (Note: we have not done the weighting of our targeting list yet..)
						  Vector3 vAnySafePoint;

						  bool SafeMovementFound=false;

						  if (!Bot.Target.Equals(null)&&Bot.Target.ObjectData.targetType.HasValue&&TargetType.ServerObjects.HasFlag(Bot.Target.ObjectData.targetType.Value))
								LOS=Bot.Target.ObjectData.Position;
						  else
								LOS=vNullLocation;


						  SafeMovementFound=GridPointAreaCache.AttemptFindSafeSpot(out vAnySafePoint, LOS);




						  if (SafeMovementFound)
						  {
								float distance=vAnySafePoint.Distance(Bot.Character.Position);

								float losdistance=0f;
								if (LOS!=vNullLocation)
									 losdistance=LOS.Distance(Bot.Character.Position);

								string los=LOS!=vNullLocation?("\r\n using LOS location "+LOS.ToString()+" distance "+losdistance.ToString()):" ";

								Logging.WriteDiagnostic("Safespot found at {0} with {1} distance{2}", vAnySafePoint.ToString(), distance, los);
								bFoundSafeSpot=true;

								//setup avoidance target
								if (!Bot.Target.Equals(null))
									 Bot.Combat.LastCachedTarget=Bot.Target.ObjectData.Clone();

								Bot.Target.ObjectData=new CacheObject(vAnySafePoint, TargetType.Avoidance, 20000f, "SafeAvoid", 2.5f, -1);
								Bot.Combat.timeCancelledEmergencyMove=DateTime.Now;
								Bot.Combat.LastAvoidanceMovement=DateTime.Now;
						  }

						  UpdateAvoidKiteRates();
					 }
					 #endregion

					 // Special flag for special whirlwind circumstances
					 Bot.Combat.bAnyNonWWIgnoreMobsInRange=false;

					 // Now give each object a weight *IF* we aren't skipping direcly to a safe-spot
					 if (!bFoundSafeSpot)
					 {
						  Bot.Combat.bStayPutDuringAvoidance=false;
						  //Weight the valid object list
						  WeightEvaluationObjList(listObjectCache);



						  #region Kiting
						  if (Bot.Class.KiteDistance>0)
						  {
								//Is our new target a unit and do we need to attempt kiting?
								if (!Bot.Target.Equals(null)
									 &&Bot.Target.ObjectData.targetType.HasValue&&Bot.Target.ObjectData.targetType==TargetType.Unit
									 &&Bot.Class.KiteDistance>0
									 &&(Bot.Class.AC!=ActorClass.Wizard||(Bot.Class.AC==ActorClass.Wizard&&(!SettingsFunky.Class.bKiteOnlyArchon||HasBuff(SNOPower.Wizard_Archon)))))
								{


									 if (nearbyUnits.Any())
									 {
										  // Note that if treasure goblin level is set to kamikaze, even avoidance moves are disabled to reach the goblin!
										  if ((!Bot.Combat.bAnyTreasureGoblinsPresent||SettingsFunky.GoblinPriority<2)&&
											  DateTime.Now.Subtract(Bot.Combat.timeCancelledKiteMove).TotalMilliseconds>=Bot.Combat.iMillisecondsCancelledKiteMoveFor)
										  {
												Bot.Combat.timeCancelledEmergencyMove=DateTime.Now;

												if (!Bot.Target.Equals(null)&&Bot.Target.ObjectData.targetType.HasValue&&TargetType.ServerObjects.HasFlag(Bot.Target.ObjectData.targetType.Value))
													 LOS=Bot.Target.ObjectData.Position;
												else
													 LOS=vNullLocation;

												Vector3 vAnySafePoint;
												if (GridPointAreaCache.AttemptFindSafeSpot(out vAnySafePoint, LOS, true))
												{
													 Logging.WriteDiagnostic("Kitespot found at {0} with {1} distance from our current position!", vAnySafePoint.ToString(), vAnySafePoint.Distance(Bot.Character.Position));
													 Bot.Combat.IsKiting=true;

													 if (nearbyUnits.Any())
														  //Cache the first unit that triggers the kiting so we can swap back to it.
														  Bot.Character.LastCachedTarget=nearbyUnits.First();//unit => unit.IsStillValid());

													 //Extend kill range since we were kiting..
													 iKeepKillRadiusExtendedFor=20;
													 Bot.Target.ObjectData=new CacheObject(vAnySafePoint, TargetType.Avoidance, 20000f, "Kitespot", 2.5f, -1);
												}
												UpdateAvoidKiteRates();
										  }
									 }
								}
								else if (Bot.Combat.KitedLastTarget&&Bot.Target.Equals(null)
										  &&Bot.Character.LastCachedTarget!=null
										  &&ObjectCache.Objects.ContainsKey(Bot.Character.LastCachedTarget.RAGUID))
								{
									 //Swap back to our orginal "kite" target
									 Bot.Target.ObjectData=ObjectCache.Objects[Bot.Character.LastCachedTarget.RAGUID];
									 Logging.WriteVerbose("Swapping back to unit {0} after kiting!", Bot.Target.ObjectData.InternalName);
								}
						  }
						  #endregion

						  /*
						  //We didn't find a target.. but lets check if its because of avoidance causing us to ignore it.
						  if (Bot.Target.Equals(null)&&ObjectCache.Objects.objectsIgnoredDueToAvoidance.Count>0
								&&!Bot.Character.bIsInTown&&!Bot.Combat.CriticalAvoidance)
						  {
								//Test each object for a potential spot that is safe to travel to
								foreach (var item in ObjectCache.Objects.objectsIgnoredDueToAvoidance)
								{
									 if (GridPointAreaCache.AttemptFindTargetSafeLocation(out vlastSafeSpot, item, true))
									 {	  //Found a point around this target which is considered safe!
										  //This will only be objects that require close range.. (for melee classes that includes units).
										  Bot.Target.ObjectData=new CacheObject(vlastSafeSpot, TargetType.Avoidance, 20000f, "SafePoint", 2.5f, -1);
										  Logging.WriteVerbose("Found a safe point around target {0}, attempting to travel there!", item.InternalName);
										  break;
									 }
								}
						  }
						  */

					 } // Not heading straight for a safe-spot?


					 //Update last Refresh Time
					 lastRefreshedObjects=DateTime.Now;

					 // No valid targets but we were told to stay put?
					 if (Bot.Target.Equals(null)&&Bot.Combat.bStayPutDuringAvoidance)
					 {
						  if (!Bot.Combat.RequiresAvoidance)
								Bot.Target.ObjectData=new CacheObject(Bot.Character.Position, TargetType.Avoidance, 20000, "StayPutPoint", 2.5f, -1);
						  else
								Bot.Combat.iMillisecondsCancelledEmergencyMoveFor=0; //reset wait time
					 }

					 // Still no target, let's see if we should backtrack or wait for wrath to come off cooldown...
					 if (Bot.Target.Equals(null))
					 {
						  // See if we should wait for [playersetting] milliseconds for possible loot drops before continuing run
						  if (DateTime.Now.Subtract(Bot.Combat.lastHadUnitInSights).TotalMilliseconds<=SettingsFunky.AfterCombatDelay&&DateTime.Now.Subtract(Bot.Combat.lastHadEliteUnitInSights).TotalMilliseconds<=10000||
								//Cut the delay time in half for non-elite monsters!
							  DateTime.Now.Subtract(Bot.Combat.lastHadUnitInSights).TotalMilliseconds<=lootDelayTime)
						  {
								Bot.Target.ObjectData=new CacheObject(Bot.Character.Position, TargetType.Avoidance, 20000, "WaitForLootDrops", 0f, -1);
						  }

						  //Herbfunks wait after loot containers are opened. 3s for rare chests, half the settings delay for everything else.
						  if ((DateTime.Now.Subtract(Bot.Combat.lastHadRareChestAsTarget).TotalMilliseconds<=3750)||
							  (DateTime.Now.Subtract(Bot.Combat.lastHadContainerAsTarget).TotalMilliseconds<=(lootDelayTime*1.25)))
						  {
								Bot.Target.ObjectData=new CacheObject(Bot.Character.Position, TargetType.Avoidance, 20000, "ContainerLootDropsWait", 0f, -1);
						  }


						  // Finally, a special check for waiting for wrath of the berserker cooldown before engaging Azmodan
						  if (Bot.Target.Equals(null)&&HotbarAbilitiesContainsPower(SNOPower.Barbarian_WrathOfTheBerserker)&&SettingsFunky.Class.bWaitForWrath&&!AbilityUseTimer(SNOPower.Barbarian_WrathOfTheBerserker)&&
							  ZetaDia.CurrentWorldId==121214&&
							  (Vector3.Distance(Bot.Character.Position, new Vector3(711.25f, 716.25f, 80.13903f))<=40f||Vector3.Distance(Bot.Character.Position, new Vector3(546.8467f, 551.7733f, 1.576313f))<=40f))
						  {
								Logging.Write("[Funky] Waiting for Wrath Of The Berserker cooldown before continuing to Azmodan.");
								Bot.Target.ObjectData=new CacheObject(Bot.Character.Position, TargetType.Avoidance, 20000, "GilesWaitForWrath", 0f, -1);
						  }
						  // And a special check for wizard archon
						  if (Bot.Target.Equals(null)&&HotbarAbilitiesContainsPower(SNOPower.Wizard_Archon)&&!AbilityUseTimer(SNOPower.Wizard_Archon)&&SettingsFunky.Class.bWaitForArchon&&ZetaDia.CurrentWorldId==121214&&
							  (Vector3.Distance(Bot.Character.Position, new Vector3(711.25f, 716.25f, 80.13903f))<=40f||Vector3.Distance(Bot.Character.Position, new Vector3(546.8467f, 551.7733f, 1.576313f))<=40f))
						  {
								Logging.Write("[Funky] Waiting for Wizard Archon cooldown before continuing to Azmodan.");
								Bot.Target.ObjectData=new CacheObject(Bot.Character.Position, TargetType.Avoidance, 20000, "GilesWaitForArchon", 0f, -1);
						  }
						  // And a very sexy special check for WD BigBadVoodoo
						  if (Bot.Target.Equals(null)&&HotbarAbilitiesContainsPower(SNOPower.Witchdoctor_BigBadVoodoo)&&!PowerManager.CanCast(SNOPower.Witchdoctor_BigBadVoodoo)&&ZetaDia.CurrentWorldId==121214&&
							  (Vector3.Distance(Bot.Character.Position, new Vector3(711.25f, 716.25f, 80.13903f))<=40f||Vector3.Distance(Bot.Character.Position, new Vector3(546.8467f, 551.7733f, 1.576313f))<=40f))
						  {
								Logging.Write("[Funky] Waiting for WD BigBadVoodoo cooldown before continuing to Azmodan.");
								Bot.Target.ObjectData=new CacheObject(Bot.Character.Position, TargetType.Avoidance, 20000, "GilesWaitForVoodooo", 0f, -1);
						  }
					 }



					 // Still no target, let's end it all!
					 if (Bot.Target.Equals(null))
					 {
						  //Check if our current path intersects avoidances. (When not in town, and not currently inside avoidance)
						  if (!Bot.Character.bIsInTown&&(SettingsFunky.AttemptAvoidanceMovements||Bot.Combat.CriticalAvoidance)
								&&navigation.CurrentPath.Count>0
								&&!ObjectCache.Obstacles.Avoidances.Any(a => a.PointInside(Bot.Character.Position)&&a.ShouldAvoid))
						  {
								Vector3 curpos=Bot.Character.Position;
								IndexedList<Vector3> curpath=navigation.CurrentPath;

								var CurrentNearbyPath=curpath.Where(v => curpos.Distance(v)<=30f);
								if (CurrentNearbyPath!=null&&CurrentNearbyPath.Any())
								{
									 CurrentNearbyPath.OrderBy(v => curpath.IndexOf(v));

									 Vector3 lastV3=vNullLocation;
									 foreach (var item in CurrentNearbyPath)
									 {
										  if (lastV3==vNullLocation)
												lastV3=item;
										  else if (ObjectCache.Obstacles.TestVectorAgainstAvoidanceZones(item, lastV3))
												Bot.Target.ObjectData=new CacheObject(Bot.Character.Position, TargetType.Avoidance, 20000, "AvoidanceIntersection", 2.5f, -1);
									 }
								}
						  }

						  //clear any prioritzed objects
						  Bot.Combat.PrioritizedRAGUIDs.Clear();

						  return;
					 }

					 if (SettingsFunky.EnableWaitAfterContainers&&Bot.Target.ObjectData.targetType==TargetType.Container)
					 {
						  //Herbfunks delay for container loot.
						  Bot.Combat.lastHadContainerAsTarget=DateTime.Now;

						  if (SnoCacheLookup.hashSNOContainerResplendant.Contains(Bot.Target.ObjectData.SNOID))
								Bot.Combat.lastHadRareChestAsTarget=DateTime.Now;
					 }
					 // Record the last time our target changed etc.
					 if (Bot.Target.ObjectData!=Bot.Character.LastCachedTarget)
					 {
						  Bot.Combat.dateSincePickedTarget=DateTime.Now;
						  Bot.Combat.LastHealthChange=DateTime.Now;
						  iTargetLastHealth=0f;
						  Bot.Combat.bLOSMovement=false;
					 }
					 else
					 {
						  // We're sticking to the same target, so update the target's health cache to check for stucks
						  if (Bot.Target.ObjectData.targetType==TargetType.Unit)
						  {
								CacheUnit thisUnitObj=(CacheUnit)Bot.Target.ObjectData;
								//Used to pause after no targets found.
								Bot.Combat.lastHadUnitInSights=DateTime.Now;

								// And record when we last saw any form of elite
								if (Bot.Target.ObjectData.IsBoss||thisUnitObj.IsEliteRareUnique||Bot.Target.ObjectData.IsTreasureGoblin)
									 Bot.Combat.lastHadEliteUnitInSights=DateTime.Now;

								// Check if the health has changed, if so update the target-pick time before we blacklist them again
								if (thisUnitObj.CurrentHealthPct.HasValue&&thisUnitObj.CurrentHealthPct.Value!=iTargetLastHealth)
								{
									 Bot.Combat.dateSincePickedTarget=DateTime.Now;
								}
								// Now store the target's last-known health
								iTargetLastHealth=thisUnitObj.CurrentHealthPct.Value;
						  }
					 }
				}
		  }
	 }
}