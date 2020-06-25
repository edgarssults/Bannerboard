using System;
using System.Collections.Generic;

namespace Ed.Bannerlord.Dashboard.Models.Widgets
{
    [Serializable]
    public class KingdomStrengthModel : DashboardUpdate
    {
        public Dictionary<string, float> Kingdoms { get; set; }
    }
}
