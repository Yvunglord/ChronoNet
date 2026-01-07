using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChronoNet.UI.Converters
{
    public class LocalCapabilitiesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 5)
            {
                var parts = new List<string>();

                if (values[0] is bool compute && compute)
                    parts.Add("✓ Compute");
                if (values[1] is bool storage && storage)
                    parts.Add("✓ Storage");
                if (values[2] is bool transfer && transfer)
                    parts.Add("✓ Transfer");
                if (values[3] is bool canSend && canSend)
                    parts.Add("✓ CanSend");
                if (values[4] is bool canReceive && canReceive)
                    parts.Add("✓ CanReceive");

                if (parts.Count == 0)
                    return "Нет активных локальных возможностей";

                return string.Join(Environment.NewLine, parts);
            }
            return "Нет активных локальных возможностей";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
