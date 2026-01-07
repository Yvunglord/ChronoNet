using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ChronoNet.UI.Converters
{
    public class PathConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 ||
                values[0] is not List<Guid> path ||
                !path.Any() ||
                values[1] is not Dictionary<Guid, string> deviceMap)
            {
                return string.Empty;
            }

            return string.Join(" → ", path.Select(id =>
                deviceMap.TryGetValue(id, out var name) ? name : $"Unknown ({id.ToString().Substring(0, 8)})"));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}