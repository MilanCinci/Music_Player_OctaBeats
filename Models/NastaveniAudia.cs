using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Models
{
    /// <summary>
    /// Třída sloužící k obecnému nastavení audia
    /// </summary>
    public class NastaveniAudia
    {
        /// <summary>
        /// Konstanta pro definování výchozí hlasitosti 70%
        /// </summary>
        private const float VychoziHlasitost = 0.7f; 

        /// <summary>
        /// Nastavení hlasitosti skladby 
        /// </summary>
        public float Hlasitost { get; set; } = VychoziHlasitost;

        /// <summary>
        /// Bezparametrický konstruktor pro inicializaci
        /// </summary>
        public NastaveniAudia() { }

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="hlasitost">Aktuální hlasitost skladby</param>
        public NastaveniAudia(float hlasitost) 
        {
            Hlasitost = hlasitost;
        }
    }
}
