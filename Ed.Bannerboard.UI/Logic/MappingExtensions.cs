using Ed.Bannerboard.UI.Models;

namespace Ed.Bannerboard.UI.Logic
{
    public static class MappingExtensions
    {
        public static List<WidgetLayout> ToWidgetLayout(this List<WidgetComponent> source)
        {
            return source
                .Select(w => new WidgetLayout
                {
                    Type = w.Type.Name,
                    Column = w.Column,
                    Row = w.Row,
                    ColumnSpan = w.ColumnSpan,
                    RowSpan = w.RowSpan
                })
                .ToList();
        }
    }
}
