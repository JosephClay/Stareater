﻿using Stareater.Utils.StateEngine;
using System.Collections.Generic;
using Stareater.GameData.Databases.Tables;
using Stareater.Players;
using Stareater.Ships.Missions;
using Stareater.Utils;

namespace Stareater.Galaxy
{
	class Fleet 
	{
		[StateProperty]
		public Player Owner { get; private set; }

		[StateProperty]
		public Vector2D Position { get; private set; }

		[StateProperty]
		public LinkedList<AMission> Missions { get; private set; }

		[StateProperty]
		public ShipGroupCollection Ships { get; private set; }

		public Fleet(Player owner, Vector2D position, LinkedList<AMission> missions) 
		{
			this.Owner = owner;
			this.Position = position;
			this.Missions = missions;
			this.Ships = new ShipGroupCollection();
		}

		private Fleet() 
		{ }
	}
}
