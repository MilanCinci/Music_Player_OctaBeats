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
        public void Play(string filePath)
        {
            // Pokud jsme jenom pozastavení, tak stačí pokračovat dál
            if (reader != null && isPaused && currentFilePath == filePath)
            {
                output?.Play();
                isPaused = false;
                return;
            }

            Stop();

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
            isPaused = false;
        }

        /// <summary>
        /// Metoda slouží k pozastavení přehrávání vybrané skladby
        /// </summary>
        public void Pause()
        {
            if (output != null)
            {
                output.Pause();
                isPaused = true;
            }
        }

        /// <summary>
        /// Metoda slouží k vypuštění využívaných zdrojů z paměti
        /// </summary>
        public void Stop()
        {
            manualStop = true;

            output?.Stop();
            output?.Dispose();
            reader?.Dispose();

            output = null;
            reader = null;
            isPaused = false;
            currentFilePath = null;
        }

        /// <summary>
        /// Metoda slouží k posunu posuvníku v závislosti na čase
        /// </summary>
        /// <param name="position">Čas, kam se posunout</param>
        public void Seek(TimeSpan position)
        {
            if (reader != null)
            {
                reader.CurrentTime = position;
            }
        }
    }
}
