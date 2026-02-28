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
    /// View, které je vázané na HistoryViewModel
    /// </summary>
    public partial class HistoryView : UserControl
    {
        public HistoryView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Metoda slouží k zrušení vybraného prvku v ListBoxu (zrušení focusu)
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">eventArgs</param>
        private void ListboxHistory_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            HitTestResult result = VisualTreeHelper.HitTest(listboxHistory, e.GetPosition(listboxHistory));
            if (result != null)
            {
                DependencyObject obj = result.VisualHit;
                while (obj != null && obj != listboxHistory)
                {
                    if (obj is ListBoxItem) return;
                    obj = VisualTreeHelper.GetParent(obj);
                }

                listboxHistory.UnselectAll();
                listboxHistory.Focusable = false;
                Keyboard.ClearFocus();
                listboxHistory.Focusable = true;
            }
        }
    }
}
