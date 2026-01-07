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
    public class GlobalCapabilitiesToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GlobalCapabilities capabilities)
            {
                var parts = new List<string>();

                if ((capabilities & GlobalCapabilities.Compute) == GlobalCapabilities.Compute)
                    parts.Add("✓ Compute");
                if ((capabilities & GlobalCapabilities.Storage) == GlobalCapabilities.Storage)
                    parts.Add("✓ Storage");
                if ((capabilities & GlobalCapabilities.Transfer) == GlobalCapabilities.Transfer)
                    parts.Add("✓ Transfer");

                if (parts.Count == 0)
                    return "Нет активных возможностей";

                return string.Join(Environment.NewLine, parts);
            }
            return "Нет активных возможностей";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
