using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace CarManagement.Converters;

// Converts a car's IsAvailable bool to a green/red brush for the status column
public class AvailabilityToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isAvailable)
            return isAvailable ? Brushes.Green : Brushes.Red;
        return Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Converts a car's IsAvailable bool to "Available"/"Rented" text
public class AvailabilityToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isAvailable)
            return isAvailable ? "Available" : "Rented";
        return "Unknown";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
