﻿using System.Runtime.InteropServices;
using Zeta.Common;
using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace FunkyBot.XMLTags
{
	[ComVisible(false)]
	[XmlElement("TrinityLog")]
	public class TrinityLogTag : ProfileBehavior
	{
		private bool m_IsDone;

		public override bool IsDone
		{
			get { return m_IsDone; }
		}

		protected override Composite CreateBehavior()
		{
			return new Action(ret =>
			{
				if (Level!=null&&Level.ToLower()=="diagnostic")
					Logging.WriteDiagnostic(Output);
				else
					Logging.Write(Output);
				m_IsDone=true;
			});
		}

		[XmlAttribute("level")]
		public string Level { get; set; }

		[XmlAttribute("output")]
		public string Output { get; set; }

		public override void ResetCachedDone()
		{
			m_IsDone=false;
			base.ResetCachedDone();
		}
	}
}