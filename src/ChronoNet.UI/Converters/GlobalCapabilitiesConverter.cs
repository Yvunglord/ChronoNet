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
    public class GlobalCapabilitiesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GlobalCapabilities capabilities)
            {
                var list = new List<string>();
                if ((capabilities & GlobalCapabilities.Compute) == GlobalCapabilities.Compute)
                    list.Add("Compute");
                if ((capabilities & GlobalCapabilities.Storage) == GlobalCapabilities.Storage)
                    list.Add("Storage");
                if ((capabilities & GlobalCapabilities.Transfer) == GlobalCapabilities.Transfer)
                    list.Add("Transfer");

                return list.Count > 0 ? string.Join(", ", list) : "None";
            }
            return "None";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
