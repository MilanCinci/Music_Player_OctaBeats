using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Hudebni_Prehravac_OctaBeats.Models.Conventers
{
    /// <summary>
    /// Třída sloužící jako conventer pro převádění NULL hodnot na Bool pro XAML
    /// </summary>
    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;

            // Pokud v XAML je ConverterParameter=Inverse, logika se otočí
            if (parameter?.ToString() == "Inverse")
            {
                return !isNull;
            }

            return isNull;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
