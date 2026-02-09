using DynamicNetwork.Domain.Enums;
using Microsoft.Msagl.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace DynamicNetwork.Presentation.Converters;

public class LinkDirectionToArrowConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is LinkDirection direction)
        {
            return direction switch
            {
                LinkDirection.Undirected => "↔",
                LinkDirection.Right => "→",
                LinkDirection.Left => "←",
                _ => "—"
            };
        }
        else if (value is bool isDirected)
        {
            return isDirected ? "→" : "↔";
        }

        return "—";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}