using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
using Hudebni_Prehravac_OctaBeats.Services.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Hudebni_Prehravac_OctaBeats.ViewModels
{
    /// <summary>
    /// ViewModel pro obsluhu metod pro editaci playlistu
    /// </summary>
    public class PlaylistEditorDialogViewModel : BaseViewModel, IDataErrorInfo
    {
        private readonly ILokalizaceService _lokalizaceService;
        private readonly IEnumerable<PlayList> _vsechnyPlaylisty;

        /// <summary>
        /// Původní název editovaného playlistu
        /// </summary>
        private readonly string _puvodniNazev;

        /// <summary>
        /// Seznam skladeb v knihovně
        /// </summary>
        public ObservableCollection<Song> KnihovnaSkladby { get; }

        /// <summary>
        /// Seznam skladeb v playlistu
        /// </summary>
        public ObservableCollection<Song> PlaylistSkladby { get; }

        /// <summary>
        /// Aktuálně vybraná skladba v knihovně
        /// </summary>
        public Song? VybranaKnihovnaSkladba { get; set; }

        /// <summary>
        /// Aktuálně vybraná skladba v playlistu
        /// </summary>
        public Song? VybranaPlaylistSkladba { get; set; }

        private string? nazevPlaylistu;
        public string? NazevPlaylistu
        {
            get => nazevPlaylistu;
            set
            {
                nazevPlaylistu = value;
                OnPropertyChanged();
            }
        }

        private bool compactMode;
        public bool CompactMode
        {
            get => compactMode;
            set
            {
                compactMode = value;
                OnPropertyChanged();
            }
        }

        /* Příkazy pro obsluhu jednotlivých metod */
        public ICommand PridatCommand { get; }
        public ICommand OdebratCommand { get; }
        public ICommand PotvrditCommand { get; }
        public AsyncRelayCommand AddFolderCommand { get; }

        // Implementace IDataErrorInfo pro validaci
        public string Error => String.Empty;
        public string this[string columnName]
        {
            get
            {
                string? result = String.Empty;
                switch (columnName)
                {
                    case nameof(NazevPlaylistu):
                        if (String.IsNullOrWhiteSpace(NazevPlaylistu))
                        {
                            result = _lokalizaceService["ErrorNameEmpty"];
                        }

                        else if (!NazevPlaylistu.Equals(_puvodniNazev, StringComparison.OrdinalIgnoreCase) &&
                             _vsechnyPlaylisty.Any(p => p.Nazev.Equals(NazevPlaylistu.Trim(), StringComparison.OrdinalIgnoreCase)))
                        {
                            result = _lokalizaceService["ErrorDuplicatePlaylistName"];
                        }
                        return result;
                }

                // Pokud není detekována žádná chyba, tak použijeme lokalizaci
                return _lokalizaceService[columnName];
            }
        }

        /// <summary>
        /// Událost pro uzavření dialogu
        /// </summary>
        public event Action<bool>? ZavritDialog;

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="knihovna">Skladby v knihovně</param>
        /// <param name="stavajiciSkladby">Skladby v playlistu</param>
        /// <param name="playlist">Vybraný playlist k editaci</param>
        /// <param name="vsechnyPlaylisty">Všechny vytvořené playlisty</param>
        /// <param name="lokalizaceService">Servis pro obsluhu metod lokalizace</param>
        public PlaylistEditorDialogViewModel(IEnumerable<Song> knihovna, IEnumerable<Song> stavajiciSkladby, PlayList playlist,
                    IEnumerable<PlayList> vsechnyPlaylisty, ILokalizaceService lokalizaceService)
        {
            _lokalizaceService = lokalizaceService;
            PlaylistSkladby = new ObservableCollection<Song>(stavajiciSkladby);

            _vsechnyPlaylisty = vsechnyPlaylisty;
            _puvodniNazev = playlist.Nazev;

            var skladbyVPlaylistu = PlaylistSkladby
                .Select(s => (
                    Interpret: (s.Interpret ?? "").Trim().ToLower(),
                    Nazev: (s.Nazev ?? Path.GetFileNameWithoutExtension(s.CestaKSouboru) ?? "").Trim().ToLower()
                ))
                .ToHashSet();

            // Přidání do knihovny skladeb v editoru jenom ty skladby, které nejsou už v playlistu
            KnihovnaSkladby = new ObservableCollection<Song>(
                knihovna.Where(s =>
                    !skladbyVPlaylistu.Contains((
                        (s.Interpret ?? "").Trim().ToLower(),
                        (s.Nazev ?? Path.GetFileNameWithoutExtension(s.CestaKSouboru) ?? "").Trim().ToLower()
                    ))
                )
            );

            NazevPlaylistu = playlist.Nazev;

            PridatCommand = new RelayCommand(parameter =>
            {
                // Hromadné přidání vybraných skladeb
                var vybraneSkladby = (parameter as IList)?.Cast<Song>().ToList();
                if (vybraneSkladby != null && vybraneSkladby.Any())
                {
                    foreach (Song song in vybraneSkladby)
                    {
                        PlaylistSkladby.Add(song);
                        KnihovnaSkladby.Remove(song);
                    }
                }
            });

            OdebratCommand = new RelayCommand(parameter =>
            {
                // Hromadné odebrání vybraných skladeb
                var vybraneSkladby = (parameter as IList)?.Cast<Song>().ToList();
                if (vybraneSkladby != null && vybraneSkladby.Any())
                {
                    foreach (Song song in vybraneSkladby)
                    {
                        KnihovnaSkladby.Add(song);
                        PlaylistSkladby.Remove(song);
                    }
                }
            });

            PotvrditCommand = new RelayCommand(_ =>
            {
                if (JeValidni())
                {
                    ZavritDialog?.Invoke(true);
                }
            });

            AddFolderCommand = new AsyncRelayCommand(PridejSlozkuDoPlaylistu);
        }

        /// <summary>
        /// Metoda slouží k přidání celé složky skladeb do playlistu
        /// </summary>
        /// <returns>Vrací Task</returns>
        public async Task PridejSlozkuDoPlaylistu()
        {
            try
            {
                using (var folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = _lokalizaceService["OpenFolderTitle"];
                    folderDialog.UseDescriptionForTitle = true;

                    if (folderDialog.ShowDialog() == DialogResult.OK)
                    {
                        string vybranaCesta = folderDialog.SelectedPath;

                        string[] podporovanePripony = { ".mp3", ".wav", ".flac" };

                        string[] soubory = Directory.GetFiles(vybranaCesta, "*.*", SearchOption.AllDirectories)
                            .Where(s => podporovanePripony.Contains(Path.GetExtension(s).ToLower()))
                            .ToArray();

                        if (soubory.Length == 0)
                        {
                            return;
                        }

                        MetadataService metadata = new MetadataService(_lokalizaceService);

                        // Rozdělení do HashSetu
                        var existujici = PlaylistSkladby
                            .Select(s => (
                                Interpret: (s.Interpret ?? "").Trim().ToLower(),
                                Nazev: (s.Nazev ?? "").Trim().ToLower()
                            ))
                            .ToHashSet();

                        foreach (string cesta in soubory)
                        {
                            try
                            {
                                // Načtení metadat skladeb ze složky
                                Song song = await Task.Run(() => metadata.Load(cesta));

                                var klic = (
                                    Interpret: (song.Interpret ?? "").Trim().ToLower(),
                                    Nazev: (song.Nazev ?? "").Trim().ToLower()
                                );

                                // Pokud ještě v playlistu nejsou skladby ze složky, tak se přidají
                                if (!existujici.Contains(klic))
                                {
                                    PlaylistSkladby.Add(song);
                                    existujici.Add(klic);
                                }
                            }
                            catch
                            {
                                // Poškozený soubor se přeskočí
                            }
                        }

                        // Odstranění duplicit z knihovny
                        OdeberDuplicitniZKnihovny();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Metoda slouží k validaci, zda jsou všechna pole správně vyplněna
        /// </summary>
        /// <returns>Vrací true, pokud jsou všechny pole validní, jinak false</returns>
        private bool JeValidni()
        {
            return String.IsNullOrEmpty(this[nameof(NazevPlaylistu)]);
        }

        /// <summary>
        /// Metoda slouží k odebrání duplicitních skladeb, které jsou v knihovně a zároveň v playlistu
        /// </summary>
        private void OdeberDuplicitniZKnihovny()
        {
            var playlistSet = PlaylistSkladby
                .Select(s => (
                    Interpret: (s.Interpret ?? "").Trim().ToLower(),
                    Nazev: (s.Nazev ?? "").Trim().ToLower()
                ))
                .ToHashSet();

            var kOdebrani = KnihovnaSkladby
                .Where(s => playlistSet.Contains((
                    (s.Interpret ?? "").Trim().ToLower(),
                    (s.Nazev ?? "").Trim().ToLower()
                )))
                .ToList();

            foreach (Song song in kOdebrani)
            {
                KnihovnaSkladby.Remove(song);
            }
        }
    }
}