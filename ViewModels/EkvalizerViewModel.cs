using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using Hudebni_Prehravac_OctaBeats.Services.Audio;
using Hudebni_Prehravac_OctaBeats.Services.Dialog;
using Hudebni_Prehravac_OctaBeats.Services.Ekvalizer;
using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hudebni_Prehravac_OctaBeats.ViewModels
{
    /// <summary>
    /// ViewModel pro obsluhu metod ekvalizéru
    /// </summary>
    public class EkvalizerViewModel : BaseViewModel
    {
        private readonly ILokalizaceService _lokalizaceService;
        private readonly IDialogService _dialogService;
        private readonly IAudioService _audioService;
        private readonly INastaveniEkvalizeruService _nastaveniEkvalizeruService;

        /// <summary>
        /// Výchozí hodnota zesílení pásem
        /// </summary>
        private const float VychoziHodnotaZesileni = 0f;

        /// <summary>
        /// Jednotlivá pásma ekvalizéru
        /// </summary>
        public ObservableCollection<PasmoEkvalizeru>? PasmaEkvalizeru { get; private set; }

        /// <summary>
        /// Jednotlivé typy přednastavení (presetů)
        /// </summary>
        private ObservableCollection<KeyValuePair<TypPrednastaveni, string>> typyPrednastaveni;
        public ObservableCollection<KeyValuePair<TypPrednastaveni, string>> TypyPrednastaveni
        {
            get => typyPrednastaveni;
            set
            {
                typyPrednastaveni = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Aktuálně vybraný typ přednastavení
        /// </summary>
        private TypPrednastaveni vybranyTypPrednastaveni;
        public TypPrednastaveni VybranyTypPrednastaveni
        {
            get => vybranyTypPrednastaveni;
            set
            {
                vybranyTypPrednastaveni = value;
                NastavPrednastaveni(value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Určuje, zda je ekvalizér zapnutý
        /// </summary>
        private bool jeEkvalizerZapnuty;
        public bool JeEkvalizerZapnuty
        {
            get => jeEkvalizerZapnuty;
            set
            {
                jeEkvalizerZapnuty = value;
                OnPropertyChanged();

                _audioService.JeEkvalizerZapnuty = value;
            }
        }

        // Delegování indexeru na službu, která je už implementována v ILokalizaceService
        public string this[string key] => _lokalizaceService[key];

        /* Příkazy pro obsluhu jednotlivých metod */
        public ICommand ResetCommand { get; }
        public ICommand UlozitCommand { get; }

        /// <summary>
        /// Událost pro zavření dialogu
        /// </summary>
        public event Action<bool>? ZavritDialog;

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="lokalizaceService">Servis pro obsluhu metod lokalizace</param>
        /// <param name="dialogService">Servis pro zobrazení příslušných dialogů</param>
        /// <param name="audioService">Servis pro obsluhu metod audia</param>
        /// <param name="ekvalizerService">Servis pro obsluhu metod ekvalizéru</param>
        public EkvalizerViewModel(ILokalizaceService lokalizaceService, IDialogService dialogService, IAudioService audioService, 
            INastaveniEkvalizeruService ekvalizerService)
        {
            _lokalizaceService = lokalizaceService;
            _dialogService = dialogService;
            _audioService = audioService;
            _nastaveniEkvalizeruService = ekvalizerService;

            // Asynchronní inicializace nastavení ekvalizéru
            _ = InicializujAsync();

            if (PasmaEkvalizeru == null || PasmaEkvalizeru.Count == 0)
            {
                PasmaEkvalizeru = new ObservableCollection<PasmoEkvalizeru>(Pasma.PasmaEkvalizeru);
            }

            ResetCommand = new RelayCommand(_ => Reset());
            UlozitCommand = new RelayCommand(_ =>
            {
                ZavritDialog?.Invoke(true);
            });
        }

        /// <summary>
        /// Metoda slouží k asynchronnímu načtení nastavení ekvalizéru
        /// </summary>
        /// <returns>Vrací Task</returns>
        public async Task InicializujAsync()
        {
            try
            {
                NastaveniEkvalizer? nastaveniEkvalizer = await _nastaveniEkvalizeruService.Load()!;

                if (nastaveniEkvalizer != null && nastaveniEkvalizer.PasmaEkvalizeru != null && nastaveniEkvalizer.PasmaEkvalizeru.Count > 0)
                {
                    PasmaEkvalizeru?.Clear();
                    foreach (PasmoEkvalizeru pasmo in nastaveniEkvalizer.PasmaEkvalizeru)
                    {
                        PasmaEkvalizeru?.Add(pasmo);
                    }

                    jeEkvalizerZapnuty = nastaveniEkvalizer.JeEkvalizerPovoleny;
                    vybranyTypPrednastaveni = nastaveniEkvalizer.TypPrednastaveni;
                }

                OnPropertyChanged(nameof(PasmaEkvalizeru));
                OnPropertyChanged(nameof(JeEkvalizerZapnuty));
                OnPropertyChanged(nameof(VybranyTypPrednastaveni));

                _audioService.AktualniPasma = PasmaEkvalizeru;
                _audioService.JeEkvalizerZapnuty = JeEkvalizerZapnuty;
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while initializing the equalizer!", nameof(InicializujAsync));
            }
        }

        /// <summary>
        /// Metoda slouží k resetování všech pásem ekvalizéru na výchozí hodnotu
        /// </summary>
        private void Reset()
        {
            if (PasmaEkvalizeru == null || PasmaEkvalizeru.Count == 0)
            {
                return;
            }

            try
            {
                foreach (PasmoEkvalizeru pasmo in PasmaEkvalizeru)
                {
                    pasmo.Zesileni = VychoziHodnotaZesileni;
                }

                PasmaEkvalizeru = new ObservableCollection<PasmoEkvalizeru>(PasmaEkvalizeru);
                VybranyTypPrednastaveni = TypPrednastaveni.Custom;

                AktualizujEkvalizer();
                OnPropertyChanged(nameof(PasmaEkvalizeru));
                OnPropertyChanged(nameof(VybranyTypPrednastaveni));
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while reseting equalizer bands!", nameof(Reset));
                _dialogService.ShowError(ex.Message);
            }
        }
      
        /// <summary>
        /// Metoda slouží k aktualizaci pásem ekvalizéru v AudioService
        /// </summary>
        public void AktualizujEkvalizer()
        {
            if(PasmaEkvalizeru == null)
            {
                return;
            }

            try
            {
                _audioService.UpdateEqualizer(PasmaEkvalizeru);
            }

            catch(Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while updating the equalizer!", nameof(AktualizujEkvalizer));
                _dialogService.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Metoda slouží k refreshnutí prvků ve View, aby se správně přeložily
        /// </summary>
        public void RefreshLokalizace()
        {
            // Uložení aktuálně vybraného typu
            TypPrednastaveni puvodniTypPrednastaveni = VybranyTypPrednastaveni;

            // Vygenerování nových kolekcí s korektními překlady
            TypyPrednastaveni = new ObservableCollection<KeyValuePair<TypPrednastaveni, string>>
            {
                new KeyValuePair<TypPrednastaveni, string>(TypPrednastaveni.Custom, _lokalizaceService["PresetCustom"]),
                new KeyValuePair<TypPrednastaveni, string>(TypPrednastaveni.Rock, _lokalizaceService["PresetRock"]),
                new KeyValuePair<TypPrednastaveni, string>(TypPrednastaveni.Pop, _lokalizaceService["PresetPop"]),
                new KeyValuePair<TypPrednastaveni, string>(TypPrednastaveni.Reggae, _lokalizaceService["PresetReggae"]),
                new KeyValuePair<TypPrednastaveni, string>(TypPrednastaveni.Techno, _lokalizaceService["PresetTechno"]),
                new KeyValuePair<TypPrednastaveni, string>(TypPrednastaveni.Ska, _lokalizaceService["PresetSka"]),
                new KeyValuePair<TypPrednastaveni, string>(TypPrednastaveni.Classical, _lokalizaceService["PresetClassical"]),
                new KeyValuePair<TypPrednastaveni, string>(TypPrednastaveni.Club, _lokalizaceService["PresetClub"]),
                new KeyValuePair<TypPrednastaveni, string>(TypPrednastaveni.Dance, _lokalizaceService["PresetDance"]),
                new KeyValuePair<TypPrednastaveni, string>(TypPrednastaveni.Headphones, _lokalizaceService["PresetHeadphones"]),
                new KeyValuePair<TypPrednastaveni, string>(TypPrednastaveni.Vocal, _lokalizaceService["PresetVocal"]),
            };

            VybranyTypPrednastaveni = puvodniTypPrednastaveni;

            // Oznámení indexeru, aby změnil překlad
            OnPropertyChanged("Item[]");
        }

        /// <summary>
        /// Metoda slouží k nastavení jednotlivých hodnot pásem podle daného typ přednastavení
        /// </summary>
        /// <param name="typPrednastaveni">Konkrétní typ přednastavení</param>
        private void NastavPrednastaveni(TypPrednastaveni typPrednastaveni)
        {
            if (PasmaEkvalizeru == null || PasmaEkvalizeru.Count == 0 || typPrednastaveni == TypPrednastaveni.Custom)
            {
                return;
            }

            try
            {
                if (Presety.PresetyEkvalizeru.TryGetValue(typPrednastaveni, out float[]? hodnoty))
                {
                    if (hodnoty == null)
                    {
                        return;
                    }

                    ObservableCollection<PasmoEkvalizeru> novaPasma = new ObservableCollection<PasmoEkvalizeru>();

                    // Nastavení pásem podle konkrétního typu přednastavení
                    for (int i = 0; i < PasmaEkvalizeru.Count && i < hodnoty.Length; i++)
                    {
                        PasmoEkvalizeru puvodni = PasmaEkvalizeru[i];
                        novaPasma.Add(new PasmoEkvalizeru(puvodni.Nazev, puvodni.Frekvence, hodnoty[i], puvodni.SirkaPasma));
                    }

                    PasmaEkvalizeru = novaPasma;

                    OnPropertyChanged(nameof(PasmaEkvalizeru));

                    AktualizujEkvalizer();
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while setting up the chosen preset!", nameof(NastavPrednastaveni));
                _dialogService.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Metoda slouží k nastavení aktuálního typu přednastavení na Vlastní
        /// </summary>
        public void NastavVlastniPrednastaveni()
        {
            if(PasmaEkvalizeru == null || PasmaEkvalizeru.Count == 0)
            {
                return;
            }

            if(VybranyTypPrednastaveni != TypPrednastaveni.Custom)
            {
                VybranyTypPrednastaveni = TypPrednastaveni.Custom;
            }
        }
    }
}
