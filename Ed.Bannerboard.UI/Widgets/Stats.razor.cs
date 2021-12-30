using Ed.Bannerboard.UI.Models;
using System.Threading.Tasks;

namespace Ed.Bannerboard.UI.Widgets
{
    public partial class Stats
    {
        private StatsModel statsModel;

        public Task Update(StatsModel model)
        {
            statsModel = model;
            StateHasChanged();

            return Task.CompletedTask;
        }
    }
}
