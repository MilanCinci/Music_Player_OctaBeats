using Hudebni_Prehravac_OctaBeats.Services.Playlist;
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
    /// View, které je vázané na PlaylistViewModel
    /// </summary>
    public partial class PlaylistView : UserControl
    {
        public PlaylistView()
        {
            InitializeComponent();

            IPlaylistService playlistService = new PlaylistService();
        }

        /// <summary>
        /// Metoda slouží k zobrazení editovacího dialogu po kliknutí na položku
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">eventArgs</param>
        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PlaylistViewModel vm && vm.VybranyPlaylist != null)
            {
                ((MainViewModel)Application.Current.MainWindow.DataContext)
                    .UpravitPlaylist(vm.VybranyPlaylist);
                lboxPlaylist.Items.Refresh();
            }
        }
    }
}
