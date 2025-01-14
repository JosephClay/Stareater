﻿using Stareater.Galaxy;
using Stareater.SpaceCombat;
using Stareater.Utils;

namespace Stareater.Controllers.Views.Combat
{
	public class CombatPlanetInfo
	{
		internal readonly CombatPlanet Data;
		
		internal CombatPlanetInfo(CombatPlanet data)
		{
			this.Data = data;
		}
		
		public int OrdinalPosition 
		{
			get { return Data.PlanetData.Position; }
		}
		
		public Vector2D Position 
		{
			get { return Data.Position; }
		}

		public PlanetType Type {
			get { return this.Data.PlanetData.Type; }
		}
		
		public PlayerInfo Owner 
		{
			get { return Data.Colony != null ? new PlayerInfo(Data.Colony.Owner) : null; }
		}
		
		public double Population 
		{
			get { return Data.Colony != null ? Data.Colony.Population : 0; }
		}
	}
}
