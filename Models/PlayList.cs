using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Models
{
    /// <summary>
    /// Třída sloužící k definování informací ohledně playlistu
    /// </summary>
    public class PlayList
    {
        /// <summary>
        /// Název playlistu
        /// </summary>
        public required string Nazev { get; set; }

        /// <summary>
        /// Seznam skladeb v playlistu
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<Song> Skladby { get; set; } = new ObservableCollection<Song>();

        /// <summary>
        /// Seznam cest ke skladbám
        /// </summary>
        public ObservableCollection<string> CestyKSkladbam { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Bezparametrický konstruktor pro inicializaci
        /// </summary>
        public PlayList() { }

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="nazev">Název playlistu</param>
        /// <param name="skladby">Seznam skladeb v playlistu</param>
        public PlayList(string nazev, ObservableCollection<Song> skladby)
        {
            Nazev = nazev;
            Skladby = skladby;
        }
    }
}
