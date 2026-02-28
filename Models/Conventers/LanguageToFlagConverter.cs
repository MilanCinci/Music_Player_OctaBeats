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
    /// Třída sloužící jako conventer pro převádění názvu jazyka na příslušný obrázek vlajky pro XAML Binding
    /// </summary>
    public class LanguageToFlagConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? jazyk = value as string;

            if (String.IsNullOrEmpty(jazyk))
            {
                return null;
            }

            if (jazyk.Contains("Čeština") || jazyk.Contains("cs-CZ"))
            {
                return "pack://application:,,,/Resources/Obrazky/Vlajka_CZ.png";
            }

            if (jazyk.Contains("English") || jazyk.Contains("en-US"))
            {
                return "pack://application:,,,/Resources/Obrazky/Vlajka_US.png";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
