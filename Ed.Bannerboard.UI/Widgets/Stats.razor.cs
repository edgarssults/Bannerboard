using Blazored.LocalStorage;
using Ed.Bannerboard.UI.Logic;
using Ed.Bannerboard.UI.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Ed.Bannerboard.UI.Widgets
{
    public partial class Stats
    {
        private StatsModel? statsModel;

        [Inject]
        private AppState? AppState { get; set; }

        public override bool CanUpdate(string model, Version? version)
        {
            return Regex.IsMatch(model, $"\"Type\":.*\"{nameof(StatsModel)}\"");
        }

        public override Task Update(string model)
        {
            statsModel = JsonConvert.DeserializeObject<StatsModel>(model, new VersionConverter());
            StateHasChanged();

            return Task.CompletedTask;
        }

        private void ResetLayoutClicked()
        {
            AppState?.NotifyResetLayout();
        }
    }
}
