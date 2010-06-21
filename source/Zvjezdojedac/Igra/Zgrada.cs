﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Prototip
{
	public class Zgrada : IPohranjivoSB
	{
		public const string BrojZgrada = "BR_ZGRADA";

		#region Ucinci

		public abstract class Ucinak
		{
			protected Formula intenzitet;

			public abstract void djeluj(Kolonija kolonija, Dictionary<string, double> varijable);

			public abstract Ucinak napraviSe(string[] parametri);

			private static Dictionary<string, Ucinak> prototipovi = null;

			public static Ucinak napraviUcinak(string tip)
			{
				if (prototipovi == null)
				{
					prototipovi = new Dictionary<string, Ucinak>();
					prototipovi.Add("SET_VAR", new UcinakSetVar());
				}

				string[] parametriStr = tip.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < parametriStr.Length; i++)
					parametriStr[i] = parametriStr[i].Trim();

				return prototipovi[parametriStr[0]].napraviSe(parametriStr);
			}
		}

		public class UcinakSetVar : Ucinak
		{
			private string varijabla;
			private Formula formula;

			public UcinakSetVar()
			{}

			private UcinakSetVar(string varijabla, Formula formula)
			{
				this.varijabla = varijabla;
				this.formula = formula;
			}

			public override Ucinak napraviSe(string[] parametri)
			{
				return new UcinakSetVar(parametri[1], Formula.IzStringa(parametri[2]));
			}

			public override void djeluj(Kolonija kolonija, Dictionary<string, double> varijable)
			{
				kolonija.efekti[varijabla] = formula.iznos(varijable);
			}
		}
		#endregion

		public class ZgradaInfo : IIdentifiable
		{
			public string ime { get; private set; }
			public int id { get; private set; }

			public Formula cijenaGradnje { get; private set; }
			public Formula dopustenaKolicina { get; private set; }
			public Formula cijenaOdrzavanja { get; private set; }
			public List<Tehnologija.Preduvjet> preduvjeti { get; private set; }

			public Image slika { get; private set; }

			public string kod { get; private set; }

			public string opis { get; private set; }

			public List<Ucinak> ucinci { get; private set; }

			public bool orbitalna { get; private set; }
			public bool ostaje { get; private set; }
			public bool ponavljaSe { get; private set; }

			public ZgradaInfo(int id, string ime, Formula cijenaGradnje, Formula dopustenaKolicina,
				Formula cijenaOdrzavanja, Image slika, string kod, string opis,
				List<Ucinak> ucinci, string svojstva, List<Tehnologija.Preduvjet> preduvjeti)
			{
				this.id = id;
				this.ime = ime;
				this.cijenaGradnje = cijenaGradnje;
				this.dopustenaKolicina = dopustenaKolicina;
				this.cijenaOdrzavanja = cijenaOdrzavanja;
				this.slika = slika;
				this.kod = kod;
				this.opis = opis;
				this.ucinci = ucinci;
				this.preduvjeti = preduvjeti;

				HashSet<string> svojstvaSet = new HashSet<string>();
				string[] svojstvaArray = svojstva.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
				foreach (string s in svojstvaArray)
					svojstvaSet.Add(s.Trim());

				if (svojstvaSet.Contains("ORBITALNA")) orbitalna = true; else orbitalna = false;
				if (!svojstvaSet.Contains("NE_OSTAJE")) ostaje = true; else ostaje = false;
				if (svojstvaSet.Contains("PONAVLJA_SE")) ponavljaSe = true; else ponavljaSe = false;
			}

			public override string ToString()
			{
				return ime;
			}
		}

		#region Statičko
		public static List<ZgradaInfo> civilneZgradeInfo = new List<ZgradaInfo>();
		public static List<ZgradaInfo> vojneZgradeInfo = new List<ZgradaInfo>();
		public static Dictionary<int, ZgradaInfo> ZgradaInfoID = new Dictionary<int, ZgradaInfo>();

		public static void ucitajInfoZgrade(Dictionary<string, string> podaci, bool jeLiCivilna)
		{
			List<Ucinak> ucinci = new List<Ucinak>();
			for(int i = 0; podaci.ContainsKey("UCINAK" + i); i++)
				ucinci.Add(Ucinak.napraviUcinak(podaci["UCINAK" + i]));
			List<Tehnologija.Preduvjet> preduvjeti = Tehnologija.Preduvjet.NaciniPreduvjete(podaci["PREDUVJETI"]);
			
			List<ZgradaInfo> popis = null;
			if (jeLiCivilna)
				popis = civilneZgradeInfo;
			else
				popis = vojneZgradeInfo;

			ZgradaInfo zgradaInfo = new ZgradaInfo(
				SlijedeciId(),
				podaci["IME"],
				Formula.IzStringa(podaci["CIJENA"]),
				Formula.IzStringa(podaci["KOLICINA"]),
				Formula.IzStringa(podaci["ODRZAVANJE"]),
				Image.FromFile(podaci["SLIKA"]),
				podaci["KOD"],
				podaci["OPIS"],
				ucinci,
				podaci["SVOJSTVA"],
				preduvjeti);
			
			popis.Add(zgradaInfo);
			ZgradaInfoID.Add(zgradaInfo.id, zgradaInfo);
		}

		private static int _SlijedeciId = 0;
		public static int SlijedeciId()
		{
			return ++_SlijedeciId;
		}
		#endregion

		public ZgradaInfo tip;

		public Zgrada(ZgradaInfo tip)
		{
			this.tip = tip;
		}

		public void djeluj(Kolonija kolonija, Dictionary<string, double> varijable)
		{
			foreach (Ucinak u in tip.ucinci)
				u.djeluj(kolonija, varijable);
		}

		public override string ToString()
		{
			return tip.ime;
		}

		#region Pohrana
		public const string PohranaTip = "ZGRADA";
		public const string PohTip = "TIP";

		public void pohrani(PodaciPisac izlaz)
		{
			izlaz.dodaj(PohTip, tip);
		}

		public static Zgrada Ucitaj(PodaciCitac ulaz)
		{
			return new Zgrada(ZgradaInfoID[ulaz.podatakInt(PohTip)]);
		}
		#endregion
	}
}
