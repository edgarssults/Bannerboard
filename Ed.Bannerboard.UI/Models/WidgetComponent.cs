using Microsoft.AspNetCore.Components;

namespace Ed.Bannerboard.UI.Models
{
    /// <summary>
    /// A model for containing a dynamically rendered widget type and instance.
    /// </summary>
    public class WidgetComponent
    {
        public WidgetComponent(Type type, int row, int column, int rowSpan = 1, int columnSpan = 1)
        {
            Type = type;
            Row = row;
            Column = column;
            RowSpan = rowSpan;
            ColumnSpan = columnSpan;
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

        /// <summary>
        /// Widget row starting with 0.
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Widget column starting with 0.
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Number of rows the widget spans.
        /// </summary>
        public int RowSpan { get; set; }

        /// <summary>
        /// Number of columns the widget spans.
        /// </summary>
        public int ColumnSpan { get; set; }
    }
}
