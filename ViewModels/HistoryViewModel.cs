using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Services.Historie;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.ViewModels
{
    /// <summary>
    /// ViewModel pro obsluhu metod historie přehrávání
    /// </summary>
    public class HistoryViewModel : BaseViewModel
    {
        private readonly IHistorieService _historieService;

        /// <summary>
        /// Seznam skladeb v historii přehrávání
        /// </summary>
        public ObservableCollection<HistoriePrehravani>? Historie { get; set; }

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="historieService">Servis pro obsluhu metod historie přehrávání</param>
        public HistoryViewModel(IHistorieService historieService)
        {
            _historieService = historieService;
            _ = InicializujAsync();
        }

        /// <summary>
        /// Pomocná metoda pro asynchronní načtení historie přehrávání
        /// </summary>
        private async Task InicializujAsync()
        {
            try
            {
                Historie = await _historieService.Load()!;
                OnPropertyChanged(nameof(Historie));
            }

            catch (Exception)
            {
                //TODO
            }
        }
    }
}
