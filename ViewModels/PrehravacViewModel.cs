using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using Hudebni_Prehravac_OctaBeats.Services;
using Hudebni_Prehravac_OctaBeats.Services.Audio;
using Hudebni_Prehravac_OctaBeats.Services.Historie;
using Hudebni_Prehravac_OctaBeats.Services.NastaveniAudia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Hudebni_Prehravac_OctaBeats.ViewModels
{
    /// <summary>
    /// ViewModel pro obsluhu metod přehrávače
    /// </summary>
    public class PrehravacViewModel : BaseViewModel
    {
        private readonly IAudioService _audioService;
        private readonly IHistorieService _historieService;
        private readonly INastaveniAudiaService _nastaveniAudiaService;
        private bool uzivatelPosouvaSlider;
        private readonly DispatcherTimer _timerUlozeniHlasitosti;

        /// <summary>
        /// Konstanta pro výchozí hlasitost skladby
        /// </summary>
        private const float VychoziHlasitost = 0.7f;

        /// <summary>
        /// Seznam skladeb v daném playlistu
        /// </summary>
        public ObservableCollection<Song> Playlist { get; } = new ObservableCollection<Song>();

        /// <summary>
        /// Aktuální index skladby, která se právě přehrává
        /// </summary>
        private int aktualniIndex = -1;

        private bool isPlaying;
        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }

        private float hlasitost;
        public float Hlasitost
        {
            get => hlasitost;
            set
            {
                if (Math.Abs(hlasitost - value) > 0.1)
                {
                    hlasitost = value;
                    _audioService.Hlasitost = value / 100f;

                    // Restartování časovače pro uložení do souboru
                    _timerUlozeniHlasitosti.Stop();
                    _timerUlozeniHlasitosti.Start();

                    OnPropertyChanged();
                }
            }
        }

        private string zdrojPrehravani;
        public string ZdrojPrehravani
        {
            get => zdrojPrehravani;
            set
            {
                zdrojPrehravani = value;
                OnPropertyChanged();
            }
        }

        private Song? aktualniSkladba;
        public Song? AktualniSkladba
        {
            get => aktualniSkladba;
            set
            {
                aktualniSkladba = value;
                OnPropertyChanged();
            }
        }

        private double aktualniCas;
        public double AktualniCas
        {
            get => aktualniCas;
            set
            {
                if (Math.Abs(aktualniCas - value) > 0.1)
                {
                    aktualniCas = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AktualniCasText));

                    // Pokud uživatel pohybuje sliderem, změní se pozice v AudioService
                    if (uzivatelPosouvaSlider)
                    {
                        _audioService.Seek(TimeSpan.FromSeconds(value));
                    }
                }
            }
        }

        private double celkovaDelka;
        public double CelkovaDelka
        {
            get => celkovaDelka;
            set
            {
                celkovaDelka = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CelkovaDelkaText));
            }
        }

        /// <summary>
        /// Aktuální čas přehrávání ve formátu mm:ss
        /// </summary>
        public string AktualniCasText => TimeSpan.FromSeconds(AktualniCas).ToString(@"mm\:ss");

        /// <summary>
        /// Celková délka skladby ve formátu mm:ss
        /// </summary>
        public string CelkovaDelkaText => TimeSpan.FromSeconds(CelkovaDelka).ToString(@"mm\:ss");

        /* Příkazy pro obsluhu jednotlivých metod */
        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }

        /// <summary>
        /// Časovač pro časovou osu
        /// </summary>
        private readonly DispatcherTimer _casovac;

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="audioService">Servis pro obsluhu metod audia</param>
        /// <param name="historieService">Servis pro obsluhu metod historie přehrávání</param>
        public PrehravacViewModel(
            IAudioService audioService,
            IHistorieService historieService,
            INastaveniAudiaService nastaveniAudiaService)
        {
            _audioService = audioService;
            _historieService = historieService;
            _nastaveniAudiaService = nastaveniAudiaService;

            // Výchozí hlasitost
            hlasitost = VychoziHlasitost * 100;
            _audioService.Hlasitost = VychoziHlasitost;

            _timerUlozeniHlasitosti = new DispatcherTimer
            {
                // Počkání 0,5 sekundy po posledním pohybu slideru hlasitosti
                Interval = TimeSpan.FromMilliseconds(500)
            };

            _timerUlozeniHlasitosti.Tick += async (s, e) =>
            {
                _timerUlozeniHlasitosti.Stop();
                await _nastaveniAudiaService.Save(new NastaveniAudio(Hlasitost / 100f));
            };

            _audioService.UkonceniSkladby += OnUkonceniSkladby;

            PlayCommand = new RelayCommand(_ => Play());
            PauseCommand = new RelayCommand(_ => Pause());
            StopCommand = new RelayCommand(_ => _audioService.Stop());
            NextCommand = new RelayCommand(_ => Next());
            PreviousCommand = new RelayCommand(_ => Previous());

            _casovac = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            _casovac.Tick += (s, e) =>
            {
                if (!uzivatelPosouvaSlider)
                {
                    double novyCas = _audioService.AktualniCas.TotalSeconds;
                    double novaDelka = _audioService.CelkovyCas.TotalSeconds;

                    AktualniCas = novyCas;
                    CelkovaDelka = novaDelka;
                }
            };

            _casovac.Start();

            // Asynchronní inicializace nastavení audia
            _ = InicializujAsync();
        }

        /// <summary>
        /// Metoda slouží k asynchronní inicializaci nastavení audia
        /// </summary>
        /// <returns>Vrací Task</returns>
        private async Task InicializujAsync()
        {
            try
            {
                NastaveniAudio? ulozeneNastaveni = await _nastaveniAudiaService.Load();

                if (ulozeneNastaveni != null)
                {
                    hlasitost = ulozeneNastaveni.Hlasitost * 100;
                    _audioService.Hlasitost = ulozeneNastaveni.Hlasitost;
                    OnPropertyChanged(nameof(Hlasitost));
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, $"Chyba při načítání nastavení audia ve třídě {nameof(PrehravacViewModel)}");
            }
        }

        /// <summary>
        /// Metoda slouží k posunu posuvníku v závislosti na čase
        /// </summary>
        /// <param name="sekundy">Čas (v sekundách), o kolik se má skladba posunout</param>
        public void Seek(double sekundy)
        {
            _audioService.Seek(TimeSpan.FromSeconds(sekundy));
            IsPlaying = true;
        }

        /// <summary>
        /// Metoda slouží k nastavení a přehrávání vybraného playlistu
        /// </summary>
        /// <param name="skladby">Skladby v playlistu</param>
        /// <param name="vybrana">Vybraná skladba</param>
        public void SetPlaylist(IEnumerable<Song> skladby, Song? vybrana, string nazevZdroje)
        {
            try
            {
                bool uzHrajeStejnaSkladba = AktualniSkladba != null && vybrana != null &&
                                            AktualniSkladba.CestaKSouboru == vybrana.CestaKSouboru;
                ZdrojPrehravani = nazevZdroje;
                Playlist.Clear();
                foreach (var s in skladby)
                {
                    Playlist.Add(s);
                }

                if (vybrana == null)
                {
                    AktualniSkladba = null;
                    _audioService.Stop();
                    return;
                }
               
                Song? hledanaSkldaba = Playlist.FirstOrDefault(s => s.CestaKSouboru == vybrana.CestaKSouboru);
                if(hledanaSkldaba == null)
                {
                    return;
                }

                // Hledání indexu v novém seznamu
                aktualniIndex = Playlist.IndexOf(hledanaSkldaba);
                AktualniSkladba = Playlist[aktualniIndex];

                // Spuštění přehrávání skladby, pouze pokud je jiná, než stávající
                if (!uzHrajeStejnaSkladba)
                {
                    Play();
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, $"Chyba při nastavování playlistu ve třídě {nameof(PrehravacViewModel)}");
            }
        }

        /// <summary>
        /// Metoda slouží ke spuštění aktuální skladby
        /// </summary>
        private async void Play()
        {
            if (AktualniSkladba == null)
            {
                return;
            }

            try
            {
                IsPlaying = true;

                await _audioService.Play(AktualniSkladba.CestaKSouboru);

                _audioService.Hlasitost = Hlasitost / 100f;

                CelkovaDelka = _audioService.CelkovyCas.TotalSeconds;
                AktualniCas = _audioService.AktualniCas.TotalSeconds;

                _ = _historieService.Add(AktualniSkladba);
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, $"Chyba při spuštění skladby ve třídě {nameof(PrehravacViewModel)}");
            }
        }

        /// <summary>
        /// Metoda slouží k pozastavení aktuálně přehrávané skladby
        /// </summary>
        private void Pause()
        {
            IsPlaying = false;
            _audioService.Pause();
        }

        /// <summary>
        /// Metoda slouží k přepnutí se na další skladbu
        /// </summary>
        private void Next()
        {
            if (Playlist.Count == 0 || AktualniSkladba == null)
            {
                return;
            }

            aktualniIndex++;

            if (aktualniIndex >= Playlist.Count)
            {
                aktualniIndex = 0;
            }

            AktualniSkladba = Playlist[aktualniIndex];
            Play();
        }

        /// <summary>
        /// Metoda slouží k přepnutí se na předchozí skladbu
        /// </summary>
        private void Previous()
        {
            if (Playlist.Count == 0 || AktualniSkladba == null)
            {
                return;
            }

            aktualniIndex--;

            if (aktualniIndex < 0)
            {
                aktualniIndex = Playlist.Count - 1;
            }

            AktualniSkladba = Playlist[aktualniIndex];
            Play();
        }

        /// <summary>
        /// Metoda slouží k automatickému přehrání další skladby
        /// </summary>
        private void OnUkonceniSkladby()
        {
            App.Current.Dispatcher.BeginInvoke(() =>
            {
                if (Playlist.Count > 1)
                {
                    Next();
                }
            });
        }

        /// <summary>
        /// Metoda slouží k signalizaci, že uživatel začal posouvat Sliderem
        /// </summary>
        public void ZacatekPosunu()
        {
            uzivatelPosouvaSlider = true;
            _audioService.Pause();
        }

        /// <summary>
        /// Metoda slouží k signalizaci, že uživatel přestal posouvat Sliderem
        /// </summary>
        public void KonecPosunu()
        {
            uzivatelPosouvaSlider = false;
            _audioService.Resume();
        }

        /// <summary>
        /// Metoda slouží k odstranění vybrané skladby z fronty přehrávání
        /// </summary>
        /// <param name="skladba">Vybraná skladba, kterou chceme smazat</param>
        public void OdstranSkladbuZFronty(Song skladba)
        {
            if (skladba == null) return;

            // Najdeme skladbu ve frontě, která má stejnou cestu k souboru
            var songVeFronte = Playlist.FirstOrDefault(s => s.CestaKSouboru == skladba.CestaKSouboru);

            if (songVeFronte != null)
            {
                // Pokud je to zrovna ta, co hraje, stopneme ji
                if (AktualniSkladba != null && AktualniSkladba.CestaKSouboru == songVeFronte.CestaKSouboru)
                {
                    _audioService.Stop();
                    AktualniSkladba = null;
                    IsPlaying = false;
                }

                // Odstraníme nalezenou instanci z kolekce
                Playlist.Remove(songVeFronte);

                // Přepočítáme index aktuální skladby
                if (AktualniSkladba != null)
                {
                    aktualniIndex = Playlist.IndexOf(AktualniSkladba);
                }
                else
                {
                    aktualniIndex = -1;
                }
            }
        }

        /// <summary>
        /// Metoda slouží k vymazání aktuálně přehrávané fronty
        /// </summary>
        /// <param name="smazanyPlaylist">Playlist, který chceme smazat</param>
        public void VymazFrontuPrehravani(PlayList? smazanyPlaylist)
        {
            if (smazanyPlaylist == null)
            {
                return;
            }

            if (smazanyPlaylist.Nazev == ZdrojPrehravani)
            {
                _audioService.Stop();
                AktualniSkladba = null;
                IsPlaying = false;
                Playlist.Clear();
                ZdrojPrehravani = String.Empty;
                aktualniIndex = -1;
            }
        }

        /// <summary>
        /// Metoda slouží k refreshnutí změn metadat skladby ve View
        /// </summary>
        public void RefreshAktualniSkladbu()
        {
            OnPropertyChanged(nameof(AktualniSkladba));
        }
    }
}
