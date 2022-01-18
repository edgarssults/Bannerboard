using Blazorise.Charts;
using Microsoft.AspNetCore.Components;

namespace Ed.Bannerboard.UI.Logic
{
    public class WidgetBase : ComponentBase, IWidget
    {
        /// <summary>
        /// Default bar chart options.
        /// </summary>
        protected object BaseBarChartOptions = new
        {
            Animation = new Animation
            {
                Duration = 0,
            },
            Legend = new Legend
            {
                Display = false,
                // Has to be set, otherwise there's a JS error
                Labels = new LegendLabels
                {
                    FontSize = 12,
                    FontFamily = "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif"
                }
            },
            Scales = new
            {
                YAxes = new[]
                {
                    new
                    {
                        Ticks = new
                        {
                            BeginAtZero = true,
                            Min = 0
                        }
                    }
                }
            },
            Responsive = true, // Default
            MaintainAspectRatio = false // So that widgets fill the allocated space
        };

        public virtual Task Update(string model)
        {
            // Should be overridden
            return Task.CompletedTask;
        }

        public virtual bool CanUpdate(string model, Version? version)
        {
            // Should be overridden
            return false;
        }

        /// <summary>
        /// Determines whether the mod version is compatible with the minimum supported version by the vidget.
        /// </summary>
        /// <param name="version">The mod version.</param>
        /// <param name="minimumSupportedVersion">Minimum supported version by the widget.</param>
        public bool IsCompatible(Version? version, Version minimumSupportedVersion)
        {
            // If we don't know the current mod version yet, don't process the update
            if (version == null)
            {
                return false;
            }

            // Current mod version should be the same or bigger than the minimum supported version
            return version.CompareTo(minimumSupportedVersion) >= 0;
        }
    }
}
