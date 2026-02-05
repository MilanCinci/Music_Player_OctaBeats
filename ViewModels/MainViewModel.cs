using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Services;
using Hudebni_Prehravac_OctaBeats.Services.Audio;
using Hudebni_Prehravac_OctaBeats.Services.Historie;
using Hudebni_Prehravac_OctaBeats.Services.KnihovnaSkladeb;
using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
using Hudebni_Prehravac_OctaBeats.Services.NastaveniAudia;
using Hudebni_Prehravac_OctaBeats.Services.Playlist;
using Hudebni_Prehravac_OctaBeats.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace Hudebni_Prehravac_OctaBeats.ViewModels
{
    /// <summary>
    /// ViewModel pro navigaci mezi obrazovkami
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly IPlaylistService _playlistService;
        private readonly IAudioService _audioService;
        private readonly IHistorieService _historieService;
        private readonly INastaveniAudiaService _nastaveniAudiaService;
        private readonly IKnihovnaService _knihovnaService;
        private readonly ILokalizaceService _lokalizaceService;

        /// <summary>
        /// Výchozí název zdroje přehrávání, pokud není uveden
        /// </summary>
        private static string VychoziNazevZdroje = "Knihovna";

        /* Příkazy pro obsluhu jednotlivých metod */
        public ICommand AddSongCommand { get; }
        public ICommand RemoveSongCommand { get; }

        /// <summary>
        /// ViewModel přehrávače
        /// </summary>
        public PrehravacViewModel PrehravacVM { get; }

        /// <summary>
        /// ViewModel playlistů
        /// </summary>
        public PlaylistViewModel PlaylistVM { get; }

        /// <summary>
        /// ViewModel knihovny skladeb
        /// </summary>
        public KnihovnaViewModel KnihovnaVM { get; }

        /// <summary>
        /// ViewModel historie přehrávání
        /// </summary>
        public HistoryViewModel HistoryVM { get; }

        /// <summary>
        /// ViewModel nastavení aplikace
        /// </summary>
        public NastaveniViewModel NastaveniVM { get; }

        /// <summary>
        /// Bezparametrický konstruktor pro inicializaci
        /// </summary>
        public MainViewModel()
        {
            _playlistService = new PlaylistService();
            _historieService = new HistoryService();
            _audioService = new AudioService();
            _nastaveniAudiaService = new NastaveniAudiaService();
            _knihovnaService = new KnihovnaService();
            _lokalizaceService = new LokalizaceService();

            PrehravacVM = new PrehravacViewModel(
                _audioService,
                _historieService,
                _nastaveniAudiaService);

            PlaylistVM = new PlaylistViewModel(_playlistService);
            KnihovnaVM = new KnihovnaViewModel(_knihovnaService);
            HistoryVM = new HistoryViewModel(_historieService);
            NastaveniVM = new NastaveniViewModel(_lokalizaceService);

            // Propojení playlistů s knihovnou
            PlaylistVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PlaylistViewModel.VybranyPlaylist))
                {
                    KnihovnaVM.VybranyPlaylist = PlaylistVM.VybranyPlaylist;
                }
            };

            // Propojení přehrávače s knihovnou
            PrehravacVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PrehravacViewModel.AktualniSkladba))
                {
                    // Pokud přehrávač přepne skladbu, aktualizujeme výběr v knihovně
                    if (KnihovnaVM.VybranaSkladba != PrehravacVM.AktualniSkladba)
                    {
                        KnihovnaVM.VybranaSkladba = PrehravacVM.AktualniSkladba;
                    }
                }
            };

            // Akce při výběru skladby v knihovně
            KnihovnaVM.SkladbaVybrana += skladba =>
            {
                if (PrehravacVM.AktualniSkladba != skladba)
                {
                    string zdroj = KnihovnaVM.VybranyPlaylist?.Nazev ?? VychoziNazevZdroje;
                    PrehravacVM.SetPlaylist(KnihovnaVM.VyfiltrovaneSkladby!, skladba, zdroj);
                }
            };

            // Akce při smazání vybrané skladby v knihovně
            KnihovnaVM.SkladbaSmazana += async skladba =>
            {
                PrehravacVM.OdstranSkladbuZFronty(skladba);
                await PlaylistVM.RemoveSongFromPlaylist(skladba);
            };

            AddSongCommand = KnihovnaVM.AddSongCommand;
            RemoveSongCommand = KnihovnaVM.RemoveSongCommand;
        }

        /// <summary>
        /// Metoda slouží k upravení playlistu
        /// </summary>
        /// <param name="playlist">Playlist, který chceme upravit</param>
        public void UpravitPlaylist(PlayList playlist)
        {
            bool upravovanyPlaylistPraveHraje = PrehravacVM.ZdrojPrehravani == playlist.Nazev;

            var vm = new PlaylistEditorDialogViewModel(
                KnihovnaVM.Skladby!,
                playlist.Skladby,
                playlist
            );

            var dialog = new PlaylistEditorDialogView
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            vm.ZavritDialog += async potvrdit =>
            {
                dialog.DialogResult = potvrdit;

                if (potvrdit)
                {
                    playlist.Nazev = vm.NazevPlaylistu;
                    playlist.Skladby.Clear();
                    playlist.CestyKSkladbam.Clear();

                    foreach (Song song in vm.PlaylistSkladby)
                    {
                        playlist.Skladby.Add(song);
                        playlist.CestyKSkladbam.Add(song.CestaKSouboru);
                    }

                    await _playlistService.Save(PlaylistVM.Playlisty!);
                    PlaylistVM.RefreshPlaylisty();

                    if (upravovanyPlaylistPraveHraje)
                    {
                        PrehravacVM.ZdrojPrehravani = playlist.Nazev;

                        // Hledání skladby, co právě hraje
                        Song? hrajiciSkladbaVNovémSeznamu = playlist.Skladby.FirstOrDefault(s => s.CestaKSouboru == PrehravacVM.AktualniSkladba?.CestaKSouboru);

                        if (hrajiciSkladbaVNovémSeznamu != null)
                        {
                            // Pokud hrající skladba v playlistu zůstala, jen přenačteme frontu bez stopnutí
                            PrehravacVM.SetPlaylist(playlist.Skladby, hrajiciSkladbaVNovémSeznamu, playlist.Nazev);
                        }

                        else
                        {
                            // Pokud byla hrající skladbu z playlistu vyhozena, všechno stopneme
                            PrehravacVM.SetPlaylist(playlist.Skladby, null, playlist.Nazev);
                        }
                    }

                    if (KnihovnaVM.VybranyPlaylist == playlist)
                    {
                        KnihovnaVM.VybranyPlaylist = null;
                        KnihovnaVM.VybranyPlaylist = playlist;
                    }
                }

                dialog.Close();
            };

            dialog.ShowDialog();
        }
    }
}
