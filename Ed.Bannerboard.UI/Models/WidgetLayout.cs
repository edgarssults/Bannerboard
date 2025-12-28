namespace Ed.Bannerboard.UI.Models
{
    public class WidgetLayout
    {
        /// <summary>
        /// Widget component type name.
        /// </summary>
        public string? Type { get; set; }

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

		/// <summary>
		/// Indicates whether the widget is visible.
		/// </summary>
		public bool IsVisible { get; set; } = true; // True for backwards compatibility
	}
}
