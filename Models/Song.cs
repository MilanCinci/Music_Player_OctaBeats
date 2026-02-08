using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Models
{
    /// <summary>
    /// Třída sloužící k definování metadat skladby
    /// </summary>
    public class Song
    {
        /// <summary>
        /// Název skladby
        /// </summary>
        public required string Nazev { get; set; }

        /// <summary>
        /// Interpret (autor) skladby
        /// </summary>
        public string? Interpret { get; set; }

        /// <summary>
        /// Název alba
        /// </summary>
        public string? Album { get; set; }

        /// <summary>
        /// Doba trvání skladby
        /// </summary>
        public TimeSpan Delka { get; set; }

        /// <summary>
        /// Přebal alba
        /// </summary>
        public byte[]? PrebalAlba { get; set; }

        /// <summary>
        /// Hlavní žánr skladby
        /// </summary>
        public string? Zanr { get; set; }

        /// <summary>
        /// Rok vydání skladby
        /// </summary>
        public uint? RokVydani {  get; set; }
        
        /// <summary>
        /// Cesta k souboru skladby
        /// </summary>
        public required string CestaKSouboru { get; set; }

        /// <summary>
        /// Bezparametrický konstruktor pro inicializaci
        /// </summary>
        public Song() { }

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="nazev">Název skladby</param>
        /// <param name="interpret">Interpret (autor) skladby</param>
        /// <param name="album">Název alba</param>
        /// <param name="delka">Doba trvání skladby</param>
        /// <param name="prebalAlba">Přebal alba</param>
        /// <param name="zanr">Hlavní žánr skladby</param>
        /// <param name="rokVydani">Rok vydání skladby</param>
        /// <param name="cestaKSouboru">Cesta k souboru skladby</param>
        public Song(string nazev, string? interpret, string? album, TimeSpan delka, byte[]? prebalAlba, string? zanr, uint? rokVydani, string cestaKSouboru)
        {
            Nazev = nazev;
            Interpret = interpret;
            Album = album;
            Delka = delka;
            PrebalAlba = prebalAlba;
            Zanr = zanr;
            RokVydani = rokVydani;
            CestaKSouboru = cestaKSouboru;
        }
    }
}
