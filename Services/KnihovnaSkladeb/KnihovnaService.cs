using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using Hudebni_Prehravac_OctaBeats.Services.Historie;
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
                        MessageBox.Show($"Skladba s názvem '{nazevSouboru} už ve složce {cilovaSlozka} existuje!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    SpravaSouboru.LogError(ex, $"Chyba při kopírování vybraných skladeb do MyMusic ve třídě {nameof(KnihovnaService)}");
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
                throw new ArgumentException("Cesta není správná, nic nebylo smazáno!");
            }

            try
            {
                if (File.Exists(cestaVybraneSkladby))
                {
                    File.Delete(cestaVybraneSkladby);
                    return true;
                }
            }

            catch(Exception ex)
            {
                SpravaSouboru.LogError(ex, $"Nastala chyba při mazání skladby z MyMusic ve třídě {nameof(KnihovnaService)}");
            }

            return false;
        }
    }
}
