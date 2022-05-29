using Blazored.LocalStorage;
using Ed.Bannerboard.Models.Widgets;
using Ed.Bannerboard.UI.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Ed.Bannerboard.UI.Widgets
{
    public partial class TownProsperity
    {
        private const string TownCountKey = "prosperity-widget-town-count";
        private readonly Version _minimumSupportedVersion = new("0.3.2");
        private TownProsperityModel? prosperityModel;
        private int townCount = 10;

        [Inject]
        private ILocalStorageService? LocalStorage { get; set; }

        [Inject]
        private IConfiguration? Configuration { get; set; }

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

        public override void SendInitialMessage()
        {
            // Widget is now initialized and message event is subscribed to, request data from the server
            SendFilterMessage();
        }

        protected override async Task OnInitializedAsync()
        {
            var storedTownCount = await LocalStorage!.GetItemAsync<int?>(TownCountKey);
            if (storedTownCount != null)
            {
                townCount = storedTownCount.Value;
            }

            await base.OnInitializedAsync();
        }

        private async Task ProsperityFilterClickedAsync(int count)
        {
            townCount = count;
            await LocalStorage!.SetItemAsync(TownCountKey, townCount);
            SendFilterMessage();
        }

        private void SendFilterMessage()
        {
            var settings = Configuration!.GetSection(nameof(DashboardSettings)).Get<DashboardSettings>();
            var model = new TownProsperityFilterModel
            {
                TownCount = townCount,
                Version = new Version(settings.Version)
            };
            OnMessageSent(JsonConvert.SerializeObject(model));
        }
    }
}
