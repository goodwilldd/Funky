﻿using System.IO;
using System.Xml.Serialization;

namespace FunkyTrinity.Settings
{
	public class SettingTargeting
	{

		 public bool IgnoreAboveAverageMobs { get; set; }
		 public bool IgnoreCorpses { get; set; }
		 public bool MissleDampeningEnforceCloseRange { get; set; }
		 public int GoblinPriority { get; set; }
		 public bool[] UseShrineTypes { get; set; }
		 public bool UseExtendedRangeRepChest { get; set; }

		 public SettingTargeting()
		 {
			  GoblinPriority=2;
			  UseShrineTypes=new bool[6] { true, true, true, true, true, true };
			  IgnoreAboveAverageMobs=false;
			  IgnoreCorpses=false;
			  UseExtendedRangeRepChest=false;
			  MissleDampeningEnforceCloseRange=true;
		 }
		 private static string DefaultFilePath=Path.Combine(Funky.FolderPaths.sTrinityPluginPath, "Config", "Defaults", "Targeting_Default.xml");
		 public static SettingTargeting DeserializeFromXML()
		 {
			  XmlSerializer deserializer=new XmlSerializer(typeof(SettingTargeting));
			  TextReader textReader=new StreamReader(DefaultFilePath);
			  SettingTargeting settings;
			  settings=(SettingTargeting)deserializer.Deserialize(textReader);
			  textReader.Close();
			  return settings;
		 }
		 public static SettingTargeting DeserializeFromXML(string Path)
		 {
			  XmlSerializer deserializer=new XmlSerializer(typeof(SettingTargeting));
			  TextReader textReader=new StreamReader(Path);
			  SettingTargeting settings;
			  settings=(SettingTargeting)deserializer.Deserialize(textReader);
			  textReader.Close();
			  return settings;
		 }
	}
}
