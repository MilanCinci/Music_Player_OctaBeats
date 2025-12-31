using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
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
    /// Třída sloužící jako conventer pro převádění lokalizace pro XAML Binding
    /// </summary>
    public class LokalizaceConverter : IValueConverter
    {
        public ILokalizaceService LokalizaceService { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
            {
                throw new Exception("Nastala chyba při konvertování lokalizace!");
            }

            return LokalizaceService.Translate(parameter.ToString()!);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
