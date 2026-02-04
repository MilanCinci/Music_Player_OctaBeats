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
        /// Metoda slouží k odstranění skladby z playlistu, pokud se odstraňuje skladba z knihovny
        /// </summary>
        /// <param name="odstranenaSkladba">Skladba, kterou chceme odstranit</param>
        /// <returns>Vrací Task</returns>
        public async Task RemoveSongFromPlaylist(Song odstranenaSkladba)
        {
            if (odstranenaSkladba == null || Playlisty == null)
            {
                return;
            }

            bool celkovaZmena = false;

            foreach (PlayList playlist in Playlisty)
            {
                if (playlist != null)
                {
                    bool zmenaCest = playlist.CestyKSkladbam.Remove(odstranenaSkladba.CestaKSouboru);
                    Song? songInPlaylist = playlist.Skladby.FirstOrDefault(s => s.CestaKSouboru == odstranenaSkladba.CestaKSouboru);
                    bool zmenaSkladeb = false;
                    if (songInPlaylist != null)
                    {
                        zmenaSkladeb = playlist.Skladby.Remove(songInPlaylist);
                    }

                    // Pokud došlo k jakékoliv změně v tomto playlistu, tak si ji zapamatujeme pro následné uložení
                    if (zmenaCest || zmenaSkladeb)
                    {
                        celkovaZmena = true;
                    }
                }
            }

            // Uložení pouze v případě, když se provedou změny v nějakém playlistu
            if (celkovaZmena)
            {
                await _playlistService.Save(Playlisty);
            }
        }

        /// <summary>
        /// Metoda slouží k přidání nového playlistu do seznamu
        /// </summary>
        /// <returns>Vrací Task</returns>
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
        /// <returns>Vrací Task</returns>
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
        /// <returns>Vrací Task</returns>
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
                            playlist.Skladby.Add(await Task.Run(() => metadata.Load(cesta)));
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
