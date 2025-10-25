using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization; 

namespace Cashere.Converters
{
    public class SearchHighlightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && parameter is string searchTerm)
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return text;

                // This is a simple implementation
                // You could make this more sophisticated with FormattedString
                return text;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
