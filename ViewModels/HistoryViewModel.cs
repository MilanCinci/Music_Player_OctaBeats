using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Services.Historie;
using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Hudebni_Prehravac_OctaBeats.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        private readonly IHistorieService _historieService;
        private readonly ILokalizaceService _lokalizaceService;

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

        // Delegování indexeru na službu, která je už implementována v ILokalizaceService
        public string this[string key] => _lokalizaceService[key];

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="historieService">Servis pro obsluhu metod historie</param>
        public HistoryViewModel(IHistorieService historieService, ILokalizaceService lokalizaceService)
        {
            _historieService = historieService;
            _lokalizaceService = lokalizaceService;
            _ = InicializujAsync();

            RemoveSelectedHistoryCommand = new AsyncRelayCommand(OdstranVybranyPrvekHistorie, () => VybranyZaznam != null);
            RemoveAllHistoryCommand = new AsyncRelayCommand(OdstranCelouHistorii);
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
                await _historieService.Load();
                OnPropertyChanged(nameof(Historie));
            }
        }

        /// <summary>
        /// Metoda slouží k odstranění vybraného prvku z historie přehrávání
        /// </summary>
        /// <returns>Vrací Task</returns>
        private async Task OdstranVybranyPrvekHistorie()
        {
            DialogResult vysledekDiaOkna = MessageBox.Show("Opravdu chcete smazat tento záznam z historie přehrávání?", "Confirm",
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (VybranyZaznam != null && vysledekDiaOkna == DialogResult.Yes)
            {
                await _historieService.Delete(VybranyZaznam);
                VybranyZaznam = null;
            }
        }

        /// <summary>
        /// Metoda slouží k odstranění celé historie přehrávání
        /// </summary>
        /// <returns>Vrací Task</returns>
        private async Task OdstranCelouHistorii()
        {
            DialogResult vysledekDiaOkna = MessageBox.Show("Opravdu chcete smazat celou historii přehrávání?", "Confirm",
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (vysledekDiaOkna == DialogResult.Yes)
            {
                await _historieService.ClearAll();
                VybranyZaznam = null;
                OnPropertyChanged(nameof(Historie));
            }
        }
    }
}