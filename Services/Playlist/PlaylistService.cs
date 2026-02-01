using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Services.Playlist
{
    /// <summary>
    /// Třída sloužící k implementování rozhraní IPlaylistService a obsluze daných metod
    /// </summary>
    public class PlaylistService : IPlaylistService
    {
        /// <summary>
        /// Cesta k JSON souboru s playlisty
        /// </summary>
        private static string CestaKSouboru = Environment.ExpandEnvironmentVariables(@"%AppData%\OctaBeats\DataFiles\playlisty.json");

        /// <summary>
        /// Metoda slouží k načtení uložených playlistů
        /// </summary>
        /// <returns>Vrací kolekci načtených playlistů</returns>
        public async Task<ObservableCollection<PlayList>>? Load()
        {
            return await SpravaSouboru.NahrajZeSouboru<ObservableCollection<PlayList>>(CestaKSouboru);
        }

        /// <summary>
        /// Metoda slouží k uložení playlistů
        /// </summary>
        /// <param name="playlisty">Seznam playlistů, který chceme uložit</param>
        public async Task Save(ObservableCollection<PlayList> playlisty)
        {
            var adresar = Path.GetDirectoryName(CestaKSouboru);

            if (!Directory.Exists(adresar))
            {
                Directory.CreateDirectory(adresar!);
            }

            await SpravaSouboru.UlozDoSouboru(CestaKSouboru, playlisty);
        }
    }
}
