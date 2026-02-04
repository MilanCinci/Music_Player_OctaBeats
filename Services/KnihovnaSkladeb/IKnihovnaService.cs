using Hudebni_Prehravac_OctaBeats.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Services.KnihovnaSkladeb
{
    /// <summary>
    /// Rozhraní sloužící k definování metod pro obsluhu knihovny skladeb
    /// </summary>
    public interface IKnihovnaService
    {
        /// <summary>
        /// Metoda slouží k načtení uložených skladeb
        /// </summary>
        /// <returns>Vrací kolekci načtených skladeb</returns>
        Task<ObservableCollection<Song>>? Load();

        /// <summary>
        /// Metoda slouží k překopírování souborů do složky s hudbou MyMusic
        /// </summary>
        /// <param name="vybraneSoubory">Uživatelem vybrané soubory dialogu</param>
        /// <returns>Vrací Task</returns>
        Task CopySongsToMyMusic(string[] vybraneSoubory);

        /// <summary>
        /// Metoda slouží k vymazání souboru ze složky s hudbou MyMusic
        /// </summary>
        /// <param name="cestaVybraneSkladby">Uživatelem vybraná skladba ke smazání</param>
        /// <returns>Vrací true, pokud cesta ke skladbě je validní, jinak false</returns>
        bool DeleteSongFromMyMusic(string cestaVybraneSkladby);
    }
}
