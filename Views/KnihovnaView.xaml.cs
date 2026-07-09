using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hudebni_Prehravac_OctaBeats.Views
{
    /// <summary>
    /// View, které je vázané na KnihovnaViewModel
    /// </summary>
    public partial class KnihovnaView : UserControl
    {
        public KnihovnaView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Metoda slouží k zamezení vybrání položky, když se kliká jinam
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">eventArgs</param>
        private void ListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.OriginalSource is ScrollViewer || e.OriginalSource is Grid))
            {
                if(e.LeftButton == MouseButtonState.Pressed)
                {
                    e.Handled = true;
                    return;
                }
            }

            // Zamezení vybraní skladby při stisknutí pravého tlačítka myši
            if (e.ClickCount < 2)
            {
                DependencyObject dep = (DependencyObject)e.OriginalSource;
                while (dep != null && !(dep is ListBoxItem))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                if (dep is ListBoxItem && DataContext is KnihovnaViewModel vm)
                {
                    if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Metoda slouží k zobrazení ozačeného prvku (automatické scrollování v seznamu) 
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">eventArgs</param>
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox lb && lb.SelectedItem != null)
            {
                // Zajištění, že označený prvek bude vždy viditelný na obrazovce
                lb.ScrollIntoView(lb.SelectedItem);         
            }
        }

        /// <summary>
        /// Metoda slouží k označení skladby, kterou chceme přehrát
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">eventArgs</param>
        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                DependencyObject dep = (DependencyObject)e.OriginalSource;
                while (dep != null && !(dep is ListBoxItem))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                if (dep is ListBoxItem item)
                {
                    Song? skladba = item.Content as Song;

                    if (skladba != null && DataContext is KnihovnaViewModel vm)
                    {
                        listBox.SelectedItem = skladba;
                        vm.VybranaSkladba = skladba;
                    }
                }
            }
        }

        /// <summary>
        /// Metoda slouží k zobrazení Popup menu pro řazení skladeb v knihovně
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">eventArgs</param>
        private void BtnSerad_Click(object sender, RoutedEventArgs e)
        {
            SortPopup.IsOpen = true;
        }
    }
}
