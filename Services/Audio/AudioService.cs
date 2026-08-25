using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using Hudebni_Prehravac_OctaBeats.Services.Ekvalizer;
using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
using Hudebni_Prehravac_OctaBeats.ViewModels;
using NAudio.Wave;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace Hudebni_Prehravac_OctaBeats.Services.Audio
{
    /// <summary>
    /// Třída sloužící k implementování rozhraní IAudioService a obsluze daných metod
    /// </summary>
    public class AudioService : IAudioService
    {
        private readonly ILokalizaceService _lokalizaceService;

        /// <summary>
        /// Výstupní audio zařízení zajišťující samotné přehrávání zvuku
        /// </summary>
        private IWavePlayer? output;

        /// <summary>
        /// Čtečka audio souboru poskytující stream dat pro přehrávání
        /// </summary>
        private AudioFileReader? reader;

        /// <summary>
        /// Vzorkování pro dynamickou změnu jednotlivých pásem ekvalizéru
        /// </summary>
        private EqualizerSampleProvider? ekvalizer;

        /// <summary>
        /// Určuje, zda je přehrávání aktuálně pozastaveno
        /// </summary>
        private bool isPaused;

        /// <summary>
        /// Určuje, zda bylo pozastavení přehrávání vyvoláno manuálně uživatelem
        /// </summary>
        private bool manualStop;

        /// <summary>
        /// Cesta k aktuálně přehrávanému souboru
        /// </summary>
        private string? currentFilePath;

        /// <summary>
        /// Mutex používaný pro zajištění thread-safe přístupu k audio zdrojům a přehrávači
        /// </summary>
        private readonly object _audioLock = new object();       

        /// <summary>
        /// Aktuální hlasitost skladby
        /// </summary>
        public float Hlasitost
        {
            get => reader?.Volume ?? 0.7f;
            set
            {
                if (reader != null)
                {
                    reader.Volume = value;
                }
            }
        }

        /// <summary>
        /// Určuje, zda je ekvalizér zapnutý
        /// </summary>
        private bool jeEkvalizerZapnuty = true;
        public bool JeEkvalizerZapnuty
        {
            get => jeEkvalizerZapnuty;
            set
            {
                jeEkvalizerZapnuty = value;

                // Pokud je ekvalizér povolený, tak se předají aktuálně nastavená pásma ekvalizéru
                if(ekvalizer != null)
                {
                    ekvalizer.IsEnabled = JeEkvalizerZapnuty;
                    if(JeEkvalizerZapnuty && AktualniPasma != null && AktualniPasma.Count > 0)
                    {
                        UpdateEqualizer(AktualniPasma);
                    }
                }
            }
        }

        /// <summary>
        /// Aktuální čas přehrávání skladby
        /// </summary>
        public TimeSpan AktualniCas => reader?.CurrentTime ?? TimeSpan.Zero;

        /// <summary>
        /// Celkový čas trvání celé skladby
        /// </summary>
        public TimeSpan CelkovyCas => reader?.TotalTime ?? TimeSpan.Zero;

        /// <summary>
        /// Aktuálně nastavená pásma ekvalizéru
        /// </summary>
        public ObservableCollection<PasmoEkvalizeru>? AktualniPasma { get; set; }

        /// <summary>
        /// Událost ukončení skladby
        /// </summary>
        public event Action? UkonceniSkladby;

        /// <summary>
        /// Událost nenalezení souboru skladby
        /// </summary>
        public event Action<string>? SouborNenalezen;

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="lokalizaceService">Servis pro obsluhu</param>
        public AudioService(ILokalizaceService lokalizaceService)
        {
            _lokalizaceService = lokalizaceService;
        }

        /// <summary>
        /// Metoda slouží ke spuštění přehrávání vybrané skladby
        /// </summary>
        /// <param name="filePath">Cesta k souboru skladby</param>
        /// <returns>Vrací Task</returns>
        public async Task Play(string filePath)
        {
            try
            {
                if (reader != null && isPaused && currentFilePath == filePath)
                {
                    output?.Play();
                    isPaused = false;
                    return;
                }

                // Kontrola existence souboru ještě před spuštěním asynchronního vlákna
                if (!File.Exists(filePath))
                {
                    string zprava = String.Format(_lokalizaceService["ErrorFileNotFound"], filePath);
                    throw new FileNotFoundException(zprava, filePath);
                }

                // Veškerá inicializace se provádí asynchronně na pozadí aplikace
                await Task.Run(() =>
                {
                    lock (_audioLock)
                    {
                        try
                        {
                            ReleaseResources();

                            manualStop = false;
                            currentFilePath = filePath;

                            // Pokud jsou načtena uživatelská pásma, použijí se, jinak se použije výchozí pole Pasma.PasmaEkvalizeru                          
                            IList<PasmoEkvalizeru> pasma;

                            if(AktualniPasma != null && AktualniPasma.Count > 0)
                            {
                                pasma = AktualniPasma;
                            }

                            else
                            {
                                pasma = Pasma.PasmaEkvalizeru;
                            }

                            // Vytvoření readeru, ekvalizéru a výstupního zařízení
                            var novyReader = new AudioFileReader(filePath);
                            var novyEkvalizer = new EqualizerSampleProvider(novyReader.ToSampleProvider(), pasma);
                            novyEkvalizer.IsEnabled = JeEkvalizerZapnuty;
                            var novyOutput = new WaveOutEvent();

                            novyOutput.Init(novyEkvalizer);

                            reader = novyReader;
                            output = novyOutput;
                            ekvalizer = novyEkvalizer;

                            output.PlaybackStopped += (s, e) =>
                            {
                                if (!manualStop && reader != null &&
                                    reader.CurrentTime >= reader.TotalTime.Subtract(TimeSpan.FromMilliseconds(200)))
                                {
                                    UkonceniSkladby?.Invoke();
                                }
                            };

                            output.Play();
                        }

                        catch (Exception)
                        {
                            // Pokud selže inicializace (např. poškozený soubor), uvolní se využívané zdroje
                            ReleaseResources();
                            throw;
                        }
                    }
                });

                isPaused = false;
            }

            catch (FileNotFoundException)
            {              
                SouborNenalezen?.Invoke(filePath);
                return;
            }

            catch (Exception ex)
            {
                ReleaseResources();
                SpravaSouboru.LogError(ex, $"Error occurred while playing a song!", nameof(AudioService));
                throw;
            }
        }

        /// <summary>
        /// Metoda slouží k pozastavení přehrávání vybrané skladby
        /// </summary>
        public void Pause()
        {
            try
            {
                if (output != null)
                {
                    output.Pause();
                    isPaused = true;
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while pausing a song!", nameof(AudioService));
                throw;
            }
        }

        /// <summary>
        /// Metoda slouží k vypuštění využívaných zdrojů z paměti
        /// </summary>
        public void Stop()
        {
            try
            {
                lock (_audioLock)
                {
                    ReleaseResources();
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while stopping a song!", nameof(AudioService));
                throw;
            }
        }       

        /// <summary>
        /// Metoda slouží ke spuštění pozastavené písničky, od času, kdy se pozastavila
        /// </summary>
        public void Resume()
        {
            try
            {
                if (output == null || reader == null)
                {
                    return;
                }

                output.Play();
                isPaused = false;
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while resuming a song!", nameof(AudioService));
                throw;
            }
        }

        /// <summary>
        /// Metoda slouží k posunu posuvníku v závislosti na čase
        /// </summary>
        /// <param name="position">Čas, kam se posunout</param>
        public void Seek(TimeSpan position)
        {
            try
            {
                if (reader != null)
                {
                    reader.CurrentTime = position;
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while seeking to position!", nameof(AudioService));
                throw;
            }
        }

        /// <summary>
        /// Metoda slouží k aktualizaci pásem ekvalizéru
        /// </summary>
        /// <param name="pasma">Jednotlivá pásma ekvalizéru</param>
        public void UpdateEqualizer(ObservableCollection<PasmoEkvalizeru> pasma)
        {
            if (pasma == null || pasma.Count == 0 || ekvalizer == null)
            {
                return;
            }

            try
            {
                AktualniPasma = pasma;
                ekvalizer.UpdateEqualizer(pasma);
            }

            catch(Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while updating the equalizer!", nameof(AudioService));
                throw;
            }
        }

        /// <summary>
        /// Pomocná metoda pro zastavení písničky a vypuštění používaných zdrojů
        /// </summary>
        private void ReleaseResources()
        {
            manualStop = true;
            output?.Stop();
            output?.Dispose();
            reader?.Dispose();
            output = null;
            reader = null;
            ekvalizer = null;
        }
    }
}
