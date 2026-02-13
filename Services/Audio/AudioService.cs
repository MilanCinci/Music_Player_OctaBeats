using Hudebni_Prehravac_OctaBeats.Persistence;
using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
using Hudebni_Prehravac_OctaBeats.ViewModels;
using NAudio.Wave;
using System;
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
        private IWavePlayer? output;
        private AudioFileReader? reader;
        private bool isPaused;
        private bool manualStop;
        private string? currentFilePath;
        private readonly object _audioLock = new object();
        private readonly ILokalizaceService _lokalizaceService;

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
        /// Aktuální čas přehrávání skladby
        /// </summary>
        public TimeSpan AktualniCas => reader?.CurrentTime ?? TimeSpan.Zero;

        /// <summary>
        /// Celkový čas trvání celé skladby
        /// </summary>
        public TimeSpan CelkovyCas => reader?.TotalTime ?? TimeSpan.Zero;

        /// <summary>
        /// Akce ukončení skladby
        /// </summary>
        public event Action? UkonceniSkladby;

        /// <summary>
        /// Akce nenalezení souboru skladby
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

                // Všechna inicializace se provádí na pozadí
                await Task.Run(() =>
                {
                    lock (_audioLock)
                    {
                        try
                        {
                            ReleaseResources();

                            manualStop = false;
                            currentFilePath = filePath;

                            // Vytvoření readeru a výstupního zařízení
                            var novyReader = new AudioFileReader(filePath);
                            var novyOutput = new WaveOutEvent();

                            novyOutput.Init(novyReader);

                            reader = novyReader;
                            output = novyOutput;

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
        }
    }
}
