using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Models
{
    /// <summary>
    /// Třída sloužící k nastavení jazykové verze
    /// </summary>
    public class Language
    {
        /// <summary>
        /// Název jazyka
        /// </summary>
        public required string Nazev { get; set; }

        /// <summary>
        /// Kód daného jazyka (např.: en-US, cs-CZ)
        /// </summary>
        public required string Kod { get; set; }

        /// <summary>
        /// Bezparametrický konstruktor pro inicializace
        /// </summary>
        public Language() { }

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="nazev">Název jazyka</param>
        /// <param name="kod">Kód daného jazyka</param>
        [SetsRequiredMembers]
        public Language(string nazev, string kod)
        {
            Nazev = nazev;
            Kod = kod;
        }
    }
}
