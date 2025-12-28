namespace Ed.Bannerboard.Simulator.Models
{
	public class SimulatedKingdom
	{
		public SimulatedKingdom()
		{
		}

		public SimulatedKingdom(string name, string primaryColor, string secondaryColor)
		{
			Name = name;
			PrimaryColor = primaryColor;
			SecondaryColor = secondaryColor;
		}

		public string Name { get; set; }

		public string PrimaryColor { get; set; }

		public string SecondaryColor { get; set; }
	}
}
