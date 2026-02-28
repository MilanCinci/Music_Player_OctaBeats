using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using Hudebni_Prehravac_OctaBeats.Services.Dialog;
using Hudebni_Prehravac_OctaBeats.Services.Historie;
using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Hudebni_Prehravac_OctaBeats.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        private readonly IHistorieService _historieService;
        private readonly ILokalizaceService _lokalizaceService;
        private readonly IDialogService _dialogService;

        /// <summary>
        /// Přímé odkazování na seznam historie v HistorieService
        /// </summary>
        public ObservableCollection<HistoriePrehravani> Historie => ((HistoryService)_historieService).MojeHistorie;

        private HistoriePrehravani? vybranyZaznam;
        public HistoriePrehravani? VybranyZaznam
        {
            get => vybranyZaznam;
            set 
            { 
                    vybranyZaznam = value; 
                    OnPropertyChanged();
                    RemoveSelectedHistoryCommand.RaiseCanExecuteChanged();
            }
        }

        /* Příkazy pro obsluhu jednotlivých metod */
        public AsyncRelayCommand RemoveSelectedHistoryCommand { get; }
        public ICommand RemoveAllHistoryCommand { get; }
        public ICommand ResetHistorySelectionCommand { get; }

        // Delegování indexeru na službu, která je už implementována v ILokalizaceService
        public string this[string key] => _lokalizaceService[key];

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="historieService">Servis pro obsluhu metod historie</param>
        /// <param name="lokalizaceService">Servis pro obsluhu metod lokalizace aplikace</param>
        /// <param name="dialogService">Servis pro zobrazení příslušných dialogů</param>
        public HistoryViewModel(IHistorieService historieService, ILokalizaceService lokalizaceService, IDialogService dialogService)
        {
            _historieService = historieService;
            _lokalizaceService = lokalizaceService;
            _dialogService = dialogService;

            // Asynchronní inicializace historie přehrávání
            _ = InicializujAsync();

            RemoveSelectedHistoryCommand = new AsyncRelayCommand(OdstranVybranyPrvekHistorie, () => VybranyZaznam != null);
            RemoveAllHistoryCommand = new AsyncRelayCommand(OdstranCelouHistorii);
            ResetHistorySelectionCommand = new RelayCommand(_ => VybranyZaznam = null);
        }

        /// <summary>
        /// Metoda slouží k refreshnutí prvků ve View, aby se správně přeložily
        /// </summary>
        public void RefreshLokalizace()
        {
            OnPropertyChanged("Item[]");
        }

        /// <summary>
        /// Metoda slouží k asynchronnímu načtení uložené historie přehrávání
        /// </summary>
        /// <returns>Vrací Task</returns>
        private async Task InicializujAsync()
        {
            if(_historieService != null)
            {
                try
                {
                    await _historieService.Load();
                    OnPropertyChanged(nameof(Historie));
                }

                catch (Exception ex)
                {
                    SpravaSouboru.LogError(ex, "Error occurred while initializing the playback history!", nameof(InicializujAsync));
                    _dialogService.ShowError(ex.Message);
                }
            }
        }

        /// <summary>
        /// Metoda slouží k odstranění vybraného prvku z historie přehrávání
        /// </summary>
        /// <returns>Vrací Task</returns>
        private async Task OdstranVybranyPrvekHistorie()
        {
            try
            {
                MessageBoxResult vysledekDiaOkna = _dialogService.ShowConfirmation(_lokalizaceService["QuestionDeleteItemFromHistory"]);
                if (VybranyZaznam != null && vysledekDiaOkna == MessageBoxResult.Yes)
                {
                    await _historieService.Delete(VybranyZaznam);
                    VybranyZaznam = null;
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "", nameof(OdstranVybranyPrvekHistorie));
                _dialogService.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Metoda slouží k odstranění celé historie přehrávání
        /// </summary>
        /// <returns>Vrací Task</returns>
        private async Task OdstranCelouHistorii()
        {
            try
            {
                MessageBoxResult vysledekDiaOkna = _dialogService.ShowConfirmation(_lokalizaceService["QuestionDeleteHistory"]);
                if (vysledekDiaOkna == MessageBoxResult.Yes)
                {
                    await _historieService.ClearAll();
                    VybranyZaznam = null;
                    OnPropertyChanged(nameof(Historie));
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "", nameof(OdstranCelouHistorii));
                _dialogService.ShowError(ex.Message);
            }
        }
    }
}