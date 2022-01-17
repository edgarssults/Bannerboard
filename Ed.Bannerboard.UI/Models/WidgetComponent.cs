using Microsoft.AspNetCore.Components;

namespace Ed.Bannerboard.UI.Models
{
    /// <summary>
    /// A model for containing a dynamically rendered widget type and instance.
    /// </summary>
    public class WidgetComponent
    {
        public WidgetComponent(Type type)
        {
            Type = type;
        }
        
        /// <summary>
        /// Widget component type.
        /// </summary>
        /// <remarks>
        /// Expecting an implementation of <see cref="IWidget"/>.
        /// </remarks>
        public Type Type { get; private set; }

        /// <summary>
        /// Widget component instance.
        /// </summary>
        public DynamicComponent? Component { get; set; }
    }
}
