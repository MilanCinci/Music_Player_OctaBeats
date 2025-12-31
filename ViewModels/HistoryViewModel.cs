using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Services.Historie;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
            Historie = _historieService.Load();
        }
    }
}
