using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Services.Dialog;
using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hudebni_Prehravac_OctaBeats.ViewModels
{
    /// <summary>
    /// ViewModel pro obsluhu metod nastavení jazyka aplikace
    /// </summary>
    public class NastaveniJazykViewModel : BaseViewModel
    {
        private readonly ILokalizaceService _lokalizaceService;
        private readonly IDialogService _dialogService;

        /// <summary>
        /// Seznam dostupných jazykových verzí
        /// </summary>
        public ObservableCollection<Language> DostupneJazyky { get;}

        private Language? vybranyJazyk;
        public Language? VybranyJazyk
        {
            get => vybranyJazyk;
            set 
            { 
                vybranyJazyk = value; 
                OnPropertyChanged(); 
            }
        }

        /* Příkazy pro obsluhu jednotlivých metod */
        public ICommand PotvrditCommand { get; }

        // Delegování indexeru na službu, která je už implementována v ILokalizaceService
        public string this[string key] => _lokalizaceService[key];

        /// <summary>
        /// Událost pro zavření dialogu
        /// </summary>
        public event Action<bool>? ZavritDialog;

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="lokalizaceService">Servis pro obsluhu metod nastavení jazykových verzí aplikace</param>
        /// <param name="dialogService">Servis pro zobrazení příslušných dialogů</param>
        public NastaveniJazykViewModel(ILokalizaceService lokalizaceService, IDialogService dialogService)
        {
            _lokalizaceService = lokalizaceService;
            _dialogService = dialogService;
           
            DostupneJazyky = new ObservableCollection<Language>
            {
                new Language { Nazev = "Čeština", Kod = "cs-CZ" },
                new Language { Nazev = "English", Kod = "en-US" }
            };

            // Nalezení aktuálně nastaveného jazyka aplikace v seznamu dostupných jazykových verzí
            VybranyJazyk = DostupneJazyky.FirstOrDefault(jazyk => jazyk.Kod.Equals(Properties.Settings.Default.Language, StringComparison.OrdinalIgnoreCase));

            PotvrditCommand = new RelayCommand(_ =>
            {              
                ZavritDialog?.Invoke(true);
            });     
        }

        /// <summary>
        /// Metoda slouží ke změně jazykové verze aplikace
        /// </summary>
        /// <param name="cultureCode">Kód jazyka, na který chceme přeložit</param>
        public void ZmenJazyk(string cultureCode)
        {
            try
            {
                _lokalizaceService.ChangeLanguage(cultureCode);

                // Nastavení a následné uložení aktuálně zvoleného jazyka uživatelem
                Properties.Settings.Default.Language = cultureCode;
                Properties.Settings.Default.Save();

                // Vyvolání aktualizace GUI prvků v XAML pomocí Indexeru[]
                OnPropertyChanged("Item[]");
            }

            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }
        }
    }
}
