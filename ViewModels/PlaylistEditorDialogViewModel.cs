using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hudebni_Prehravac_OctaBeats.ViewModels
{
    /// <summary>
    /// ViewModel pro obsluhu metod pro editaci playlistu
    /// </summary>
    public class PlaylistEditorDialogViewModel : BaseViewModel
    {
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
        public PlaylistEditorDialogViewModel(IEnumerable<Song> knihovna, IEnumerable<Song> stavajiciSkladby, PlayList playlist)
        {
            KnihovnaSkladby = new ObservableCollection<Song>(knihovna);

            // KLÍČOVÁ ZMĚNA: Vytvoříme novou kolekci se stejnými prvky, 
            // nepracujeme s tou původní z MainViewModelu.
            PlaylistSkladby = new ObservableCollection<Song>(stavajiciSkladby);

            NazevPlaylistu = playlist.Nazev;

            PridatCommand = new RelayCommand(_ =>
            {
                if (VybranaKnihovnaSkladba != null && !PlaylistSkladby.Any(s => s.CestaKSouboru == VybranaKnihovnaSkladba.CestaKSouboru))
                {
                    PlaylistSkladby.Add(VybranaKnihovnaSkladba);
                }
            });

            OdebratCommand = new RelayCommand(_ =>
            {
                if (VybranaPlaylistSkladba != null)
                {
                    PlaylistSkladby.Remove(VybranaPlaylistSkladba);
                }
            });

            // Potvrzení vrátí true - data budeme přebírat v MainViewModelu
            PotvrditCommand = new RelayCommand(_ => ZavritDialog?.Invoke(true));
            ZrusitCommand = new RelayCommand(_ => ZavritDialog?.Invoke(false));
        }
    }
}
