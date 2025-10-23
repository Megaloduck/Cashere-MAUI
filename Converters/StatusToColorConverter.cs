using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Cashere.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "completed" => Color.FromArgb("#27AE60"),
                    "pending" => Color.FromArgb("#F39C12"),
                    "processing" => Color.FromArgb("#3498DB"),
                    "failed" => Color.FromArgb("#E74C3C"),
                    "cancelled" => Color.FromArgb("#95A5A6"),
                    _ => Color.FromArgb("#95A5A6")
                };
            }
            return Color.FromArgb("#95A5A6");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}