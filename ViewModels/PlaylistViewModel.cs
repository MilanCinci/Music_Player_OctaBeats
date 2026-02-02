using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Services.Metadata;
using Hudebni_Prehravac_OctaBeats.Services.Playlist;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// Aktuální index pro nepojmenované playlisty
        /// </summary>
        private int indexPlaylistu = 1;

        /// <summary>
        /// Výchozí název pro nepojmenované playlisty
        /// </summary>
        private static string vychoziNazev = "New playlist";

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
                NovyNazevPlaylistu = $"{vychoziNazev}{indexPlaylistu}";
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
        /// Metoda slouží k asynchronnímu načtení playlistů a metadat
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

                // Hledání aktuálního indexu u uložených playlistů jako "New playlist" 
                List<PlayList> hledanePlaylisty = Playlisty.Where(playlist => playlist.Nazev.StartsWith(vychoziNazev, StringComparison.OrdinalIgnoreCase))
                                                            .ToList();
                int maxIndex = 0;

                foreach (PlayList playlist in hledanePlaylisty)
                {
                    var shoda = Regex.Match(playlist.Nazev, @"\d+$");

                    if (shoda.Success)
                    {
                        if (int.TryParse(shoda.Value, out int index))
                        {
                            if (index > maxIndex)
                            {
                                maxIndex = index;
                            }
                        }
                    }
                }

                indexPlaylistu = maxIndex + 1;
            }

            catch (Exception)
            {
                //TODO
            }
        }
    }
}
