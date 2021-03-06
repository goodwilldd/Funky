﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fBaseXtensions.Game;
using fBaseXtensions.Game.Hero;
using fBaseXtensions.Helpers;
using fBaseXtensions.Settings;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals.Service;
using Zeta.TreeSharp;

namespace fBaseXtensions.Behaviors
{
    public static class CharacterControl
    {
        private static BnetCharacterIndexInfo _heroindexinfo = new BnetCharacterIndexInfo();

        public static BnetCharacterIndexInfo HeroIndexInfo
        {
            get
            {
                if (_heroindexinfo.Characters.Count == 0)
                {
                    if (File.Exists(BnetCharacterIndexInfo.BnetCharacterInfoSettingsPath))
                    {
                        _heroindexinfo = BnetCharacterIndexInfo.DeserializeFromXML();
                    }
                }
                return _heroindexinfo;
            }
        }

        /// <summary>
        /// If we are engaging a character switch to preform a town run with.
        /// </summary>
        public static bool GamblingCharacterSwitch = false;

        //For the combat handler!
        internal static bool AltHeroGamblingEnabled = false;
        //To switch back after finished!
        internal static bool GamblingCharacterSwitchToMain = false;

        public static RunStatus GamblingCharacterSwitchBehavior()
        {
            if (!_delayer.Test(5d))
                return RunStatus.Running;

            if (GamblingCharacterSwitchToMain)
            {
                ZetaDia.Memory.ClearCache();
                HeroInfo curheroinfo = new HeroInfo(ZetaDia.Service.Hero);

                if (!curheroinfo.Equals(MainHeroInfo))
                    ZetaDia.Service.GameAccount.SwitchHero(FunkyBaseExtension.Settings.General.AltHeroIndex);
                else
                {
                    
                    FunkyGame.ShouldRefreshAccountDetails = true;

                    MainHeroInfo = null;
                    AltHeroInfo = null;
                    GamblingCharacterSwitch = false;
                    GamblingCharacterSwitchToMain = false;

                    return RunStatus.Success;
                }

                return RunStatus.Running;
            }
            //Update Main Hero Info and Switch to Alt Hero!
            if (MainHeroInfo == null)
                return CharacterSwitch();

            //Update Alt Hero Info and Start Adventure Mode Game!
            if (AltHeroInfo == null)
                return UpdateAltHero();


            //Finished for now.. lets load the new game and let combat control take over!
            FunkyGame.ShouldRefreshAccountDetails = true;
            GamblingCharacterSwitch = false;
            AltHeroGamblingEnabled = true;
            return RunStatus.Success;


            //if (!ZetaDia.IsInGame)
            //{
            //    BotMain.StatusText = "[Funky] Hero Switch *Loading Profile!*";

            //    //Load Adventure Mode Profile!
            //    string NewGameProfile = Path.Combine(FolderPaths.PluginPath, "Behaviors","Profiles", "AdventureMode.xml");

                

            //    if (ProfileManager.CurrentProfile.Path != NewGameProfile)
            //    {
            //        Logger.Write(LogLevel.OutOfGame, "Current Profile Path: {0}\r\nAdventureMode Profile Path {1}",
            //        ProfileManager.CurrentProfile.Path, NewGameProfile);

            //        if (File.Exists(NewGameProfile))
            //        {
            //            Logger.Write(LogLevel.OutOfGame, "Loading UpdateAltHero profile");
            //            ProfileManager.Load(NewGameProfile,false);
            //            return RunStatus.Running;
            //        }

            //        //Could not find file!
            //        return RunStatus.Success;
            //    }
            //    else
            //    {
            //        //Finished for now.. lets load the new game and let combat control take over!
            //        FunkyGame.ShouldRefreshAccountDetails = true;
            //        GamblingCharacterSwitch = false;
            //        AltHeroGamblingEnabled = true;
            //        return RunStatus.Success;
            //    }
            //}

            
            return RunStatus.Success;
        }


