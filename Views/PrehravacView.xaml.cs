using Hudebni_Prehravac_OctaBeats.Services.Audio;
using Hudebni_Prehravac_OctaBeats.Services.Historie;
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
    /// View, které je vázané na PrehravacViewModel
    /// </summary>
    public partial class PrehravacView : UserControl
    {
        public PrehravacView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Metoda slouží k signalizaci, že se hýbe sliderem
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">eventArgs</param>
        private void Slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PrehravacViewModel vm)
            {
                vm.ZacatekPosunu();
            }
        }

        /// <summary>
        /// Metoda slouží k signalizaci, že se hýbe sliderem
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">eventArgs</param>
        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PrehravacViewModel vm && sender is Slider slider)
            {
                vm.Seek(slider.Value);
                vm.KonecPosunu();
            }
        }
    }
}
