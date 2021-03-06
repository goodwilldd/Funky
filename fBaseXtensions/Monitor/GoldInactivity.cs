﻿using System;
using fBaseXtensions.Helpers;
using fBaseXtensions.Settings;

namespace fBaseXtensions.Monitor
{
	public static class GoldInactivity
	{
		public static bool TimeoutTripped { get; private set; }
		public static DateTime LastCoinageUpdate
		{
			get { return _lastcoinageupdate; }
			set
			{
				_lastcoinageupdate = value;
				TimeoutTripped = false;
			}
		}
		private static DateTime _lastcoinageupdate=DateTime.Now;

		internal static void CheckTimeoutTripped()
		{
			if (OnGoldTimeoutTripped == null) return;
			if (MonitorSettings.MonitorSettingsTag.GoldInactivityTimeoutSeconds == 0) return;

			double lastCoinageChange = DateTime.Now.Subtract(LastCoinageUpdate).TotalSeconds;
			if (lastCoinageChange > 5)
			{
				TimeoutTripped = lastCoinageChange >= MonitorSettings.MonitorSettingsTag.GoldInactivityTimeoutSeconds;
				if (TimeoutTripped)
				{
					Logger.DBLog.Info("[Funky] Gold Timeout Breached");
					OnGoldTimeoutTripped();
				}
			}
		}

		public delegate void GoldTimeoutTripped();
		public static event GoldTimeoutTripped OnGoldTimeoutTripped;
	}
}
