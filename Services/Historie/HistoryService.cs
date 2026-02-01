using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Services.Historie
{
    public class HistoryService : IHistorieService
    {
        private static readonly string CestaKSouboru =
            Environment.ExpandEnvironmentVariables(@"%AppData%\OctaBeats\DataFiles\historie.json");

        private readonly SemaphoreSlim _fileSemaphore = new SemaphoreSlim(1, 1);

        private ObservableCollection<HistoriePrehravani> historie = new ObservableCollection<HistoriePrehravani>();

        /// <summary>
        /// Metoda slouží k načtení uložené historie přehrávání
        /// </summary>
        /// <returns>Vrací kolekci načtené historie přehrávání</returns>
        public async Task<ObservableCollection<HistoriePrehravani>> Load()
        {
            historie = await SpravaSouboru
                .NahrajZeSouboru<ObservableCollection<HistoriePrehravani>>(CestaKSouboru) ?? new ObservableCollection<HistoriePrehravani>();

            return historie;
        }

        /// <summary>
        /// Metoda slouží k přidání skladby do historie přehrávání
        /// </summary>
        /// <param name="song">Přehraná skladba</param>
        public async Task Add(Song song)
        {
            if (song == null)
                throw new ArgumentNullException(nameof(song));

            historie.Insert(0, new HistoriePrehravani
            {
                Song = song,
                DatumPrehrani = DateTime.Now
            });

            await SaveInternal();
        }

        /// <summary>
        /// Metoda slouží k uložení historie přehrávání
        /// </summary>
        public async Task Save()
        {
            await SaveInternal();
        }

        /// <summary>
        /// Pomocná metoda pro asynchronní uložení historie
        /// </summary>
        /// <returns>Vrací task</returns>
        private async Task SaveInternal()
        {
            await _fileSemaphore.WaitAsync();

            try
            {
                var snapshot = historie.ToList();
                await SpravaSouboru.UlozDoSouboru(CestaKSouboru, snapshot);
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, $"Chyba při zápisu historie ve třídě {nameof(HistoryService)}");
            }

            finally
            {
                _fileSemaphore.Release();
            }
        }
    }
}
