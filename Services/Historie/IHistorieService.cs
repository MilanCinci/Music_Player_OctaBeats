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
        Task Add(Song song);

        /// <summary>
        /// Metoda slouží k načtení uložené historie přehrávání
        /// </summary>
        /// <returns>Vrací kolekci načtené historie přehrávání</returns>
        Task<ObservableCollection<HistoriePrehravani>>? Load();

        /// <summary>
        /// Metoda slouží k uložení historie přehrávání
        /// </summary>
        Task Save();
    }
}
