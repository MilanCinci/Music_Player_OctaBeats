using Hudebni_Prehravac_OctaBeats.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// Aktuální hlasitost skladby
        /// </summary>
        float Hlasitost { get; set; }

        /// <summary>
        /// Určuje, zda je ekvalizér zapnutý
        /// </summary>
        bool JeEkvalizerZapnuty { get; set; }

        /// <summary>
        /// Aktuálně nastavená pásma ekvalizéru
        /// </summary>
        ObservableCollection<PasmoEkvalizeru>? AktualniPasma { get; set; }

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
        Task Play(string filePath);

        /// <summary>
        /// Metoda slouží k zastavení přehrávání vybrané skladby
        /// </summary>
        void Pause();

        /// <summary>
        /// Metoda slouží k vypuštění využívaných zdrojů z paměti
        /// </summary>
        void Stop();

        /// <summary>
        /// Metoda slouží ke spuštění pozastavené písničky, od času, kdy se pozastavila
        /// </summary>
        void Resume();

        /// <summary>
        /// Metoda slouží k posunu posuvníku v závislosti na čase
        /// </summary>
        /// <param name="position">Čas, kam se posunout</param>
        void Seek(TimeSpan position);

        /// <summary>
        /// Metoda slouží k aktualizaci pásem ekvalizéru
        /// </summary>
        /// <param name="pasma">Jednotlivá pásma ekvalizéru</param>
        void UpdateEqualizer(ObservableCollection<PasmoEkvalizeru> pasma);

        /// <summary>
        /// Událost ukončení skladby
        /// </summary>
        event Action? UkonceniSkladby;

        /// <summary>
        /// Událost nenalezení souboru skladby
        /// </summary>
        event Action<string>? SouborNenalezen;
    }
}
