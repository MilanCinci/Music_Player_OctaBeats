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
using System.Windows.Shapes;

namespace Hudebni_Prehravac_OctaBeats.Views
{
    /// <summary>
    /// View, které je vázané na MainViewModel
    /// </summary>
    public partial class HlavniOkno : Window
    {
        public HlavniOkno()
        {
            /* TODO 
                    Dodělat Nastavení, Historie Přehrávání */
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }
    }
}
