using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Services.KnihovnaSkladeb;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System.Windows.Forms;
using Hudebni_Prehravac_OctaBeats.Services.Dialog;
using Hudebni_Prehravac_OctaBeats.Persistence;
using System.Windows;
using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;

namespace Hudebni_Prehravac_OctaBeats.ViewModels
{
    /// <summary>
    /// ViewModel pro obsluhu metod knihovny skladeb
    /// </summary>
    public class KnihovnaViewModel : BaseViewModel
    {
        private readonly IKnihovnaService _knihovnaService;
        private readonly ILokalizaceService _lokalizaceService;
        private readonly IDialogService _dialogService;
        private bool potlacVyber;

        private ObservableCollection<Song>? skladby;
        /// <summary>
        /// Seznam skladeb
        /// </summary>
        public ObservableCollection<Song>? Skladby
        {
            get => skladby;
            set { skladby = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Song>? vyfiltrovaneSkladby;
        /// <summary>
        /// Seznam vyfiltrovaných skladeb podle zadaných vyhledávacích kritérií
        /// </summary>
        public ObservableCollection<Song>? VyfiltrovaneSkladby
        {
            get => vyfiltrovaneSkladby;
            set { vyfiltrovaneSkladby = value; OnPropertyChanged(); }
        }

        private DateTime posledniVyber = DateTime.MinValue;

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

        /// <summary>
        /// Akce pro vymazanou skladbu
        /// </summary>
        public event Action<Song>? SkladbaSmazana;

        /// <summary>
        /// Akce pro editaci metadat skladby
        /// </summary>
        public event Action<Song>? SkladbaEditacePozadovana;

        private Song? vybranaSkladba;
        public Song? VybranaSkladba
        {
            get => vybranaSkladba;
            set
            {
                if (vybranaSkladba == value)
                {
                    return;
                }

                vybranaSkladba = value;
                OnPropertyChanged();

                if (value != null && !potlacVyber)
                {
                    DateTime nyni = DateTime.Now;
                    if ((nyni - posledniVyber).TotalMilliseconds > 200)
                    {
                        posledniVyber = nyni;
                        SkladbaVybrana?.Invoke(value);
                    }
                }
            }
        }

        /* Příkazy pro obsluhu jednotlivých metod */
        public ICommand AddSongCommand { get; }
        public ICommand RemoveSongCommand { get; }
        public ICommand EditMetadataCommand { get; }

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="knihovnaService">Servis pro obsluhu metod knihovny skladeb</param>
        /// <param name="lokalizaceService">Servis pro obsluhu metod lokalizace aplikace</param>
        /// <param name="dialogService">Servis pro zobrazení příslušných dialogů</param>
        public KnihovnaViewModel(IKnihovnaService knihovnaService, ILokalizaceService lokalizaceService, IDialogService dialogService)
        {
            _lokalizaceService = lokalizaceService;
            _knihovnaService = knihovnaService;
            _dialogService = dialogService;

            AddSongCommand = new AsyncRelayCommand(PridejSkladbuDoKnihovny);
            RemoveSongCommand = new RelayCommand(
                 param => OdeberVybranouSkladbuZKnihovny(param),
                 param => VybranyPlaylist == null && (param is Song || VybranaSkladba != null)
            );
            EditMetadataCommand = new RelayCommand(param => 
            {
                if (param is Song song)
                {
                    SkladbaEditacePozadovana?.Invoke(song);
                }
            });

            // Výchozí typ vyhledávání
            VybranyTypVyhledavani = TypVyhledavani.Nazev;

            // Asynchronní inicializace knihovny
            _ = InicializujAsync();
        }

        /// <summary>
        /// Metoda slouží k asynchronnímu načtení knihovny skladeb
        /// </summary>
        /// <returns>Vrací Task</returns>
        public async Task InicializujAsync()
        {
            try
            {
                Skladby = await _knihovnaService.Load()!;
                VyfiltrovaneSkladby = new ObservableCollection<Song>(Skladby);
                OnPropertyChanged(nameof(Skladby));
                OnPropertyChanged(nameof(VyfiltrovaneSkladby));
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while initializing the library!", nameof(InicializujAsync));
                _dialogService.ShowError(ex.Message);
            }
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

            try
            {
                VyfiltrovaneSkladby.Clear();

                if (VybranyPlaylist != null)
                {
                    vyhledavanyText = String.Empty;
                    OnPropertyChanged(nameof(VyhledavanyText));
                    foreach (Song skladba in VybranyPlaylist.Skladby)
                    {
                        VyfiltrovaneSkladby.Add(skladba);
                    }
                }

                else if (Skladby != null)
                {
                    Vyfiltruj();
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while changing the source of the playback history!", nameof(PrepnoutZdrojSkladeb));
                _dialogService.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Metoda slouží k vyfiltrování skladeb podle zvoleného kritéria
        /// </summary>
        private void Vyfiltruj()
        {
            if (VyfiltrovaneSkladby == null || Skladby == null)
            {
                return;
            }

            VyfiltrovaneSkladby.Clear();

            // Zakázání vyhledávácího pole v knihovně, když je vybraný playlist
            if (VybranyPlaylist != null)
            {
                return;
            }

            try
            {
                foreach (Song skladba in Skladby)
                {
                    if (String.IsNullOrWhiteSpace(VyhledavanyText))
                    {
                        VyfiltrovaneSkladby.Add(skladba);
                        continue;
                    }

                    bool shoda = false;

                    switch (VybranyTypVyhledavani)
                    {
                        case TypVyhledavani.Nazev:
                            shoda = skladba.Nazev.Contains(VyhledavanyText, StringComparison.OrdinalIgnoreCase);
                            break;

                        case TypVyhledavani.Interpret:
                            if (skladba.Interpret != null)
                            {
                                shoda = skladba.Interpret.Contains(VyhledavanyText, StringComparison.OrdinalIgnoreCase);
                            }
                            break;

                        case TypVyhledavani.Zanr:
                            if (skladba.Zanr != null)
                            {
                                shoda = skladba.Zanr.Contains(VyhledavanyText, StringComparison.OrdinalIgnoreCase);
                            }
                            break;
                    }

                    if (shoda)
                    {
                        VyfiltrovaneSkladby.Add(skladba);
                    }
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while filtering songs!", nameof(Vyfiltruj));
                _dialogService.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Metoda slouží k refreshnutí změn ve View
        /// </summary>
        public void RefreshKnihovnu()
        {
            OnPropertyChanged(nameof(Skladby));
            OnPropertyChanged(nameof(VyfiltrovaneSkladby));
            PrepnoutZdrojSkladeb();
        }

        /// <summary>
        /// Metoda slouží k asynchronnímu překopírování vybraných skladeb do složky MyMusic
        /// </summary>
        /// <returns>Vrací Task</returns>
        public async Task PridejSkladbuDoKnihovny()
        {
            try
            {
                OpenFileDialog fileDialog = new OpenFileDialog
                {
                    Title = "Select music files to copy",
                    Multiselect = true,
                    Filter = "Music files (*.mp3;*.wav;*.flac)|*.mp3;*.wav;*.flac"
                };

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    await _knihovnaService.CopySongsToMyMusic(fileDialog.FileNames);
                    Skladby = await _knihovnaService.Load()!;
                    PrepnoutZdrojSkladeb();
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "", nameof(PridejSkladbuDoKnihovny));
                _dialogService.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Metoda slouží k odebrání skladby ze složky MyMusic. Jako vstup používá <paramref name="parameter"/>
        /// </summary>
        /// <param name="parameter">Objekt představující vybranou skladbu</param>
        public void OdeberVybranouSkladbuZKnihovny(object? parameter)
        {
            // Získání skladby z parametru ContextMenu nebo vybraná skladba
            Song? skladbaKeSmazani = parameter as Song ?? VybranaSkladba;

            if (skladbaKeSmazani == null)
            {
                return;
            }

            try
            {
                string zprava = String.Format(_lokalizaceService["QuestionDeleteItemFromLibrary"], skladbaKeSmazani.Nazev);
                MessageBoxResult vysledekDiaOkna = _dialogService.ShowConfirmation(zprava);

                if (vysledekDiaOkna == MessageBoxResult.Yes)
                {
                    // Signál pro přehrávač a playlisty, aby přestaly soubor používat
                    SkladbaSmazana?.Invoke(skladbaKeSmazani);

                    // Fyzické smazání z MyMusic
                    bool uspesneSmazano = _knihovnaService.DeleteSongFromMyMusic(skladbaKeSmazani.CestaKSouboru);

                    if (uspesneSmazano)
                    {
                        Skladby?.Remove(skladbaKeSmazani);
                        VyfiltrovaneSkladby?.Remove(skladbaKeSmazani);

                        // Pokud smazaná skladba byla ta vybraná (přehrávaná), vynulujeme výběr
                        if (VybranaSkladba == skladbaKeSmazani)
                        {
                            VybranaSkladba = null;
                        }
                    }

                    else
                    {
                        _dialogService.ShowWarning(_lokalizaceService["ErrorCannotDeleteFile"]);
                    }
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while deleting the song from the library!", nameof(OdeberVybranouSkladbuZKnihovny));
                _dialogService.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Metoda slouží k nastavení skladby na aktuálně vybranou
        /// </summary>
        /// <param name="skladba">Skladba, která se má nastavit jako aktuálně vybraná skladba</param>
        public void NastavVybranouSkladbu(Song? skladba)
        {
            if (skladba == null || VyfiltrovaneSkladby == null)
            {
                return;
            }

            try
            {
                potlacVyber = true;
                VybranaSkladba = VyfiltrovaneSkladby.FirstOrDefault(s => s.CestaKSouboru == skladba.CestaKSouboru);
                potlacVyber = false;
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while setting the currently selected song!", nameof(NastavVybranouSkladbu));
                _dialogService.ShowError(ex.Message);
            }
        }
    }
}