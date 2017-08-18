﻿using Ikadn.Ikon.Types;
using Stareater.Utils.StateEngine;

namespace Stareater.Players.Natives
{
	public class OrganellePlayerFactory : IOffscreenPlayerFactory
	{
		#region IOffscreenPlayerFactory implementation
		public IOffscreenPlayer Create()
		{
			return new OrganellePlayer();
		}
		
		public IOffscreenPlayer Load(IkonComposite rawData, LoadSession session)
		{
			return session.Load<OrganellePlayer>(rawData);
		}
		
		public string Id 
		{
			get { return "OrganelleAI"; }
		}
		
		public string Name 
		{
			get { return "no name"; } //TODO(later) make name
		}
		#endregion
	}
}
