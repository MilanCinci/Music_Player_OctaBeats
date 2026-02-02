using System;
using System.Globalization;
using System.Windows.Data;

namespace Hudebni_Prehravac_OctaBeats.Models.Conventers
{
    /// <summary>
    /// Třída sloužící jako conventer pro převádění hlasitosti na konkrétní frame obrázku hlasitosti pro XAML Binding
    /// </summary>
    public class VolumeRangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float vol && parameter is string range)
            {
                return range switch
                {
                    "5" => vol <= 0,
                    "1" => vol > 0 && vol <= 25,          
                    "2" => vol > 25 && vol <= 50, 
                    "3" => vol > 50 && vol <= 75, 
                    "4" => vol > 75,   
                    _ => false
                };
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}