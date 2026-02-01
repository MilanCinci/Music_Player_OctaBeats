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
        public ObservableCollection<PlayList>? Playlisty { get; set; }

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

        private int indexPlaylistu = 0;

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

            _ = InicializujAsync();

            AddPlaylistCommand = new AsyncRelayCommand(AddPlaylist);
            RemovePlaylistCommand = new AsyncRelayCommand(RemovePlaylist);
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
        private async Task AddPlaylist()
        {
            if(Playlisty == null)
            {
                return;
            }

            // Nastavení výchozího názvu playlistu
            if (String.IsNullOrWhiteSpace(NovyNazevPlaylistu))
            {
                NovyNazevPlaylistu = $"New playlist{indexPlaylistu}";
                indexPlaylistu++;
            }

            PlayList novyPlaylist = new PlayList
            {
                Nazev = NovyNazevPlaylistu,
                Skladby = new ObservableCollection<Song>()
            };

            Playlisty.Add(novyPlaylist);

            NovyNazevPlaylistu = String.Empty;

            await _playlistService.Save(Playlisty);
        }

        /// <summary>
        /// Metoda slouží k odebrání vybraného playlistu
        /// </summary>
        private async Task RemovePlaylist()
        {
            if (VybranyPlaylist != null && Playlisty != null)
            {
                Playlisty.Remove(VybranyPlaylist);
                VybranyPlaylist = null;
                await _playlistService.Save(Playlisty);
            }
        }

        /// <summary>
        /// Metoda slouží k načtení playlistů + metadat
        /// </summary>
        private async Task InicializujAsync()
        {
            try
            {
                Playlisty = await _playlistService.Load()! ?? new ObservableCollection<PlayList>();

                var metadata = new MetadataService();

                foreach (var playlist in Playlisty)
                {
                    playlist.Skladby.Clear();

                    foreach (var cesta in playlist.CestyKSkladbam)
                    {
                        try
                        {
                            playlist.Skladby.Add(
                                await Task.Run(() => metadata.Load(cesta))
                            );
                        }

                        catch
                        {

                        }
                    }
                }

                OnPropertyChanged(nameof(Playlisty));
            }

            catch (Exception)
            {
                //TODO
            }
        }
    }
}
