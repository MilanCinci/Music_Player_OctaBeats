using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
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

        private string nazevPlaylistu;
        public string NazevPlaylistu
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
        public ICommand ZrusitCommand { get; }


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
                            result = "Název playlistu nemůže být prázdný!";
                        }

                        else if (!NazevPlaylistu.Equals(_puvodniNazev, StringComparison.OrdinalIgnoreCase) &&
                             _vsechnyPlaylisty.Any(p => p.Nazev.Equals(NazevPlaylistu.Trim(), StringComparison.OrdinalIgnoreCase)))
                        {
                            result = "Jiný playlist s tímto názvem již existuje!";
                        }
                        break;
                }

                return result;
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
        /// <param name="playlistSkladby">Skladby v playlistu</param>
        /// <param name="playlist">Vybraný playlist k editaci</param>
        public PlaylistEditorDialogViewModel(IEnumerable<Song> knihovna, IEnumerable<Song> stavajiciSkladby, PlayList playlist, IEnumerable<PlayList> vsechnyPlaylisty)
        {
            PlaylistSkladby = new ObservableCollection<Song>(stavajiciSkladby);

            _vsechnyPlaylisty = vsechnyPlaylisty;
            _puvodniNazev = playlist.Nazev;

            // Do knihovny v editoru dáme pouze ty skladby, které se nenachází v aktuálně upravovaném playlistu
            var cestyVPlaylistu = PlaylistSkladby.Select(s => s.CestaKSouboru).ToHashSet();
            KnihovnaSkladby = new ObservableCollection<Song>(knihovna.Where(s => !cestyVPlaylistu.Contains(s.CestaKSouboru)));

            NazevPlaylistu = playlist.Nazev;

            PridatCommand = new RelayCommand(_ =>
            {
                if (VybranaKnihovnaSkladba != null)
                {
                    // Přidáme skladbu do playlistu
                    PlaylistSkladby.Add(VybranaKnihovnaSkladba);
                    // Odebereme ji z viditelného seznamu knihovny v editoru
                    KnihovnaSkladby.Remove(VybranaKnihovnaSkladba);
                }
            });

            OdebratCommand = new RelayCommand(_ =>
            {
                if (VybranaPlaylistSkladba != null)
                {
                    // Vrátíme skladbu zpět do seznamu knihovny v editoru
                    KnihovnaSkladby.Add(VybranaPlaylistSkladba);
                    // Odebereme ji z playlistu
                    PlaylistSkladby.Remove(VybranaPlaylistSkladba);
                }
            });

            // Potvrzení vrátí true - data budeme přebírat v MainViewModelu
            PotvrditCommand = new RelayCommand(_ =>
            {
                if (JeValidni())
                {
                    ZavritDialog?.Invoke(true);
                }
            });
            ZrusitCommand = new RelayCommand(_ => ZavritDialog?.Invoke(false));
        }

        /// <summary>
        /// Metoda slouží k validaci, zda jsou všechna pole správně vyplněna
        /// </summary>
        private bool JeValidni()
        {
            return String.IsNullOrEmpty(this[nameof(NazevPlaylistu)]);
        }
    }
}