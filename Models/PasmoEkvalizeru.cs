using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Models
{
    /// <summary>
    /// Třída sloužící k definování informací ohledně pásma ekvalizéru
    /// </summary>
    public class PasmoEkvalizeru
    {
        /// <summary>
        /// Název pásma
        /// </summary>
        public string Nazev { get; set; }
        
        /// <summary>
        /// Frekvence pásma
        /// </summary>
        public float Frekvence { get; set; }

        /// <summary>
        /// Celkové zesílení pásma
        /// </summary>
        public float Zesileni {  get; set; }

        /// <summary>
        /// Šířka pásma (Q faktor)
        /// </summary>
        public float SirkaPasma { get; set; } = 0.8f;

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="nazev">Název pásma</param>
        /// <param name="frekvence">Frekvence pásma</param>
        /// <param name="zesileni">Zesílení pásma</param>
        /// <param name="sirkaPasma">Šířka pásma</param>
        public PasmoEkvalizeru(string nazev, float frekvence, float zesileni, float sirkaPasma = 0.8f)
        {
            Nazev = nazev;
            Frekvence = frekvence;
            Zesileni = zesileni;
            SirkaPasma = sirkaPasma;
        }
    }
}
