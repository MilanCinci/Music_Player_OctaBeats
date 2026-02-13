using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Models
{
    /// <summary>
    /// Třída sloužící k definování skladeb pro historii přehrávání
    /// </summary>
    public class HistoriePrehravani
    {
        /// <summary>
        /// Skladba v historii přehrávání
        /// </summary>
        public Song Song { get; set; }

        /// <summary>
        /// Datum a čas posledního přehrání
        /// </summary>
        public DateTime DatumPrehrani { get; set; }

        /// <summary>
        /// Bezparametrický konstruktor pro inicializaci
        /// </summary>
        public HistoriePrehravani() { }

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="song">Skladba v historii přehrávání</param>
        /// <param name="datumPrehrani">Datum a čas posledního přehrání</param>
        public HistoriePrehravani(Song song, DateTime datumPrehrani)
        {
            Song = song;
            DatumPrehrani = datumPrehrani;
        }
    }
}
