using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.ViewModels
{
    /// <summary>
    /// ViewModel pro obsluhu metod nastavení
    /// </summary>
    public class NastaveniViewModel : BaseViewModel
    {
        private readonly ILokalizaceService _lokalizaceService;

        /// <summary>
        /// Seznam dostupných jazykových verzí aplikace
        /// </summary>
        public ObservableCollection<CultureInfo> DostupneJazyky { get; }

        private CultureInfo vybranyJazyk;
        public CultureInfo VybranyJazyk
        {
            get => vybranyJazyk;
            set
            {
                vybranyJazyk = value;
                _lokalizaceService.ChangeLanguage(value.Name);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="lokalizaceService">Servis pro obsluhu metod nastavení</param>
        public NastaveniViewModel(ILokalizaceService lokalizaceService)
        {
            _lokalizaceService = lokalizaceService;

            // Nastavení dostupných jazykových verzí
            DostupneJazyky = new ObservableCollection<CultureInfo>
            {
                new CultureInfo("cs-CZ"),
                new CultureInfo("en-US")
            };

            VybranyJazyk = lokalizaceService.AktualniJazyk;
        }
    }
}
