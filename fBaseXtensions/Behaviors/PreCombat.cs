﻿using System;
using System.Linq;
using System.Threading.Tasks;
using fBaseXtensions.Game;
using FunkyBot.DBHandlers.CharacterMule;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors.Gizmos;
using Zeta.TreeSharp;
using Logger = fBaseXtensions.Helpers.Logger;

namespace fBaseXtensions.Behaviors
{
	public static class PreCombat
	{
		public static bool PreCombatOverlord(object ret)
		{
			if (FunkyGame.GameIsInvalid) return false;

			FunkyGame.Targeting.CheckPrecombat();

			//Check for game prohibiting ui elements (Achievements, Skills, Waypoint Map, etc)
			var uie = UI.FindGameProhibitingElements();
			if (uie != null)
			{
				UI.ClosingUIElements = true;
				return true;
			}


			if (OutOfGame.MuleBehavior)
			{
				if (BotMain.StatusText.Contains("Game Finished"))
				{
					if (ZetaDia.Actors.GetActorsOfType<GizmoPlayerSharedStash>(true, true).Any())
					{
						return true;
					}
				}
			}
			else if (ExitGame.ShouldExitGame)
			{
				ExitGame.BehaviorEngaged = true;
				return true;
			}
			//else if(FunkyBaseExtension.Settings.AdventureMode.AllowCombatModifications && BountyCache.RiftTrialIsActiveQuest && FunkyGame.Hero.iCurrentLevelID == 405915)
			//{
			//	Logger.DBLog.Info("Performing Trial Rift Handler Behavior!");
			//	return true;
			//}
			//else if (GoblinBehavior.BehaviorEngaged && GoblinBehavior.ShouldRunBehavior())
			//{
			//	Logger.DBLog.Info("Starting Goblin Behavior.");
			//	return true;
			//}

			return false;
		}

		public static RunStatus HandleTarget(object ret)
		{
			if (UI.ClosingUIElements)
				return UI.CloseGameProhibitingElements();

			if (OutOfGame.MuleBehavior)
			{
				if (!OutOfGame.TransferedGear)
				{
					return NewMuleGame.StashTransfer();
				}

				return NewMuleGame.FinishMuleBehavior();
			}

			//Exit Game!!
			if (ExitGame.BehaviorEngaged)
			{
				return ExitGame.Behavior();
			}

			//Trial Rift!
			//if (FunkyBaseExtension.Settings.AdventureMode.AllowCombatModifications && BountyCache.RiftTrialIsActiveQuest && FunkyGame.Hero.iCurrentLevelID == 405915)
			//{
			//	return TrialRiftBehavior.Behavior();
			//}

			//if (GoblinBehavior.BehaviorEngaged)
			//	return GoblinBehavior.Behavior();

			return RunStatus.Success;
		}
	}
}
