using Hudebni_Prehravac_OctaBeats.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Services.Historie
{
    /// <summary>
    /// Rozhraní sloužící k definování metod pro obsluhu historie přehrávání
    /// </summary>
    public interface IHistorieService
    {
        /// <summary>
        /// Metoda slouží k přidání skladby do historie přehrávání
        /// </summary>
        /// <param name="song">Přehraná skladba</param>
        /// <returns>Vrací Task</returns>
        Task Add(Song song);

        /// <summary>
        /// Metoda slouží k odstranění konkrétního záznamu historie
        /// </summary>
        /// <param name="historie">Záznam historie, kterou chceme smazat</param>
        /// <returns>Vrací Task</returns>
        Task Delete(HistoriePrehravani historie);

        /// <summary>
        /// Metoda slouží k vymazání celé historie přehrávání
        /// </summary>
        /// <returns>Vrací Task</returns>
        Task ClearAll();

        /// <summary>
        /// Metoda slouží k načtení uložené historie přehrávání
        /// </summary>
        /// <returns>Vrací kolekci načtené historie přehrávání</returns>
        Task<ObservableCollection<HistoriePrehravani>> Load();

        /// <summary>
        /// Metoda slouží k uložení historie přehrávání
        /// </summary>
        /// <returns>Vrací Task</returns>
        Task Save();
    }
}
