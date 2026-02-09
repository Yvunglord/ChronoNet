using DynamicNetwork.Domain.Flows;
using System.Globalization;
using System.Windows.Data;

namespace DynamicNetwork.Presentation.Converters;

public class TransformationsToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IReadOnlyList<FlowTransformation> transformations)
        {
            var result = string.Join("; ",
                transformations.Select(t => $"{t.InputType}→{t.OutputType}"));

            if (string.IsNullOrEmpty(result))
                return "Без преобразований";

            return $"Преобразования: {result}";
        }
        return "Без преобразований";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
