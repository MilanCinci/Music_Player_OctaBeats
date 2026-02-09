using Hudebni_Prehravac_OctaBeats.Persistence;
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
                    throw new FileNotFoundException($"Soubor '{filePath}' nebyl nalezen!", filePath);
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
                                // Kontrola, zda skladba skončila přirozeně (není to manuální Stop)
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
                            // Pokud selže inicializace (např. poškozený soubor), uvolníme zdroje a pošleme chybu dál
                            ReleaseResources();
                            throw;
                        }
                    }
                });

                isPaused = false;
            }

            catch (FileNotFoundException)
            {
                // Signalizace pro MainViewModel, že soubor neexistuje a je potřeba provést refresh
                SouborNenalezen?.Invoke(filePath);
            }

            catch (Exception ex)
            {
                ReleaseResources();
                SpravaSouboru.LogError(ex, "Audio výstup selhal", nameof(AudioService));
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

            catch (Exception)
            {
                throw new Exception();
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

            catch (Exception)
            {
                throw new Exception();
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

            catch (Exception)
            {
                throw new Exception();
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

            catch (Exception)
            {
                throw new Exception();
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
