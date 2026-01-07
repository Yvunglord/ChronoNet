using ChronoNet.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChronoNet.UI.Converters
{
    public class DirectionToArrowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EdgeDirection direction)
            {
                return direction switch
                {
                    EdgeDirection.Undirected => "↔",
                    EdgeDirection.Right => "→",
                    EdgeDirection.Left => "←",
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
}
