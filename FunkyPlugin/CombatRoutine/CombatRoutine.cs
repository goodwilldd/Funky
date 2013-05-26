﻿using System.Windows;
using Zeta;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;
using System.Windows.Controls;
using System.IO;
using System.Reflection;

namespace GilesBlankCombatRoutine
{
	 [System.Runtime.InteropServices.ComVisible(false)]
    public class FunkyRoutine : CombatRoutine
    {
        private static string sDemonBuddyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static string sTrinityPluginPath = sDemonBuddyPath + @"\Plugins\FunkyTrinity\";

        
        public override void Initialize()
        {
            // Set up the pause button
            System.Windows.Application.Current.Dispatcher.Invoke(
            new System.Action(
            () =>
            {
                Window mainWindow = System.Windows.Application.Current.MainWindow;
                try
                {
                    mainWindow.Title = "DB - " + ZetaDia.Service.CurrentHero.BattleTagName;
                }
                catch
                {

                }
                var tab = mainWindow.FindName("tabControlMain") as TabControl;
                if (tab == null) return;
                var infoDumpTab = tab.Items[0] as TabItem;
                if (infoDumpTab == null) return;
                var grid = infoDumpTab.Content as Grid;
                if (grid == null) return;
					 var FunkyButton=grid.FindName("Funky");
					 
					 Demonbuddy.SplitButton btnSplit_Funky;
                FunkyDebug.initDebugLabels(out btnSplit_Funky);

					 if (FunkyButton!=null)
						  return;
					 else
						  grid.Children.Add(btnSplit_Funky);
            }));
        }


        public sealed override void Dispose()
        {
				// Set up the pause button
				System.Windows.Application.Current.Dispatcher.Invoke(
				new System.Action(
				() =>
				{
					 Window mainWindow=System.Windows.Application.Current.MainWindow;
					 var tab=mainWindow.FindName("tabControlMain") as TabControl;
					 if (tab==null) return;
					 var infoDumpTab=tab.Items[0] as TabItem;
					 if (infoDumpTab==null) return;
					 var grid=infoDumpTab.Content as Grid;
					 if (grid==null) return;
					 Demonbuddy.SplitButton btnfunky = grid.FindName("Funky") as Demonbuddy.SplitButton;
					 if (btnfunky==null) return;
					 grid.Children.Remove(btnfunky);
				}));
				
				System.GC.SuppressFinalize(this);
				return;
        }

        public override string Name { get { return "Funky"; } }

        public override Window ConfigWindow { get { return null; } }

        public override ActorClass Class { get { return ZetaDia.Me.ActorClass; } }

        public override SNOPower DestroyObjectPower
        {
            get
            {
                if (ZetaDia.IsInGame)
                    return ZetaDia.CPlayer.GetPowerForSlot(HotbarSlot.HotbarMouseLeft);
                else
                    return SNOPower.None;
            }
        }

        public override float DestroyObjectDistance { get { return 15; } }

        /*private Composite _combat;
        private Composite _buff;*/
        public override Composite Combat { get { return new PrioritySelector(); } }
        public override Composite Buff { get { return new PrioritySelector(); } }

    }
}
