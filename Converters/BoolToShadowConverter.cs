using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Globalization;

namespace Cashere.Converters
{
    public class BoolToShadowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = value is bool b && b;
            if (isSelected)
            {
                return new Shadow
                {
                    Brush = new SolidColorBrush(Color.FromArgb("#2196F3")), // Primary color glow
                    Offset = new Point(0, 4),
                    Radius = 12,
                    Opacity = 0.6f
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
}
