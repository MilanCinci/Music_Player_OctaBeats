using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using Hudebni_Prehravac_OctaBeats.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
        /// Konstanta pro určení maximálního počtu vláken, které smí vstupit v jednu chvíli do kritické sekce
        /// </summary>
        private const int MaxPocetVlakenVKritSekci = 1;

        /// <summary>
        /// Konstanta pro určení počtu vláken, na který se má semafor inicializovat
        /// </summary>
        private const int PocetVlakenVSemaforu = 1;

        /// <summary>
        /// Seznam historie přehrávání
        /// </summary>
        private ObservableCollection<HistoriePrehravani>? historie;

        /// <summary>
        /// Semafor pro kontrolu vstupu do kritické sekce
        /// </summary>
        private static SemaphoreSlim? fileSemaphore;

        /// <summary>
        /// Bezparametrický konstruktor pro inicializaci
        /// </summary>
        public HistoryService()
        {
            fileSemaphore = new SemaphoreSlim(PocetVlakenVSemaforu, MaxPocetVlakenVKritSekci);
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

            if(fileSemaphore == null)
            {
                throw new NullReferenceException("Semafor nemůže být NULL!");
            }

            historie.Insert(0, new HistoriePrehravani 
            { 
                Song = song, 
                DatumPrehrani = DateTime.Now 
            });

            var snapshot = historie.ToList();

            // Ukládání historie přehrávání na pozadí
            Task.Run(async () =>
            {
                // Čekání než se uvolní přístup k souboru
                await fileSemaphore.WaitAsync();
                try
                {
                    SpravaSouboru.UlozDoSouboru(CestaKSouboru, snapshot);
                }

                catch (Exception ex)
                {
                    SpravaSouboru.LogError(ex, $"Chyba při zápisu historie ve třídě {nameof(HistoryService)}");
                }

                finally
                {
                    fileSemaphore.Release();
                }
            });
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
        /// <param name="data">Data historie, které chceme uložit</param>
        public void Save(List<HistoriePrehravani> data)
        {
            Task.Run(() =>
            {
                try
                {
                    SpravaSouboru.UlozDoSouboru(CestaKSouboru, data);
                }

                catch (Exception ex)
                {
                    SpravaSouboru.LogError(ex, $"Chyba při ukládání historie do souboru ve třídě {nameof(HistoryService)}");
                }
            });
        }
    }
}
