using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Services;
using Hudebni_Prehravac_OctaBeats.Services.Audio;
using Hudebni_Prehravac_OctaBeats.Services.Historie;
using Hudebni_Prehravac_OctaBeats.Services.KnihovnaSkladeb;
using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
using Hudebni_Prehravac_OctaBeats.Services.Metadata;
using Hudebni_Prehravac_OctaBeats.Services.NastaveniAudia;
using Hudebni_Prehravac_OctaBeats.Services.Playlist;
using Hudebni_Prehravac_OctaBeats.Views;
using System;
using System.IO;
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
        public ICommand RefreshChangesCommand { get; }

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

            // Akce při smazání playlistu, který se právě přehrává
            PlaylistVM.PlaylistSmazan += playlist =>
            {
                PrehravacVM.VymazFrontuPrehravani(playlist);
            };

            // Akce při editaci metadat vybrané skladby
            KnihovnaVM.SkladbaEditacePozadovana += song => 
            {
                UpravitMetadata(song);
                
            };

            // Akce při nenalezení souboru skladby
            _audioService.SouborNenalezen += async (cestaKSouboru) =>
            {
                await RefreshVsechDat();
                MessageBox.Show($"Soubor '{cestaKSouboru}' neexistuje! Knihovna a playlistu jsou aktualizovány",
                                "Playing Error ", MessageBoxButton.OK, MessageBoxImage.Information);
            };

            AddSongCommand = KnihovnaVM.AddSongCommand;
            RemoveSongCommand = KnihovnaVM.RemoveSongCommand;
            RefreshChangesCommand = new AsyncRelayCommand(RefreshVsechDat);
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
                playlist,
                PlaylistVM.Playlisty!
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

        /// <summary>
        /// Metoda slouží k úprávě metadat vybrané skladby
        /// </summary>
        /// <param name="song">Skladba, kterou chceme editovat</param>
        public void UpravitMetadata(Song song)
        {
            var vm = new SongMetadataEditorViewModel(song);
            var dialog = new SongMetadataEditorView
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            vm.ZavritDialog += async potvrdit =>
            {
                if (potvrdit)
                {
                    bool hrajeTatoSkladba = PrehravacVM.AktualniSkladba != null && PrehravacVM.AktualniSkladba.CestaKSouboru == song.CestaKSouboru;

                    TimeSpan poziceVPrehravaci = TimeSpan.Zero;

                    // Pokud hraje editováná skladba, musíme ji zastavit a vypustit zdroje, aby se uvolnil proces pro uložení!
                    if (hrajeTatoSkladba)
                    {
                        poziceVPrehravaci = _audioService.AktualniCas;
                        _audioService.Stop();
                    }

                    // Aktualizace metadat skladby                   
                    song.Nazev = vm.Nazev;
                    if (String.IsNullOrWhiteSpace(vm.Interpret))
                    {
                        song.Interpret = "Unknown";
                    }

                    else
                    {
                        song.Interpret = vm.Interpret;
                    }

                    if (String.IsNullOrWhiteSpace(vm.Album))
                    {
                        song.Album = "Unknown";
                    }

                    else
                    {
                        song.Album = vm.Album;
                    }

                    song.Zanr = vm.Zanr;
                    if (uint.TryParse(vm.RokVydani, out uint rok))
                    {
                        song.RokVydani = rok;
                    }

                    song.PrebalAlba = vm.PrebalAlba;

                    try
                    {
                        IMetadataService metadataService = new MetadataService();
                        await metadataService.Save(song);

                        // Aktualizace stejné skladby ve všech playlistech
                        if (PlaylistVM.Playlisty != null)
                        {
                            foreach (PlayList playlist in PlaylistVM.Playlisty)
                            {
                                Song? songInPlaylist = playlist.Skladby.FirstOrDefault(s => s.CestaKSouboru == song.CestaKSouboru);
                                if (songInPlaylist != null)
                                {
                                    // Přepsání dat v objektu, který drží playlist
                                    songInPlaylist.Nazev = song.Nazev;
                                    songInPlaylist.Interpret = song.Interpret;
                                    songInPlaylist.Album = song.Album;
                                    songInPlaylist.Zanr = song.Zanr;
                                    songInPlaylist.RokVydani = song.RokVydani;
                                }
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    // Pokud hraje editovaná skladba, tak ji musíme zastavit a poté znovu přehrát
                    if (hrajeTatoSkladba)
                    {
                        await _audioService.Play(song.CestaKSouboru);
                        _audioService.Pause();
                        _audioService.Seek(poziceVPrehravaci);
                        PrehravacVM.IsPlaying = false;
                        PrehravacVM.RefreshAktualniSkladbu();
                    }

                    //Refresh všech potřebných UI komponent
                    KnihovnaVM.RefreshKnihovnu();
                    PlaylistVM.RefreshPlaylisty();

                    // Pokud je v knihovně zobrazen playlist, který jsme právě změnili, musíme ho znovu prokliknout, aby se refreshnul
                    if (KnihovnaVM.VybranyPlaylist != null)
                    {
                        PlayList? aktualni = KnihovnaVM.VybranyPlaylist;
                        KnihovnaVM.VybranyPlaylist = null;
                        KnihovnaVM.VybranyPlaylist = aktualni;
                    }
                }

                dialog.Close();
            };

            dialog.ShowDialog();
        }

        /// <summary>
        /// Metoda slouží k globálnímu refresh celé aplikace (načte znovu disk i playlisty)
        /// </summary>
        /// <returns>Vrací Task</returns>
        public async Task RefreshVsechDat()
        {
            try
            {
                // Načtení skladeb znovu do knihovny
                await KnihovnaVM.InicializujAsync();

                // Procházení všech playlistů a odstranění z nich skladby, které již neexistují na disku
                if (PlaylistVM.Playlisty != null)
                {
                    bool bylaZmena = false;
                    foreach (PlayList playlist in PlaylistVM.Playlisty)
                    {
                        // Najdeme všechny skladby, které už fyzicky neexistují
                        var kOdstraneni = playlist.Skladby.Where(s => !File.Exists(s.CestaKSouboru)).ToList();

                        foreach (Song song in kOdstraneni)
                        {
                            playlist.Skladby.Remove(song);
                            playlist.CestyKSkladbam.Remove(song.CestaKSouboru);
                            bylaZmena = true;
                        }
                    }

                    if (bylaZmena)
                    {
                        await _playlistService.Save(PlaylistVM.Playlisty);
                        PlaylistVM.RefreshPlaylisty();
                        PlaylistVM.VybranyPlaylist = null;
                    }
                }

                // reset přehrávače, pokud hrající skladba zmizela
                if (PrehravacVM.AktualniSkladba != null && !File.Exists(PrehravacVM.AktualniSkladba.CestaKSouboru))
                {
                    _audioService.Stop();
                    PrehravacVM.AktualniSkladba = null;
                    PrehravacVM.IsPlaying = false;
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při synchronizaci změn: {ex.Message} !");
            }
        }
    }
}
