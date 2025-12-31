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
        ObservableCollection<Song>? Load();
    }
}
