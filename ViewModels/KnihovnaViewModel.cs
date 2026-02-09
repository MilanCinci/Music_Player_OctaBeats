using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Services.KnihovnaSkladeb;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Linq;

namespace Hudebni_Prehravac_OctaBeats.ViewModels
{
    /// <summary>
    /// ViewModel pro obsluhu metod knihovny skladeb
    /// </summary>
    public class KnihovnaViewModel : BaseViewModel
    {
        private readonly IKnihovnaService _knihovnaService;

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

                if (value != null)
                {
                    // Skladba se spustí jen pokud od posledního výběru uběhlo aspoň 200ms
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
        public KnihovnaViewModel(IKnihovnaService knihovnaService)
        {
            _knihovnaService = knihovnaService;
            AddSongCommand = new AsyncRelayCommand(PridejSkladbuDoKnihovny);
            RemoveSongCommand = new RelayCommand(
                 param => OdeberVybranouSkladbuZKnihovny(param),
                 param => VybranyPlaylist == null && (param is Song || VybranaSkladba != null)
            );
            EditMetadataCommand = new RelayCommand(param => { if (param is Song s) SkladbaEditacePozadovana?.Invoke(s); });

            // Výchozí typ vyhledávání
            VybranyTypVyhledavani = TypVyhledavani.Nazev;
            _ = InicializujAsync();
        }

        /// <summary>
        /// Pomocná motoda pro asynchronní načtení knihovny skladeb
        /// </summary>
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
                MessageBox.Show($"Nepodařilo se načíst knihovnu: {ex.Message} !");
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

            VyfiltrovaneSkladby.Clear();

            if (VybranyPlaylist != null)
            {
                vyhledavanyText = String.Empty;
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

        //TODO Dodělat tady to mazání, aby se nedělal focus při pravým kliknutí

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

            foreach (var s in Skladby)
            {
                if (String.IsNullOrWhiteSpace(VyhledavanyText))
                {
                    VyfiltrovaneSkladby.Add(s);
                    continue;
                }

                bool shoda = false;

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
                }

                if (shoda)
                {
                    VyfiltrovaneSkladby.Add(s);
                }
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

        /// <summary>
        /// Metoda slouží k odebrání skladby ze složky MyMusic.
        /// Podporuje smazání vybrané skladby i skladby předané parametrem (pravý klik).
        /// </summary>
        public void OdeberVybranouSkladbuZKnihovny(object? parameter)
        {
            // Získáme skladbu: buď z parametru (ContextMenu), nebo aktuálně vybranou.
            Song? skladbaKeSmazani = parameter as Song ?? VybranaSkladba;

            // Pokud nemáme ani jedno, nemáme co mazat.
            if (skladbaKeSmazani == null)
            {
                return;
            }

            try
            {
                // Používáme MessageBox pro potvrzení
                DialogResult vysledekDiaOkna = MessageBox.Show(
                    $"Opravdu chcete vymazat skladbu '{skladbaKeSmazani.Nazev}' ?",
                    "Potvrdit smazání",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (vysledekDiaOkna == DialogResult.Yes)
                {
                    // 1. Signál pro přehrávač a playlisty, aby přestaly soubor používat
                    SkladbaSmazana?.Invoke(skladbaKeSmazani);

                    // 2. Fyzické smazání z disku
                    bool uspesneSmazano = _knihovnaService.DeleteSongFromMyMusic(skladbaKeSmazani.CestaKSouboru);

                    if (uspesneSmazano)
                    {
                        // 3. Odstranění z kolekcí v UI
                        Skladby?.Remove(skladbaKeSmazani);
                        VyfiltrovaneSkladby?.Remove(skladbaKeSmazani);

                        // 4. Pokud smazaná skladba byla ta vybraná (přehrávaná), vynulujeme výběr
                        if (VybranaSkladba == skladbaKeSmazani)
                        {
                            VybranaSkladba = null;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Soubor se nepodařilo smazat. Možná je používán jiným programem/procesem!", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}