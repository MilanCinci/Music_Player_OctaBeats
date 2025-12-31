using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Services.Lokalizace
{
    /// <summary>
    /// Rozhraní sloužící k definování metod pro obsluhu lokalizace aplikace
    /// </summary>
    public interface ILokalizaceService
    {
        /// <summary>
        /// Aktuálně nastavený jazyk aplikace
        /// </summary>
        CultureInfo AktualniJazyk { get; }

        /// <summary>
        /// Metoda slouží ke změně jazyka aplikace
        /// </summary>
        /// <param name="cultureCode">Kód jazyka, na který chceme aplikaci přeložit</param>
        void ChangeLanguage(string cultureCode);

        /// <summary>
        /// Metoda slouží k překladu jednotlivých prvků v aplikaci
        /// </summary>
        /// <param name="key">Klíč prvku, který chceme přeložit</param>
        /// <returns>Vrací přeložený prvek</returns>
        string Translate(string key);
    }
}
