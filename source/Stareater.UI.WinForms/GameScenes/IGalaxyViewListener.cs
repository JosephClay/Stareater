﻿using System.Collections.Generic;
using Stareater.Controllers;
using Stareater.Controllers.Views.Ships;

namespace Stareater.GameScenes
{
	public interface IGalaxyViewListener
	{
		void TurnEnded();

		void FleetDeselected();
		void FleetClicked(IEnumerable<FleetInfo> fleets);
		
		void SystemOpened(StarSystemController systemController);
		void SystemSelected(StarSystemController systemController);
	}
}