        private static int _startingBloodShardCount = -1;
        private static bool _forcedTownRun = false;
        internal static RunStatus GamblingCharacterCombatHandler()
        {
            if (_startingBloodShardCount == -1)
                _startingBloodShardCount = Backpack.GetBloodShardCount();
            

            if (!Zeta.Bot.Logic.BrainBehavior.IsVendoring)
            {
                if (_startingBloodShardCount != Backpack.GetBloodShardCount() || _forcedTownRun)
                {
                    //Finished!
                    ExitGameBehavior.ShouldExitGame = true;
                    AltHeroGamblingEnabled = false;
                    GamblingCharacterSwitchToMain = true;
                    GamblingCharacterSwitch = true;
                    _startingBloodShardCount = -1;
                    _forcedTownRun = false;
                    _delayer.Reset();
                    ProfileManager.Load(_lastProfilePath, false);
                    return RunStatus.Success;
                }
                else
                {
                    Zeta.Bot.Logic.BrainBehavior.ForceTownrun();
                    _forcedTownRun = true;
                    return RunStatus.Running;
                }
            }
            else
            {
                return RunStatus.Success;
            }
        }


        private static Delayer _delayer = new Delayer();

        private static HeroInfo MainHeroInfo;
        private static string _lastProfilePath;
        private static RunStatus CharacterSwitch()
        {
            BotMain.StatusText = "[Funky] Hero Switch *Switching Heros*";
            if (FunkyBaseExtension.Settings.General.AltHeroIndex < 0)
            {
                Logger.DBLog.InfoFormat("Hero Index Info not setup!");
                BotMain.Stop();
                return RunStatus.Success;
            }

            if (HeroIndexInfo.Characters.Count == 0)
            {
                Logger.DBLog.InfoFormat("Hero Index Info not setup!");
                BotMain.Stop();
                return RunStatus.Success;
            }

            if (MainHeroInfo == null)
            {
                ZetaDia.Memory.ClearCache();
                MainHeroInfo = new HeroInfo(ZetaDia.Service.Hero);
            }
            _lastProfilePath = ProfileManager.CurrentProfile.Path;
            Logger.DBLog.InfoFormat("Switching to Hero Index {0}", FunkyBaseExtension.Settings.General.AltHeroIndex);
            ZetaDia.Service.GameAccount.SwitchHero(FunkyBaseExtension.Settings.General.AltHeroIndex);
            return RunStatus.Running;
        }


        private static HeroInfo AltHeroInfo;

        private static RunStatus UpdateAltHero()
        {
            BotMain.StatusText = "[Funky] Hero Switch *Refreshing Alt Info*";
            if (AltHeroInfo == null)
            {
                ZetaDia.Memory.ClearCache();
                AltHeroInfo = new HeroInfo(ZetaDia.Service.Hero);
                return RunStatus.Running;
            }

            if (AltHeroInfo.Equals(MainHeroInfo))
            {
                //Switch Failed or Values were incorrect!
                return RunStatus.Success;
            }

            return RunStatus.Running;
        }

        /// <summary>
        /// Used as a temporary wrapper of hero info
        /// </summary>
        public class HeroInfo
        {
            public string Name { get; set; }
            public int Level { get; set; }
            public int ParagonLevel { get; set; }
            public int QuestSNO { get; set; }
            public int QuestStep { get; set; }
            public ActorClass Class { get; set; }
            public TimeSpan TimePlayed { get; set; }


            public HeroInfo(BnetHero hero)
            {
                Name = hero.Name;
                Level = hero.Level;
                ParagonLevel = hero.ParagonLevel;
                QuestSNO = hero.QuestSNO;
                QuestStep = hero.QuestStep;
                Class = hero.Class;
                TimePlayed = hero.TimePlayed;
            }

            public HeroInfo()
            {
                Name = String.Empty;
                Level = -1;
                ParagonLevel = -1;
                QuestSNO = -1;
                QuestStep = -1;
                Class = ActorClass.Invalid;
                TimePlayed= new TimeSpan(0,0,0,0);
            }

            public override string ToString()
            {
                return String.Format("Hero: Name {0} Level {1} Paragon {2} Class {3} QuestSNO {4} Step {5} TimePlayed {6}",
                                            Name,Level,ParagonLevel,Class,QuestSNO,QuestStep,TimePlayed);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                //Check for null and compare run-time types. 
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                else
                {
                    HeroInfo p = (HeroInfo)obj;
                    return 
                        (Name == p.Name) && 
                        (Level == p.Level) && 
                        (Class == p.Class) &&
                        (ParagonLevel == p.ParagonLevel) &&
                        (TimePlayed == p.TimePlayed);
                }
            }
        }
    }
}
