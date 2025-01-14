﻿using Stareater.Utils.StateEngine;
using Stareater.Ships;

namespace Stareater.Galaxy
{
	class ShipGroup 
	{
		[StateProperty]
		public Design Design { get; private set; }

		[StateProperty]
		public long Quantity { get; set; }

		[StateProperty]
		public double Damage { get; set; }

		[StateProperty]
		public double UpgradePoints { get; set; }

		[StateProperty]
		public double PopulationTransport { get; set; }

		public ShipGroup(Design design, long quantity, double damage, double upgradePoints, double population) 
		{
			this.Design = design;
			this.Quantity = quantity;
			this.Damage = damage;
			this.UpgradePoints = upgradePoints;
			this.PopulationTransport = population;
		}

		private ShipGroup() 
		{ } 
	}
}
