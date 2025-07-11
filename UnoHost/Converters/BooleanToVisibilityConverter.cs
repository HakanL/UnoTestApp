﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMXCore.DMXCore100;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isVisible)
        {
            if (parameter as string == "Inverse")
            {
                return isVisible ? Visibility.Collapsed : Visibility.Visible;
            }

            // Default behavior (when parameter is not "Inverse")
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
