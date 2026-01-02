using NAudio.Wave;
using System;

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
        /// Metoda slouží ke spuštění přehrávání vybrané skladby
        /// </summary>
        /// <param name="filePath">Cesta k souboru skladby</param>
        public async Task Play(string filePath)
        {
            try
            {
                // Pokud jsme jenom pozastavení, tak stačí pokračovat dál
                if (reader != null && isPaused && currentFilePath == filePath)
                {
                    output?.Play();
                    isPaused = false;
                    return;
                }

                // Spustíme inicializaci na pozadí
                await Task.Run(() =>
                {
                    // Zámek zajistí, že se druhé vlákno nespustí, 
                    // dokud první nedokončí Stop() a Init()
                    lock (_audioLock)
                    {
                        StopInternal(); // Voláme interní metodu bez dalšího zámku

                        manualStop = false;
                        currentFilePath = filePath;
                        reader = new AudioFileReader(filePath);
                        output = new WaveOutEvent();
                        output.Init(reader);

                        output.PlaybackStopped += (s, e) =>
                        {
                            // Kontrola, zda skladba skutečně dojela do konce a nebylo to vyvoláno tlačítkem Stop/Next/Previous
                            if (!manualStop && reader != null && reader.CurrentTime >= reader.TotalTime.Subtract(TimeSpan.FromMilliseconds(100)))
                            {
                                UkonceniSkladby?.Invoke();
                            }
                        };

                        output.Play();
                    }
                });
                isPaused = false;
            }

            catch (Exception)
            {
                throw new Exception();
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
                    StopInternal();
                }
            }

            catch (Exception)
            {
                throw new Exception();
            }
        }

        private void StopInternal()
        {
            manualStop = true;
            output?.Stop();
            output?.Dispose();
            reader?.Dispose();
            output = null;
            reader = null;
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
    }
}
