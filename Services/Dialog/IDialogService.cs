using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Hudebni_Prehravac_OctaBeats.Services.Dialog
{
    /// <summary>
    /// Rozhraní sloužící k definování metod pro vytvoření jednoduchých dialogových oken
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Metoda slouží k zobrazení Error dialogu
        /// </summary>
        /// <param name="message">Zpráva, která se má zobrazit uvnitř dialogu</param>
        void ShowError(string message);

        /// <summary>
        /// Metoda slouží k zobrazení Information dialogu
        /// </summary>
        /// <param name="message">Zpráva, která se má zobrazit uvnitř dialogu</param>
        void ShowInfo(string message);

        /// <summary>
        /// Metoda slouží k zobrazení Confirmation dialogu
        /// </summary>
        /// <param name="message">Zpráva, která se má zobrazit uvnitř dialogu</param>
        /// <returns>Vrací výsledek dialogu (MessageBoxResult)</returns>
        MessageBoxResult ShowConfirmation(string message);

        /// <summary>
        /// Metoda slouží k zobrazení Warning dialogu
        /// </summary>
        /// <param name="message">Zpráva, která se má zobrazit uvnitř dialogu</param>
        void ShowWarning(string message);
    }
}
