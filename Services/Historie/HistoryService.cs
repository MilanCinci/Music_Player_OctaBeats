using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Hudebni_Prehravac_OctaBeats.Services.Historie
{
    /// <summary>
    /// Třída sloužící k implementování rozhraní IHistorieService a obsluze daných metod
    /// </summary>
    public class HistoryService : IHistorieService
    {
        /// <summary>
        /// Cesta k JSON souboru s historií
        /// </summary>
        private static string CestaKSouboru = Environment.ExpandEnvironmentVariables(@"%AppData%\OctaBeats\DataFiles\historie.json");

        /// <summary>
        /// Seznam historie přehrávání
        /// </summary>
        private ObservableCollection<HistoriePrehravani>? historie;

        /// <summary>
        /// Bezparametrický konstruktor pro inicializaci
        /// </summary>
        public HistoryService()
        {
            historie = SpravaSouboru.NahrajZeSouboru<ObservableCollection<HistoriePrehravani>>(CestaKSouboru);
        }

        /// <summary>
        /// Metoda slouží k přidání skladby do historie přehrávání
        /// </summary>
        /// <param name="song">Přehraná skladba</param>
        public void Add(Song song)
        {

            if(historie == null)
            {
                throw new NullReferenceException("Historie přehrávání nemůže být NULL!");
            }

            // Vložení nejnověji přehrané skladby na začátek historie
            historie.Insert(0, new HistoriePrehravani
            {
                Song = song,
                DatumPrehrani = DateTime.Now
            });

            Save();
        }

        /// <summary>
        /// Metoda slouží k načtení uložené historie přehrávání
        /// </summary>
        /// <returns>Vrací kolekci načtené historie přehrávání</returns>
        public ObservableCollection<HistoriePrehravani>? Load()
        {
            return historie;
        }

        /// <summary>
        /// Metoda slouží k uložení historie přehrávání
        /// </summary>
        public void Save()
        {
            SpravaSouboru.UlozDoSouboru(CestaKSouboru, historie);
        }
    }
}
