using ChronoNet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ChronoNet.UI.Converters
{
    public class DeviceNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Guid deviceId && System.Windows.Application.Current?.MainWindow?.DataContext is WindowViewModel vm)
            {
                var device = vm.AllDevices.FirstOrDefault(d => d.Id == deviceId);
                return device?.Name ?? deviceId.ToString();
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
