using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Services.Metadata;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Services.KnihovnaSkladeb
{
    /// <summary>
    /// Třída sloužící k implementování rozhraní IKnihovnaService a obsluze daných metod
    /// </summary>
    public class KnihovnaService : IKnihovnaService
    {
        /// <summary>
        /// Obslužná třída pro získání skladeb společně s jejich metadaty
        /// </summary>
        private readonly IMetadataService _metadataService;

        /// <summary>
        /// Pole podporovaných hudebních formátů přehrávače
        /// </summary>
        private readonly string[] _podporovaneFormaty =
        {
            ".mp3", ".wav", ".flac"
        };

        /// <summary>
        /// Bezparametrický konstruktor pro inicializaci
        /// </summary>
        public KnihovnaService()
        {
            _metadataService = new MetadataService();
        }

        /// <summary>
        /// Cesta k souboru se skladbami (defaultně to bude speciální Windows složka MyMusic)
        /// </summary>
        private static string CestaKSouboru = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        /// <summary>
        /// Metoda slouží k načtení uložených skladeb
        /// </summary>
        /// <returns>Vrací kolekci načtených skladeb</returns>
        public async Task<ObservableCollection<Song>> Load()
        {
            ObservableCollection<Song> skladby = new ObservableCollection<Song>();

            if (!Directory.Exists(CestaKSouboru))
            {
                return skladby;
            }

            // Hledání souborů, které mají podporované formáty
            var soubory = Directory.GetFiles(
                CestaKSouboru, "*.*", SearchOption.AllDirectories)
                .Where(s => _podporovaneFormaty.Contains(Path.GetExtension(s)));

            foreach (var soubor in soubory)
            {
                try
                {
                    var song = await _metadataService.Load(soubor);
                    skladby.Add(song);
                }

                catch
                {

                }
            }

            return skladby;
        }
    }
}
