using System;
using System.Collections.Generic;

namespace Ed.Bannerboard.Models.Widgets
{
    [Serializable]
    public class KingdomWarsModel
    {
        public List<KingdomWarsItem> Kingdoms { get; set; }
    }

    [Serializable]
    public class KingdomWarsItem
    {
        public string Name { get; set; }

        public List<string> Wars { get; set; }

        public string PrimaryColor { get; set; }

        public string SecondaryColor { get; set; }
    }
}
