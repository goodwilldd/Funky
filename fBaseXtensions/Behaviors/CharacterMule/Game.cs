﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using fBaseXtensions.Behaviors;
using fBaseXtensions.Game;
using fBaseXtensions.Helpers;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Bot.Settings;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.Actors.Gizmos;
using Zeta.TreeSharp;
using Logger = fBaseXtensions.Helpers.Logger;
using LogLevel = fBaseXtensions.Helpers.LogLevel;

namespace FunkyBot.DBHandlers.CharacterMule
{

	public static class NewMuleGame
	{
		private static DateTime LastActionTaken = DateTime.Today;
		private static int RandomWaitTimeMilliseconds = 1000;

		private static void RandomizeWaitTime(bool longwait = false)
		{
			Random R = new Random(DateTime.Now.Millisecond);
			if (!longwait)
				RandomWaitTimeMilliseconds = R.Next(1000, 2250);
			else
				RandomWaitTimeMilliseconds = R.Next(3050, 5880);
		}

		internal static int BotHeroIndex = -1;
		internal static string LastProfile = null;
		internal static string BotHeroName = null;
		internal static int LastHandicap = 0;

		public static RunStatus BeginNewGameProfile()
		{
			if (DateTime.Now.Subtract(LastActionTaken).TotalMilliseconds > RandomWaitTimeMilliseconds)
			{
				string NewGameProfile = FolderPaths.PluginPath + @"Behaviors\CharacterMule\NewGame.xml";
				if (ProfileManager.CurrentProfile.Path != NewGameProfile)
				{
					if (File.Exists(NewGameProfile))
					{
						Logger.Write(LogLevel.OutOfGame, "Loading NewGame profile");
						ProfileManager.Load(NewGameProfile);
						CharacterSettings.Instance.MonsterPowerLevel = 0;
					}
				}
				else
					return RunStatus.Success;

				LastActionTaken = DateTime.Now;

			}
			return RunStatus.Running;
		}

		private static GizmoPlayerSharedStash CurrentStashObject;
		private static Vector3 StashV3 = new Vector3(2971.285f, 2798.801f, 24.04533f);
		private static Queue<ACDItem> SortedStashItems = new Queue<ACDItem>();


		public static RunStatus StashTransfer()
		{
			if (CurrentStashObject == null)
			{
				//Find the stash object
				ZetaDia.Actors.Update();
				var objs = ZetaDia.Actors.GetActorsOfType<GizmoPlayerSharedStash>(true, true);
				if (objs.Any())
				{
					CurrentStashObject = objs.First();
				}
				else
				{
					Logger.DBLog.InfoFormat("Failed to find stash.. Moving To Stash Vector");
					Navigator.MoveTo(StashV3, "Stash");
				}
			}
			else if (!UIElements.StashWindow.IsVisible)
			{
				if (CurrentStashObject.Distance > 7.5f)
				{
					if (!FunkyGame.Hero.IsMoving)
					{
						if (CurrentStashObject.Distance > 50f)
							ZetaDia.Me.UsePower(SNOPower.Walk, CurrentStashObject.Position, ZetaDia.Me.WorldDynamicId);
						else
						{
							//Wait until we are not moving to send click again..
							if (FunkyGame.Hero.IsMoving)
								return RunStatus.Running;

							ZetaDia.Me.UsePower(SNOPower.Axe_Operate_Gizmo, CurrentStashObject.Position, ZetaDia.Me.WorldDynamicId, CurrentStashObject.ACDGuid);
						}
					}
				}
				else
					CurrentStashObject.Interact();
			}
			else
			{
				if (SortedStashItems.Count == 0)
				{
					int itemSlotsFilled = 0;
					var sortedItems = ZetaDia.Me.Inventory.StashItems.Where(item => !ItemManager.Current.ItemIsProtected(item)).OrderBy(item => !item.IsTwoSquareItem).ThenByDescending(item => item.InventoryRow).ThenByDescending(item => item.InventoryColumn);
					foreach (var item in sortedItems)
					{
						SortedStashItems.Enqueue(item);
						itemSlotsFilled += item.IsTwoSquareItem ? 2 : 1;
						if (itemSlotsFilled > 59)
							break;
					}

				}
				else
				{
					if (ZetaDia.Me.Inventory.NumFreeBackpackSlots > 1)
					{
						if (DateTime.Now.Subtract(LastActionTaken).TotalMilliseconds > RandomWaitTimeMilliseconds)
						{
							// ACDItem currentItem=SortedStashItems[0];
							ZetaDia.Me.Inventory.QuickWithdraw(SortedStashItems.Dequeue());
							LastActionTaken = DateTime.Now;
							RandomizeWaitTime();
						}
					}
					else
					{
						CurrentStashObject = null;
						SortedStashItems.Clear();
						LastActionTaken = DateTime.Today;
						OutOfGame.TransferedGear = true;
						
						//Delete settings
						string sFunkyCharacterFolder = Path.Combine(FolderPaths.DemonBuddyPath, "Settings", "FunkyBot", FunkyGame.CurrentAccountName);
						if (Directory.Exists(sFunkyCharacterFolder))
						{
							string sFunkyCharacterConfigFile = Path.Combine(sFunkyCharacterFolder, FunkyGame.CurrentHeroName + ".cfg");
							if (File.Exists(sFunkyCharacterConfigFile))
								File.Delete(sFunkyCharacterConfigFile);
						}

						OutOfGame.NewCharacterName = null;

						ZetaDia.Service.Party.LeaveGame(true);
						return RunStatus.Running;
					}
				}

			}


			return RunStatus.Running;
		}


		public static RunStatus FinishMuleBehavior()
		{
			if (DateTime.Now.Subtract(LastActionTaken).TotalMilliseconds > RandomWaitTimeMilliseconds)
			{
				if (ZetaDia.IsInGame)
				{
					ZetaDia.Service.Party.LeaveGame(true);
					RandomizeWaitTime(true);
				}
				else if (ZetaDia.Service.Hero.Name != BotHeroName)
				{
					//ISSUE: Does Not Select Hero!
					ZetaDia.Service.GameAccount.SwitchHero(BotHeroIndex);
					RandomizeWaitTime(true);
					BotHeroIndex++;
				}
				else if (ProfileManager.CurrentProfile.Path != LastProfile)
				{
					ProfileManager.Load(LastProfile);
					RandomizeWaitTime();
					CharacterSettings.Instance.MonsterPowerLevel = LastHandicap;
				}
				else
				{
					return RunStatus.Success;
				}
				LastActionTaken = DateTime.Now;

			}

			return RunStatus.Running;
		}
	}

}