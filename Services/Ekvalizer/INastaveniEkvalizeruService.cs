using Hudebni_Prehravac_OctaBeats.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Services.Ekvalizer
{
    /// <summary>
    /// Rozhraní sloužící k definování metod pro obsluhu nastavení ekvalizéru
    /// </summary>
    public interface INastaveniEkvalizeruService
    {
        /// <summary>
        /// Metoda slouží k načtení uloženého nastavení ekvalizéru
        /// </summary>
        /// <returns>Vrací nastavení ekvalizéru</returns>
        Task<NastaveniEkvalizer?> Load();

        /// <summary>
        /// Metoda slouží k uložení aktuálního nastavení ekvalizéru
        /// </summary>
        /// <param name="nastaveniEkvalizer">Aktuální nastavení ekvalizéru, které chceme uložit</param>
        /// <returns>Vrací Task</returns>
        Task Save(NastaveniEkvalizer nastaveniEkvalizer);
    }
}
