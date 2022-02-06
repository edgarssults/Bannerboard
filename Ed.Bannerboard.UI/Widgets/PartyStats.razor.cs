using Ed.Bannerboard.Models.Widgets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Ed.Bannerboard.UI.Widgets
{
    public partial class PartyStats
    {
        private readonly Version _minimumSupportedVersion = new("0.3.1");
        private PartyStatsModel? partyModel;

        public override bool CanUpdate(string model, Version? version)
        {
            return Regex.IsMatch(model, $"\"Type\":.*\"{nameof(PartyStatsModel)}\"")
                && IsCompatible(version, _minimumSupportedVersion);
        }

        public override Task Update(string model)
        {
            partyModel = JsonConvert.DeserializeObject<PartyStatsModel>(model, new VersionConverter());
            if (partyModel == null)
            {
                return Task.CompletedTask;
            }

            StateHasChanged();
            return Task.CompletedTask;
        }

        private static string GetMemberIcon(MemberStatsItem member)
        {
            return member switch
            {
                { IsInfantry: true } => "infantry",
                { IsArcher: true } => "archer",
                { IsCavalry: true} => "cavalry",
                { IsMountedArcher: true } => "mounted-archer",
                _ => "infantry"
            };
        }
    }
}
