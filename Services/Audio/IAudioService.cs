using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Services.Audio
{
    /// <summary>
    /// Rozhraní sloužící k definování metod pro základní ovládání skladby
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// Aktuální čas přehrávání skladby
        /// </summary>
        TimeSpan AktualniCas { get; }

        /// <summary>
        /// Celkový čas trvání celé skladby
        /// </summary>
        TimeSpan CelkovyCas { get; }

        /// <summary>
        /// Metoda slouží ke spuštění přehrávání vybrané skladby
        /// </summary>
        /// <param name="filePath">Cesta k souboru skladby</param>
        public Task Play(string filePath);

        /// <summary>
        /// Metoda slouží k zastavení přehrávání vybrané skladby
        /// </summary>
        public void Pause();

        /// <summary>
        /// Metoda slouží k vypuštění využívaných zdrojů z paměti
        /// </summary>
        public void Stop();     

        /// <summary>
        /// Metoda slouží k posunu posuvníku v závislosti na čase
        /// </summary>
        /// <param name="position">Čas, kam se posunout</param>
        void Seek(TimeSpan position);

        /// <summary>
        /// Akce ukončení skladby
        /// </summary>
        event Action UkonceniSkladby;
    }
}
