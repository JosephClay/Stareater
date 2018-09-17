﻿using System.Collections.Generic;
using System.Linq;
using Stareater.Galaxy;
using Stareater.Ships;
using Stareater.Ships.Missions;

namespace Stareater.GameData.Construction
{
	class ConstructionAddColonizer : IConstructionEffect
	{
		public Design ColonizerDesign { get; private set; }
		public Planet Destination { get; private set; }

		public ConstructionAddColonizer(Design colonizerDesign, Planet destination)
		{
			this.ColonizerDesign = colonizerDesign;
			this.Destination = destination;
		}
			
		#region IConstructionEffect implementation
		public void Apply(MainGame game, AConstructionSite site, long quantity)
		{
			var missions = new LinkedList<AMission>();
			missions.AddLast(new SkipTurnMission());
			missions.AddLast(new LoadMission());

			//TODO(later) check shortest path
			if (site.Location.Star != Destination.Star)
			{
				var lastStar = site.Location.Star;
				var nextStar = Destination.Star;
				var wormhole = game.States.Wormholes.At[lastStar, nextStar].FirstOrDefault();
				missions.AddLast(new MoveMission(Destination.Star, wormhole));
			}
			
			missions.AddLast(new ColonizationMission(Destination));

			//TODO(v0.8) report new ship construction
			game.Derivates.Of(site.Owner).
				SpawnShip(site.Location.Star, this.ColonizerDesign, quantity, 0, missions, game.States);
		}
		#endregion
	}
}
