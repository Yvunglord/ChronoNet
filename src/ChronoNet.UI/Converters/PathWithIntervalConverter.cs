using ChronoNet.Application.DTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ChronoNet.UI.Converters
{
    public class PathWithIntervalConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return string.Empty;

            if (values[0] is PathWithInterval pathWithInterval)
            {
                if (values[1] is Dictionary<Guid, string> deviceMap)
                {
                    var deviceNames = pathWithInterval.Path
                        .Select(id => deviceMap.TryGetValue(id, out var name) ? name : id.ToString())
                        .ToList();

                    var pathString = string.Join(" → ", deviceNames);
                    var intervalString = $"[{pathWithInterval.Interval.Start} - {pathWithInterval.Interval.End}]";

                    return $"{pathString} {intervalString}";
                }
            }

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}