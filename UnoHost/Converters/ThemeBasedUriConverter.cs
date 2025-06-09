using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using DMXCore.DMXCore100.Extensions;

namespace DMXCore.DMXCore100;

public class ThemeBasedUriConverter : IValueConverter
{
    internal static bool IsDarkMode { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string baseUri)
        {
            return IconHelper.GetThemeBasedIcon(baseUri, IsDarkMode);
        }

        return value; // Fallback for invalid input
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
