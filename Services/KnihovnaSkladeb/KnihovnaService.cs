using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using Hudebni_Prehravac_OctaBeats.Services.Dialog;
using Hudebni_Prehravac_OctaBeats.Services.Historie;
using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
using Hudebni_Prehravac_OctaBeats.Services.Metadata;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Hudebni_Prehravac_OctaBeats.Services.KnihovnaSkladeb
{
    /// <summary>
    /// Třída sloužící k implementování rozhraní IKnihovnaService a obsluze daných metod
    /// </summary>
    public class KnihovnaService : IKnihovnaService
    {
        private readonly IMetadataService _metadataService;
        private readonly IDialogService _dialogService;
        private readonly ILokalizaceService _lokalizaceService;

        /// <summary>
        /// Pole podporovaných hudebních formátů přehrávače
        /// </summary>
        private readonly string[] _podporovaneFormaty =
        {
            ".mp3", ".wav", ".flac"
        };

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="lokalizaceService">Servis pro obsluhu metod lokalizace aplikace</param>
        /// <param name="dialogService">Servis pro zobrazení příslušných dialogů</param>
        public KnihovnaService(ILokalizaceService lokalizaceService, IDialogService dialogService)
        {
            _lokalizaceService = lokalizaceService;
            _dialogService = dialogService;
            _metadataService = new MetadataService(_lokalizaceService);
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

            // Pokud adresář neexistuje, vrátí se prázdný seznam skladeb
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

                catch (FileNotFoundException)
                {
                    // Soubor zmizel z disku -> přeskočí se
                    continue;
                }

                catch (Exception)
                {
                    // Jiná chyba (poškozený tag apod.) -> přeskočí se
                    continue;
                }
            }

            return skladby;
        }

        /// <summary>
        /// Metoda slouží k překopírování souborů do složky s hudbou MyMusic
        /// </summary>
        /// <param name="vybraneSoubory">Uživatelem vybrané soubory dialogu</param>
        /// <returns>Vrací Task</returns>
        public async Task CopySongsToMyMusic(string[] vybraneSoubory)
        {
            if(vybraneSoubory == null || vybraneSoubory.Length == 0)
            {
                return;
            }

            string cilovaSlozka = CestaKSouboru;

            foreach (string zdroj in vybraneSoubory)
            {
                try
                {
                    string nazevSouboru = Path.GetFileName(zdroj);
                    string cil = Path.Combine(cilovaSlozka, nazevSouboru);

                    // Pokud soubor už existuje, zobrazí se upozornění
                    if (File.Exists(cil))
                    {
                        string zprava = String.Format(_lokalizaceService["WarningFileAlreadyExists"], nazevSouboru, cilovaSlozka);
                        _dialogService.ShowWarning(zprava);
                        continue;
                    }

                    // Použití asynchronního kopírování
                    using (FileStream zdrojovyStream = File.OpenRead(zdroj))
                    using (FileStream cilovyStream = File.Create(cil))
                    {
                        await zdrojovyStream.CopyToAsync(cilovyStream);
                    }
                }

                catch (Exception ex)
                {
                    SpravaSouboru.LogError(ex, "Error occurred while copying songs to MyMusic folder!", nameof(CopySongsToMyMusic));
                    throw;
                }
            }
        }

        /// <summary>
        /// Metoda slouží k vymazání souboru ze složky s hudbou MyMusic
        /// </summary>
        /// <param name="cestaVybraneSkladby">Uživatelem vybraná skladba ke smazání</param>
        /// <returns>Vrací true, pokud cesta ke skladbě je validní, jinak false</returns>
        public bool DeleteSongFromMyMusic(string cestaVybraneSkladby)
        {
            if(String.IsNullOrEmpty(cestaVybraneSkladby))
            {
                throw new ArgumentException(_lokalizaceService["ErrorInvalidPath"]);
            }

            try
            {
                if (File.Exists(cestaVybraneSkladby))
                {
                    File.Delete(cestaVybraneSkladby);
                    return true;
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while deleting the song from MyMusic folder!", nameof(DeleteSongFromMyMusic));
                throw;
            }

            return false;
        }
    }
}
