using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Models
{
    /// <summary>
    /// Třída sloužící k uchování importovaných skladeb v knihovně
    /// </summary>
    public class KnihovnaSkladeb
    {
        /// <summary>
        /// Seznam skladeb v knihovně
        /// </summary>
        public ObservableCollection<Song> Skladby { get; set; } = new ObservableCollection<Song>();

        /// <summary>
        /// Bezparametrický konstruktor pro inicializaci
        /// </summary>
        public KnihovnaSkladeb() { }

        /// <summary>
        /// Parametrický konstruktor
        /// </summary>
        /// <param name="skladby">Seznam skladeb v knihovně</param>
        public KnihovnaSkladeb(ObservableCollection<Song> skladby)
        {
            Skladby = skladby;
        }
    }
}
