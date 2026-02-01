using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hudebni_Prehravac_OctaBeats.Models;

namespace Hudebni_Prehravac_OctaBeats.Services.Playlist
{
    /// <summary>
    /// Rozhraní sloužící k definování metod pro obsluhu playlistu
    /// </summary>
    public interface IPlaylistService
    {
        /// <summary>
        /// Metoda slouží k načtení uložených playlistů
        /// </summary>
        /// <returns>Vrací kolekci načtených playlistů</returns>
        Task<ObservableCollection<PlayList>>? Load();

        /// <summary>
        /// Metoda slouží k uložení playlistů
        /// </summary>
        /// <param name="playlisty">Seznam playlistů, který chceme uložit</param>
        Task Save(ObservableCollection<PlayList> playlisty);
    }
}
