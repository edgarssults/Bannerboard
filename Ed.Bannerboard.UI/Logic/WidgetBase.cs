using Blazorise.Charts;
using Microsoft.AspNetCore.Components;

namespace Ed.Bannerboard.UI.Logic
{
    public class WidgetBase : ComponentBase, IWidget
    {
        /// <summary>
        /// Default bar chart options.
        /// </summary>
        protected BarChartOptions BaseBarChartOptions = new()
        {
            Animation = new()
            {
                Duration = 0, // So that updates are immediate
            },
            Plugins = new()
            {
                Legend = new()
                {
                    Display = false // Legend doesn't work when each bar has its own color
                }
            },
            Scales = new()
            {
                Y = new()
                {
                    BeginAtZero = true, // So that data looks consistent
                    Min = 0 // So that data looks consistent
                }
            },
            Responsive = true, // Default, so that canvas resizes together with the container
            MaintainAspectRatio = false // So that canvas fills the whole container
        };

        public event EventHandler<string>? MessageSent;

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

        public virtual void SendInitialMessage()
        {
            // Should be overridden if necessary
        }

        /// <summary>
        /// Invokes the message sent event with the specified message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        protected virtual void OnMessageSent(string message)
        {
            MessageSent?.Invoke(this, message);
        }

        /// <summary>
        /// Determines whether the mod version is compatible with the minimum supported version by the widget.
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
