﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zvjezdojedac.Igra.Igraci.OsnovniRI
{
	class ORIPlanetarniSloj : ASlojUpravljanja
	{
		public ORIPlanetarniSloj(ORIKoordinator koordinator)
			: base(koordinator)
		{ }

		public override void OdrediZahtjeve()
		{
			// ne radi nista
		}

		public override void Djeluj()
		{
			foreach (var kolonija in igrac.kolonije)
				kolonija.UdioIndustrije = 0;
		}
	}
}
