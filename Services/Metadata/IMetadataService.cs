using Hudebni_Prehravac_OctaBeats.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Services.Metadata
{
    /// <summary>
    /// Rozhraní sloužící k definování metod pro obsluhu metadat skladeb
    /// </summary>
    public interface IMetadataService
    {
        /// <summary>
        /// Metoda slouží k načtení uložených metadat o skladbě
        /// </summary>
        /// <returns>Vrací metadata skladby</returns>
        Task<Song> Load(string cestaKSouboru);

        /// <summary>
        /// Metoda slouží k uložení metadat skladby
        /// </summary>
        /// <param name="song">skladba, u které chceme uložit metadata</param>
        Task Save(Song song);
    }
}
