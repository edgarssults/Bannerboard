using Ed.Bannerboard.UI.Models;
using System;
using System.Threading.Tasks;

namespace Ed.Bannerboard.UI.Widgets
{
    public partial class Stats
    {
        private readonly Version _minimumSupportedVersion = new("0.1.0");
        private StatsModel statsModel;

        public override bool CanUpdate(object model, Version version)
        {
            return IsCompatible(version, _minimumSupportedVersion) && model is StatsModel;
        }

        public override Task Update(object model)
        {
            statsModel = model as StatsModel;
            StateHasChanged();

            return Task.CompletedTask;
        }
    }
}
