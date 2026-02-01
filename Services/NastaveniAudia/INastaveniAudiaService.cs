using Hudebni_Prehravac_OctaBeats.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Services
{
    /// <summary>
    /// Rozhraní sloužící k definování metod pro obsluhu nastavení audia
    /// </summary>
    public interface INastaveniAudiaService
    {
        /// <summary>
        /// Metoda slouží k načtení uloženého nastavení audia
        /// </summary>
        /// <returns>Vrací nastavení audia</returns>
        Task<NastaveniAudio?> Load();

        /// <summary>
        /// Metoda slouží k uložení aktuálního nastavení audia
        /// </summary>
        /// <param name="nastaveniAudia">Aktuální nastavení audia, které chceme uložit</param>
        Task Save(NastaveniAudio nastaveniAudia);
    }
}
