using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Services.Historie
{
    public class HistoryService : IHistorieService
    {
        /// <summary>
        /// Cesta k souboru s historií přehrávání
        /// </summary>
        private static readonly string CestaKSouboru = Environment.ExpandEnvironmentVariables(@"%AppData%\OctaBeats\DataFiles\historie.json");

        /// <summary>
        /// Semafor pro řízení přístupu k souboru historie
        /// </summary>
        private readonly SemaphoreSlim _fileSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Limit, kolik může být v historii skladeb
        /// </summary>
        private const int LimitHistorie = 65;

        /// <summary>
        /// Seznam historie přehrávání
        /// </summary>
        public ObservableCollection<HistoriePrehravani> MojeHistorie { get; } = new ObservableCollection<HistoriePrehravani>();

        /// <summary>
        /// Metoda slouží k načtení uložené historie přehrávání
        /// </summary>
        /// <returns>Vrací kolekci načtené historie přehrávání</returns>
        public async Task<ObservableCollection<HistoriePrehravani>> Load()
        {
            try
            {
                var nactenaData = await SpravaSouboru.NahrajZeSouboru<List<HistoriePrehravani>>(CestaKSouboru);

                if (nactenaData != null)
                {
                    MojeHistorie.Clear();
                    foreach (HistoriePrehravani polozka in nactenaData)
                    {
                        MojeHistorie.Add(polozka);
                    }
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while loading the playback history!", nameof(Load));
                throw;
            }

            return MojeHistorie;
        }

        /// <summary>
        /// Metoda slouží k přidání skladby do historie přehrávání
        /// </summary>
        /// <param name="song">Přehraná skladba</param>
        /// <returns>Vrací Task</returns>
        public async Task Add(Song song)
        {
            if (song == null)
            {
                return;
            }

            try
            {
                HistoriePrehravani novyZaznam = new HistoriePrehravani(song, DateTime.Now);

                // Počkání na vykonání Insert na UI vlákně, kvůli zasahování do kolekce vytvořené na UI vlákně
                App.Current.Dispatcher.Invoke(() =>
                {
                    // Přidání nového záznamu historie na začátek seznamu
                    MojeHistorie.Insert(0, novyZaznam);

                    // Pokud se přesáhne limit, začnou se mazat skladby od konce
                    if (MojeHistorie.Count > LimitHistorie)
                    {
                        MojeHistorie.RemoveAt(MojeHistorie.Count - 1);
                    }
                });

                // Uložení kopie historie na pozadí, aby se zachovala konzistence vláken
                List<HistoriePrehravani> copy = MojeHistorie.ToList();
                await Task.Run(() => SaveCopy(copy));
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while adding the song to the playback history!", nameof(Add));
                throw;
            }
        }

        /// <summary>
        /// Metoda slouží k odstranění konkrétního záznamu historie
        /// </summary>
        /// <param name="historie">Záznam historie, který chceme smazat</param>
        /// <returns>Vrací Task</returns>
        public async Task Delete(HistoriePrehravani historie)
        {
            if(historie == null)
            {
                return;
            }

            try
            {
                bool byloOdstraneno = false;

                // Počkání na vykonání Remove na UI vlákně, kvůli zasahování do kolekce vytvořené na UI vlákně
                App.Current.Dispatcher.Invoke(() =>
                {
                    byloOdstraneno = MojeHistorie.Remove(historie);
                });

                if (byloOdstraneno)
                {
                    await Save();
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while removing the selected song from the playback history!", nameof(Delete));
                throw;
            }
        } 

        /// <summary>
        /// Metoda slouží k vymazání celé historie přehrávání
        /// </summary>
        /// <returns>Vrací Task</returns>
        public async Task ClearAll()
        {
            try
            {
                MojeHistorie.Clear();
                await Save();
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while clearing the playback history!", nameof(ClearAll));
                throw;
            }
        }
      
        /// <summary>
        /// Metoda slouží k uložení historie přehrávání
        /// </summary>
        /// <returns>Vrací Task</returns>
        public async Task Save()
        {
            try
            {
                await SaveCopy(MojeHistorie.ToList());
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while saving the playback history!", nameof(Save));
                throw;
            }
        }

        /// <summary>
        /// Metoda slouží k asynchronnímu uložení kopie historie
        /// </summary>
        /// <param name="data">Historie, kterou chceme uložit</param>
        /// <returns>Vrací Task</returns>
        private async Task SaveCopy(List<HistoriePrehravani> data)
        {
            // Zapsání a kontrola přístupu při zápisu do konkrétního souboru
            await _fileSemaphore.WaitAsync();

            try
            {
                string? adresar = Path.GetDirectoryName(CestaKSouboru);
                if (adresar != null && !Directory.Exists(adresar))
                {
                    Directory.CreateDirectory(adresar!);
                }

                await SpravaSouboru.UlozDoSouboru(CestaKSouboru, data);
            }

            catch(Exception)
            {
                throw;
            }

            finally
            {
                _fileSemaphore.Release();
            }
        }
    }
}