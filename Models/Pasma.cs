using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Models
{
    /// <summary>
    /// Statická třída pro inicializaci a uchování jednotlivých pásem ekvalizéru
    /// </summary>
    public static class Pasma
    {
        /// <summary>
        /// Pole jednotlivých pásem
        /// </summary>
        public static PasmoEkvalizeru[] PasmaEkvalizeru { get; }

        /// <summary>
        /// Výchozí hodnota zesílení pásem
        /// </summary>
        private const float VychoziHodnotaZesileni = 0f;

        /// <summary>
        /// Statický konstruktor pro inicializaci pole pásem
        /// </summary>
        static Pasma()
        {
            PasmaEkvalizeru = new PasmoEkvalizeru[]
            {
                new ("32Hz", 32f, VychoziHodnotaZesileni),
                new ("64Hz", 64f, VychoziHodnotaZesileni),
                new ("125Hz", 125f, VychoziHodnotaZesileni),
                new ("250Hz", 250f, VychoziHodnotaZesileni),
                new ("500Hz", 500f, VychoziHodnotaZesileni),
                new ("1kHz", 1000f, VychoziHodnotaZesileni),
                new ("2kHz", 2000f, VychoziHodnotaZesileni),
                new ("4kHz", 4000f, VychoziHodnotaZesileni),
                new ("8kHz", 8000f, VychoziHodnotaZesileni),
                new ("16kHz", 16000f, VychoziHodnotaZesileni),
            };
        }
    }
}
