using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Cashere.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string colorPair)
            {
                var colors = colorPair.Split('|');
                if (colors.Length == 2)
                {
                    var selectedColor = colors[0].Trim();
                    var unselectedColor = colors[1].Trim();

                    // Check if using dynamic resources
                    if (selectedColor.StartsWith("Dynamic:"))
                    {
                        var resourceKey = selectedColor.Replace("Dynamic:", "");
                        selectedColor = GetDynamicResource(resourceKey);
                    }

                    if (unselectedColor.StartsWith("Dynamic:"))
                    {
                        var resourceKey = unselectedColor.Replace("Dynamic:", "");
                        unselectedColor = GetDynamicResource(resourceKey);
                    }

                    return Color.FromArgb(boolValue ? selectedColor : unselectedColor);
                }
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetDynamicResource(string key)
        {
            if (Application.Current.Resources.TryGetValue(key, out var resource))
            {
                if (resource is Color color)
                {
                    return color.ToArgbHex();
                }
            }
            return "#000000"; // Fallback
        }
    }
}