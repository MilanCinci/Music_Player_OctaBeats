using Hudebni_Prehravac_OctaBeats.Persistence;
using Hudebni_Prehravac_OctaBeats.Resources.JazykoveVerze;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Services.Lokalizace
{
    /// <summary>
    /// Třída sloužící k implementování rozhraní ILokalizaceService a obsluze daných metod
    /// </summary>
    public class LokalizaceService : ILokalizaceService
    {
        /// <summary>
        /// Obslužná třída pro definování jazykových zdrojů
        /// </summary>
        private readonly ResourceManager _resourceManager = Resources.JazykoveVerze.Strings.ResourceManager;

        /// <summary>
        /// Aktuálně nastavený jazyk aplikace
        /// </summary>
        public CultureInfo AktualniJazyk { get; private set; } = CultureInfo.CurrentUICulture;

        /// <summary>
        /// Metoda slouží ke změně jazyka aplikace
        /// </summary>
        /// <param name="cultureCode">Kód jazyka, na který chceme aplikaci přeložit</param>
        public void ChangeLanguage(string cultureCode)
        {
            try
            {
                AktualniJazyk = new CultureInfo(cultureCode);
                CultureInfo.CurrentUICulture = AktualniJazyk;
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while changing the current language!", nameof(ChangeLanguage));
                throw;
            }
        }

        /// <summary>
        /// Metoda slouží k překladu jednotlivých prvků v aplikaci
        /// </summary>
        /// <param name="key">Klíč prvku, který chceme přeložit</param>
        /// <returns>Vrací přeložený prvek</returns>
        public string Translate(string key)
        {
            try
            {
                return _resourceManager.GetString(key, AktualniJazyk) ?? key;
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while translating a specified key!", nameof(Translate));
                throw;
            }
        }

        /// <summary>
        /// Indexer pro získání správných překladů pomocí vstupního klíče
        /// </summary>
        /// <param name="key">Klíč, podle kterého budeme překládat</param>
        /// <returns>Vrací přeložený text na správný jazyk</returns>
        public string this[string key] => Translate(key);
    }
}
