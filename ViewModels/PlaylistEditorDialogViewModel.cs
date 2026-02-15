using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        /* Příkazy pro obsluhu jednotlivých metod */
        public ICommand PridatCommand { get; }
        public ICommand OdebratCommand { get; }
        public ICommand PotvrditCommand { get; }

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
        /// Akce pro uzavření dialogu
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

            // Do knihovny v editoru se přidají pouze ty skladby, které se nenachází v aktuálně upravovaném playlistu
            var cestyVPlaylistu = PlaylistSkladby.Select(skladba => skladba.CestaKSouboru).ToHashSet();
            KnihovnaSkladby = new ObservableCollection<Song>(knihovna.Where(skladba => !cestyVPlaylistu.Contains(skladba.CestaKSouboru)));

            NazevPlaylistu = playlist.Nazev;

            PridatCommand = new RelayCommand(_ =>
            {
                if (VybranaKnihovnaSkladba != null)
                {
                    PlaylistSkladby.Add(VybranaKnihovnaSkladba);
                    KnihovnaSkladby.Remove(VybranaKnihovnaSkladba);
                }
            });

            OdebratCommand = new RelayCommand(_ =>
            {
                if (VybranaPlaylistSkladba != null)
                {
                    KnihovnaSkladby.Add(VybranaPlaylistSkladba);
                    PlaylistSkladby.Remove(VybranaPlaylistSkladba);
                }
            });

            PotvrditCommand = new RelayCommand(_ =>
            {
                if (JeValidni())
                {
                    ZavritDialog?.Invoke(true);
                }
            });
            }

        /// <summary>
        /// Metoda slouží k validaci, zda jsou všechna pole správně vyplněna
        /// </summary>
        /// <returns>Vrací true, pokud je vše validní, jinak false</returns>
        private bool JeValidni()
        {
            return String.IsNullOrEmpty(this[nameof(NazevPlaylistu)]);
        }
    }
}