using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Services.KnihovnaSkladeb;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.ViewModels
{
    /// <summary>
    /// ViewModel pro obsluhu metod knihovny skladeb
    /// </summary>
    public class KnihovnaViewModel : BaseViewModel
    {
        private readonly IKnihovnaService _knihovnaService;

        /// <summary>
        /// Seznam skladeb
        /// </summary>
        public ObservableCollection<Song>? Skladby { get; set; }

        /// <summary>
        /// Seznam vyfiltrovaných skladeb podle zadaných vyhledávacích kritérií
        /// </summary>
        public ObservableCollection<Song>? VyfiltrovaneSkladby { get; set; }

        private string? vyhledavanyText; 
        public string? VyhledavanyText 
        { 
            get => vyhledavanyText; 

            set 
            { 
                vyhledavanyText = value; 
                Vyfiltruj(); 
                OnPropertyChanged(); 
            } 
        }

        private TypVyhledavani vybranyTypVyhledavani;
        public TypVyhledavani VybranyTypVyhledavani
        {
            get => vybranyTypVyhledavani;
            set
            {
                vybranyTypVyhledavani = value;
                Vyfiltruj();
                OnPropertyChanged();
            }
        }

        private PlayList? vybranyPlaylist;
        public PlayList? VybranyPlaylist
        {
            get => vybranyPlaylist;
            set
            {
                vybranyPlaylist = value;
                OnPropertyChanged();
                PrepnoutZdrojSkladeb();
            }
        }

        /// <summary>
        /// Akce pro vybranou skladbu
        /// </summary>
        public event Action<Song>? SkladbaVybrana;

        private Song? vybranaSkladba;
        public Song? VybranaSkladba
        {
            get => vybranaSkladba;
            set
            {
                vybranaSkladba = value;
                OnPropertyChanged();
                if (value != null)
                {
                    SkladbaVybrana?.Invoke(value);
                }
            }
        }


        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="knihovnaService">Servis pro obsluhu metod knihovny skladeb</param>
        public KnihovnaViewModel(IKnihovnaService knihovnaService)
        {
            _knihovnaService = knihovnaService;
            Skladby = _knihovnaService.Load();
            if (Skladby != null)
            {
                VyfiltrovaneSkladby = new ObservableCollection<Song>(Skladby);
                OnPropertyChanged(nameof(VyfiltrovaneSkladby));
            }

            // Výchozí typ vyhledávání
            VybranyTypVyhledavani = TypVyhledavani.Nazev;
        }

        /// <summary>
        /// Metoda slouží k přepnutí zdroje zobrazovaných skladeb mezi knihovnou a playlistem
        /// </summary>
        private void PrepnoutZdrojSkladeb()
        {
            if (VyfiltrovaneSkladby == null)
            {
                return;
            }

            VyfiltrovaneSkladby.Clear();

            if (VybranyPlaylist != null)
            {
                vyhledavanyText = string.Empty;
                OnPropertyChanged(nameof(VyhledavanyText));
                foreach (var s in VybranyPlaylist.Skladby)
                {
                    VyfiltrovaneSkladby.Add(s);
                }
            }

            else if (Skladby != null)
            {
                Vyfiltruj();
            }
        }


        /// <summary>
        /// Metoda slouží k vyfiltrování skladeb podle zvoleného kritéria
        /// </summary>
        private void Vyfiltruj()
        {
            if(VyfiltrovaneSkladby == null || Skladby == null)
            {
                return;
            }

            VyfiltrovaneSkladby.Clear();

            // Zakázání vyhledávácího pole v knihovně, když je vybraný playlist
            if (VybranyPlaylist != null)
            {
                return;
            }

            // Procházení všech skladeb v knihovně
            foreach (var s in Skladby)
            {
                if (string.IsNullOrWhiteSpace(VyhledavanyText))
                {
                    VyfiltrovaneSkladby.Add(s);
                    continue;
                }

                bool shoda = false;

                // Logika vyfiltrování podle zvoleného kritéria
                switch (VybranyTypVyhledavani)
                {
                    case TypVyhledavani.Nazev:
                        shoda = s.Nazev.Contains(VyhledavanyText, StringComparison.OrdinalIgnoreCase);
                        break;

                    case TypVyhledavani.Interpret:
                        if (s.Interpret != null)
                        {
                            shoda = s.Interpret.Contains(VyhledavanyText, StringComparison.OrdinalIgnoreCase);
                        }

                        break;

                    default:
                        shoda = false;
                        break;
                }

                if (shoda) 
                {
                    VyfiltrovaneSkladby.Add(s);
                }
            }
        }
    }
}
