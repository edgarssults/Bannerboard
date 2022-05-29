using Ed.Bannerboard.Models.Widgets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Ed.Bannerboard.UI.Widgets
{
    public partial class TownProsperity
    {
        private readonly Version _minimumSupportedVersion = new("0.3.2");
        private TownProsperityModel? prosperityModel;

        public override bool CanUpdate(string model, Version? version)
        {
            return Regex.IsMatch(model, $"\"Type\":.*\"{nameof(TownProsperityModel)}\"")
                && IsCompatible(version, _minimumSupportedVersion);
        }

        public override Task Update(string model)
        {
            prosperityModel = JsonConvert.DeserializeObject<TownProsperityModel>(model, new VersionConverter());
            if (prosperityModel == null)
            {
                return Task.CompletedTask;
            }

            StateHasChanged();
            return Task.CompletedTask;
        }
    }
}
