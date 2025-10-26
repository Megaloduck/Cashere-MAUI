using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Cashere.Converters
{
    public class PaymentMethodToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string paymentMethod)
            {
                return paymentMethod.ToLower() switch
                {
                    "cash" => Color.FromArgb("#667eea"),
                    "qris" => Color.FromArgb("#f093fb"),
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
