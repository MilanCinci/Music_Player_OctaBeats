using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Services.Metadata;
using Hudebni_Prehravac_OctaBeats.Services.Playlist;
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
    /// ViewModel pro obsluhu metod playlist 
    /// </summary>
    public class PlaylistViewModel : BaseViewModel
    {
        private readonly IPlaylistService _playlistService;

        /// <summary>
        /// Seznam vytvořených playlistů
        /// </summary>
        public ObservableCollection<PlayList> Playlisty { get; set; }

        private PlayList? vybranyPlaylist;
        public PlayList? VybranyPlaylist
        {
            get => vybranyPlaylist;
            set
            {
                vybranyPlaylist = value;
                OnPropertyChanged();
            }
        }

        private string? novyNazevPlaylistu;
        public string? NovyNazevPlaylistu
        {
            get => novyNazevPlaylistu;
            set
            {
                novyNazevPlaylistu = value;
                OnPropertyChanged();
            }
        }

        /* Příkazy pro obsluhu jednotlivých metod */
        public ICommand AddPlaylistCommand { get; }
        public ICommand RemovePlaylistCommand { get; }
        public ICommand ResetVyberCommand { get; }

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="playlistService">Servis pro obsluhu metod playlistů</param>
        public PlaylistViewModel(IPlaylistService playlistService)
        {
            _playlistService = playlistService;

            Playlisty = _playlistService.Load() ?? new ObservableCollection<PlayList>();

            var metadata = new MetadataService();

            foreach (var playlist in Playlisty)
            {
                playlist.Skladby.Clear();

                foreach (var cesta in playlist.CestyKSkladbam)
                {
                    try
                    {
                        playlist.Skladby.Add(metadata.Load(cesta));
                    }

                    catch
                    {

                    }
                }
            }

            AddPlaylistCommand = new RelayCommand(_ => AddPlaylist());
            RemovePlaylistCommand = new RelayCommand(_ => RemovePlaylist());
            ResetVyberCommand = new RelayCommand(_ => VybranyPlaylist = null);
        }

        /// <summary>
        /// Metoda slouží k refreshnutí změn ve View
        /// </summary>
        public void RefreshPlaylisty()
        {
            OnPropertyChanged(nameof(Playlisty));
        }

        /// <summary>
        /// Metoda slouží k přidání nového playlistu do seznamu
        /// </summary>
        private void AddPlaylist()
        {
            // Nastavení výchozího názvu playlistu
            if (string.IsNullOrWhiteSpace(NovyNazevPlaylistu))
            {
                NovyNazevPlaylistu = "Nový playlist";
            }

            PlayList novyPlaylist = new PlayList
            {
                Nazev = NovyNazevPlaylistu,
                Skladby = new ObservableCollection<Song>()
            };

            Playlisty.Add(novyPlaylist);

            NovyNazevPlaylistu = String.Empty;

            _playlistService.Save(Playlisty);
        }

        /// <summary>
        /// Metoda slouží k odebrání vybraného playlistu
        /// </summary>
        private void RemovePlaylist()
        {
            if (VybranyPlaylist != null)
            {
                Playlisty.Remove(VybranyPlaylist);
                VybranyPlaylist = null;
                _playlistService.Save(Playlisty);
            }
        }
    }
}
