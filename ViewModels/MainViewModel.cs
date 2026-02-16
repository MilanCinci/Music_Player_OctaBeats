using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using Hudebni_Prehravac_OctaBeats.Services;
using Hudebni_Prehravac_OctaBeats.Services.Audio;
using Hudebni_Prehravac_OctaBeats.Services.Dialog;
using Hudebni_Prehravac_OctaBeats.Services.Historie;
using Hudebni_Prehravac_OctaBeats.Services.KnihovnaSkladeb;
using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
using Hudebni_Prehravac_OctaBeats.Services.Metadata;
using Hudebni_Prehravac_OctaBeats.Services.NastaveniAudia;
using Hudebni_Prehravac_OctaBeats.Services.Playlist;
using Hudebni_Prehravac_OctaBeats.Views;
using System;
using System.IO;
using System.Security.Cryptography;
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
        private readonly IDialogService _dialogService;

        /// <summary>
        /// Výchozí název zdroje přehrávání, pokud není uveden
        /// </summary>
        private static string VychoziNazevZdroje = "Knihovna";

        /* Příkazy pro obsluhu jednotlivých metod */
        public ICommand AddSongCommand { get; }
        public ICommand RemoveSongCommand { get; }
        public ICommand RefreshChangesCommand { get; }
        public ICommand RemoveSelectedHistoryCommand { get; }
        public ICommand RemoveAllHistoryCommand { get; }
        public ICommand OpenSettingsLanguageCommand { get; }
        public ICommand ExitAppCommand { get; }
        public ICommand ChangeThemeCommand { get; }

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
        public NastaveniJazykViewModel NastaveniJazykVM { get; }

        /// <summary>
        /// Bezparametrický konstruktor pro inicializaci
        /// </summary>
        public MainViewModel()
        {
            _dialogService = new DialogService();
            _lokalizaceService = new LokalizaceService();
            _playlistService = new PlaylistService();
            _historieService = new HistoryService();
            _audioService = new AudioService(_lokalizaceService);
            _nastaveniAudiaService = new NastaveniAudiaService();
            _knihovnaService = new KnihovnaService(_lokalizaceService, _dialogService);

            PrehravacVM = new PrehravacViewModel(
                _audioService,
                _historieService,
                _nastaveniAudiaService,
                _lokalizaceService,
                _dialogService);

            PlaylistVM = new PlaylistViewModel(_playlistService, _lokalizaceService, _dialogService);
            KnihovnaVM = new KnihovnaViewModel(_knihovnaService, _lokalizaceService, _dialogService);
            HistoryVM = new HistoryViewModel(_historieService, _lokalizaceService, _dialogService);
            NastaveniJazykVM = new NastaveniJazykViewModel(_lokalizaceService, _dialogService);

            // Načtení a nastavení jazyka z Application Properties
            string ulozenyJazyk = Properties.Settings.Default.Language;
            _lokalizaceService.ChangeLanguage(ulozenyJazyk);

            // Načtení a nastavení vzhledu aplikace z Application Properties
            bool jeTmavyRezim = Properties.Settings.Default.IsDarkMode;
            ZmenVzhledAplikace(jeTmavyRezim);

            // Prvotní načtení správně přeložených ComboboxItemů
            KnihovnaVM.RefreshLokalizace();

            // Propojení playlistů s knihovnou
            PlaylistVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PlaylistViewModel.VybranyPlaylist))
                {
                    KnihovnaVM.VybranyPlaylist = PlaylistVM.VybranyPlaylist;
                }
            };

            // Událost při výběru skladby v knihovně
            KnihovnaVM.SkladbaVybrana += skladba =>
            {
                if (PrehravacVM.AktualniSkladba != skladba)
                {
                    string zdroj = KnihovnaVM.VybranyPlaylist?.Nazev ?? VychoziNazevZdroje;
                    PrehravacVM.SetPlaylist(KnihovnaVM.VyfiltrovaneSkladby!, skladba, zdroj);
                }
            };

            // Propojení přehrávače s knihovnou
            PrehravacVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PrehravacVM.AktualniSkladba))
                {
                    KnihovnaVM.NastavVybranouSkladbu(
                        PrehravacVM.AktualniSkladba);
                }
            };

            // Událost při nenalezení skladby (ošetření chyby + refresh)
            _audioService.SouborNenalezen += async (cestaKSouboru) =>
            {
                await RefreshVsechDat();
                _dialogService.ShowInfo(String.Format(_lokalizaceService["InfoFileNotFoundRefreshData"], cestaKSouboru));
            };

            // Událost při smazání skladby z knihovny
            KnihovnaVM.SkladbaSmazana += async skladba =>
            {
                PrehravacVM.OdstranSkladbuZFronty(skladba);
                await PlaylistVM.RemoveSongFromPlaylist(skladba);
            };

            // Událost při smazání playlistu 
            PlaylistVM.PlaylistSmazan += playlist => PrehravacVM.VymazFrontuPrehravani(playlist);

            // Událost při požádání editace metadat skladby
            KnihovnaVM.SkladbaEditacePozadovana += song => UpravitMetadata(song);

            AddSongCommand = KnihovnaVM.AddSongCommand;
            RemoveSongCommand = KnihovnaVM.RemoveSongCommand;
            RefreshChangesCommand = new AsyncRelayCommand(RefreshVsechDat);
            RemoveSelectedHistoryCommand = HistoryVM.RemoveSelectedHistoryCommand;
            RemoveAllHistoryCommand = HistoryVM.RemoveAllHistoryCommand;
            OpenSettingsLanguageCommand = new RelayCommand(_ => OtevriNastaveniJazyka());
            ExitAppCommand = new RelayCommand(_ => UkonciAplikaci());
            ChangeThemeCommand = new RelayCommand(vzhled => ZmenVzhledAplikace(vzhled));
        }

        /// <summary>
        /// Metoda slouží k upravení playlistu
        /// </summary>
        /// <param name="playlist">Playlist, který chceme upravit</param>
        public void UpravitPlaylist(PlayList playlist)
        {
            try
            {
                bool upravovanyPlaylistPraveHraje = PrehravacVM.ZdrojPrehravani == playlist.Nazev;

                var vm = new PlaylistEditorDialogViewModel(
                    KnihovnaVM.Skladby!,
                    playlist.Skladby,
                    playlist,
                    PlaylistVM.Playlisty!,
                    _lokalizaceService
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
                        if (vm.NazevPlaylistu == null)
                        {
                            return;
                        }

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

                        // Kontrola, zda upravovaný playlist je také právě přehrávaný
                        if (upravovanyPlaylistPraveHraje)
                        {
                            PrehravacVM.ZdrojPrehravani = playlist.Nazev;

                            // Hledání skladby, která právě hraje
                            Song? hrajiciSkladbaVNovemSeznamu = playlist.Skladby.FirstOrDefault(s => s.CestaKSouboru == PrehravacVM.AktualniSkladba?.CestaKSouboru);

                            if (hrajiciSkladbaVNovemSeznamu != null)
                            {
                                // Pokud hrající skladba v playlistu zůstala, jen přenačteme frontu bez stopnutí
                                PrehravacVM.SetPlaylist(playlist.Skladby, hrajiciSkladbaVNovemSeznamu, playlist.Nazev);
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

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while editing the selected playlist!", nameof(UpravitPlaylist));
                _dialogService.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Metoda slouží k úprávě metadat vybrané skladby
        /// </summary>
        /// <param name="song">Skladba, kterou chceme editovat</param>
        public void UpravitMetadata(Song song)
        {
            try
            {
                var vm = new SongMetadataEditorViewModel(song, _lokalizaceService, _dialogService);
                var dialog = new SongMetadataEditorView
                {
                    DataContext = vm,
                    Owner = Application.Current.MainWindow
                };

                vm.ZavritDialog += async potvrdit =>
                {
                    if (potvrdit)
                    {
                        bool hrajeTatoSkladba = PrehravacVM.AktualniSkladba != null && 
                                                PrehravacVM.AktualniSkladba.CestaKSouboru == song.CestaKSouboru;

                        TimeSpan poziceVPrehravaci = TimeSpan.Zero;

                        // Pokud hraje editovaná skladba, musíme ji zastavit a vypustit zdroje, aby se uvolnil proces pro uložení!
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

                        IMetadataService metadataService = new MetadataService(_lokalizaceService);
                        await metadataService.Save(song);

                        // Aktualizace stejné skladby ve všech playlistech
                        if (PlaylistVM.Playlisty != null)
                        {
                            foreach (PlayList playlist in PlaylistVM.Playlisty)
                            {
                                Song? skladbaVPlaylistu = playlist.Skladby.FirstOrDefault(s => s.CestaKSouboru == song.CestaKSouboru);
                                if (skladbaVPlaylistu != null)
                                {
                                    // Přepsání dat v objektu, který drží playlist
                                    skladbaVPlaylistu.Nazev = song.Nazev;
                                    skladbaVPlaylistu.Interpret = song.Interpret;
                                    skladbaVPlaylistu.Album = song.Album;
                                    skladbaVPlaylistu.Zanr = song.Zanr;
                                    skladbaVPlaylistu.RokVydani = song.RokVydani;
                                }
                            }
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

                        // Refresh všech potřebných UI komponent
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

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while editing the song metadata!", nameof(UpravitMetadata));
                _dialogService.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Metoda slouží k otevření dialogu pro úpravu jazyka aplikace
        /// </summary>
        private void OtevriNastaveniJazyka()
        {
            try
            {
                var dialog = new NastaveniJazykView
                {
                    DataContext = NastaveniJazykVM,
                    Owner = Application.Current.MainWindow
                };

                NastaveniJazykVM.ZavritDialog += potvrdit =>
                {
                    if (potvrdit)
                    {
                        NastaveniJazykVM.ZmenJazyk(NastaveniJazykVM.VybranyJazyk?.Kod ?? NastaveniJazykVM.DostupneJazyky.
                                                        First(jazyk => jazyk.Nazev.Equals("English", StringComparison.OrdinalIgnoreCase)).Kod);
                        PlaylistVM.RefreshLokalizace();
                        PrehravacVM.RefreshLokalizace();
                        HistoryVM.RefreshLokalizace();
                        KnihovnaVM.RefreshLokalizace();
                    }

                    dialog.Close();
                };

                dialog.ShowDialog();
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "", nameof(OtevriNastaveniJazyka));
                _dialogService.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Metoda slouží k globálnímu refresh celé aplikace (načte znovu disk i playlisty)
        /// </summary>
        /// <returns>Vrací Task</returns>
        public async Task RefreshVsechDat()
        {
            try
            {
                // TODO
                // Doladit design přepínání mezi světlým a tmavým režimem
                // Doladit celkově trochu design
                // Pomocí klávesové zkratky, odfocusovat vybraný záznam z historie

                // Načtení skladeb znovu do knihovny
                await KnihovnaVM.InicializujAsync();

                // Synchronizace fronty v přehrávači (odstranění neexistujících souborů z fronty)
                if (PrehravacVM.Playlist != null && PrehravacVM.Playlist.Count > 0)
                {
                    // Nalezení skladeb ve frontě, které už fyzicky neexistují
                    var neexistujiciVeFronte = PrehravacVM.Playlist
                        .Where(skladba => !File.Exists(skladba.CestaKSouboru))
                        .ToList();

                    foreach (Song song in neexistujiciVeFronte)
                    {
                        PrehravacVM.OdstranSkladbuZFronty(song);
                    }
                }

                // Procházení všech playlistů a odstranění z nich skladby, které již neexistují na disku
                if (PlaylistVM.Playlisty != null)
                {
                    bool bylaZmena = false;
                    foreach (PlayList playlist in PlaylistVM.Playlisty)
                    {
                        // Nalezení všech skladeb, které už fyzicky neexistují
                        var kOdstraneni = playlist.Skladby.Where(skladba => !File.Exists(skladba.CestaKSouboru)).ToList();

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

                // Reset přehrávače, pokud hrající skladba zmizela
                if (PrehravacVM.AktualniSkladba != null && !File.Exists(PrehravacVM.AktualniSkladba.CestaKSouboru))
                {
                    _audioService.Stop();
                    PrehravacVM.AktualniSkladba = null;
                    PrehravacVM.IsPlaying = false;
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "", nameof(RefreshVsechDat));
                _dialogService.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Metoda slouží k ukončení celé aplikace
        /// </summary>
        public void UkonciAplikaci()
        {
            try
            {
                App.Current.Shutdown();
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while closing the application!", nameof(UkonciAplikaci));
                _dialogService.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Metoda slouží k dynamickému přepnutí vzhledu aplikace
        /// </summary>
        /// <param name="vzhled">Vzhled aplikace, který jsem nově zvolili</param>
        public void ZmenVzhledAplikace(object? vzhled)
        {
            if (vzhled == null)
            {
                return;
            }

            bool tmavyRezim = false;
            if (vzhled is bool b)
            {
                tmavyRezim = b;
            }

            else if (vzhled is string s)
            {
                bool.TryParse(s, out tmavyRezim);
            }

            string motivPath = String.Empty;

            if (tmavyRezim)
            {
                motivPath = "Resources/Themes/DarkTheme.xaml";
            }

            else
            {
                motivPath = "Resources/Themes/LightTheme.xaml";
            }

            try
            {
                var appResources = Application.Current.Resources.MergedDictionaries;

                // Nalezení a odstranění stávajícího slovníku s motivem
                var staryMotiv = appResources.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme.xaml"));

                if (staryMotiv != null)
                {
                    appResources.Remove(staryMotiv);
                }

                // Přidáme nový slovník
                appResources.Add(new ResourceDictionary 
                {
                    Source = new Uri(motivPath, UriKind.Relative) 
                });

                // Uložení nového volby vzhledu do Application Properties
                Properties.Settings.Default.IsDarkMode = tmavyRezim;
                Properties.Settings.Default.Save();

                // Refresh lokalizace pro všechny ViewModely, aby se aktualizovaly barvy vázané na indexery
                PlaylistVM.RefreshLokalizace();
                KnihovnaVM.RefreshLokalizace();
                PrehravacVM.RefreshLokalizace();
                HistoryVM.RefreshLokalizace();
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while changing the app theme!", nameof(ZmenVzhledAplikace));
                _dialogService.ShowError(ex.Message);
            }
        }
    }
}
