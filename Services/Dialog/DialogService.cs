using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hudebni_Prehravac_OctaBeats.Services.Dialog
{
    /// <summary>
    /// Třída sloužící k implementování rozhraní IDialogService a obsluze daných metod
    /// </summary>
    public class DialogService : IDialogService
    {
        /// <summary>
        /// Metoda slouží k zobrazení Error dialogu
        /// </summary>
        /// <param name="message">Zpráva, která se má zobrazit uvnitř dialogu</param>
        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Metoda slouží k zobrazení Information dialogu
        /// </summary>
        /// <param name="message">Zpráva, která se má zobrazit uvnitř dialogu</param>
        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Metoda slouží k zobrazení Confirmation dialogu
        /// </summary>
        /// <param name="message">Zpráva, která se má zobrazit uvnitř dialogu</param>
        /// <returns>Vrací výsledek dialogu (MessageBoxResult)</returns>
        public MessageBoxResult ShowConfirmation(string message)
        {
            return MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        /// <summary>
        /// Metoda slouží k zobrazení Warning dialogu
        /// </summary>
        /// <param name="message">Zpráva, která se má zobrazit uvnitř dialogu</param>
        public void ShowWarning(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
