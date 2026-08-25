using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Models
{
    /// <summary>
    /// Třída sloužící k obecnému nastavení ekvalizéru
    /// </summary>
    public class NastaveniEkvalizer
    {
        /// <summary>
        /// Cesta k souboru s nastavením ekvalizéru
        /// </summary>
        public static string CestaKSouboru = Environment.ExpandEnvironmentVariables(@"%AppData%\OctaBeats\DataFiles\nastaveniEkvalizer.json");

        /// <summary>
        /// Indikátor, zda je povolený ekvalizér či ne
        /// </summary>
        public bool JeEkvalizerPovoleny { get; set; }
        
        /// <summary>
        /// Jednotlivá pásma ekvalizéru
        /// </summary>
        public List<PasmoEkvalizeru> PasmaEkvalizeru { get; set; } = new List<PasmoEkvalizeru>();

        /// <summary>
        /// Konkrétní typ přednastavení (presetu) ekvalizéru
        /// </summary>
        public TypPrednastaveni TypPrednastaveni { get; set; }

        /// <summary>
        /// Bezparametrický konstruktor pro inicializaci
        /// </summary>
        public NastaveniEkvalizer() { }

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="jeEkvalizerPovoleny">Indikátor, zda je povolený ekvalizér či ne</param>
        /// <param name="pasmaEkvalizeru">Jednotlivá pásma ekvalizéru</param>
        /// <param name="typPrednastaveni">Konkrétní typ přednastavení (presetu)</param>
        public NastaveniEkvalizer(bool jeEkvalizerPovoleny, List<PasmoEkvalizeru> pasmaEkvalizeru, TypPrednastaveni typPrednastaveni)
        {
            JeEkvalizerPovoleny = jeEkvalizerPovoleny;
            PasmaEkvalizeru = pasmaEkvalizeru;
            TypPrednastaveni = typPrednastaveni;
        }
    }
}
