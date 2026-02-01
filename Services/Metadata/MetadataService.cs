using Hudebni_Prehravac_OctaBeats.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using TagLib;

namespace Hudebni_Prehravac_OctaBeats.Services.Metadata
{
    /// <summary>
    /// Třída sloužící k implementování rozhraní IMetadataService a obsluze daných metod
    /// </summary>
    public class MetadataService : IMetadataService
    {
        /// <summary>
        /// Metoda slouží k načtení uložených metadat o skladbě
        /// </summary>
        /// <param name="cestaKSouboru">Cesta k souboru se skladbami</param>
        /// <returns>Vrací metadata skladby</returns>
        public async Task<Song> Load(string cestaKSouboru)
        {
            return await Task.Run(() =>
            {
                var soubor = TagLib.File.Create(cestaKSouboru);

                byte[]? prebalAlba = null;

                // Získání přebalu alba, pokud je nějaký obrázek přebalu k dispozici v metadatech
                if (soubor.Tag.Pictures != null && soubor.Tag.Pictures.Length > 0)
                {
                    prebalAlba = soubor.Tag.Pictures[0].Data.Data;
                }

                return new Song
                {
                    Nazev = soubor.Tag.Title ?? Path.GetFileNameWithoutExtension(cestaKSouboru),
                    Interpret = soubor.Tag.FirstPerformer ?? "Unknown",
                    Album = soubor.Tag.Album ?? "Unknown",
                    Delka = soubor.Properties.Duration,
                    PrebalAlba = prebalAlba,
                    CestaKSouboru = cestaKSouboru
                };
            });
        }

        /// <summary>
        /// Metoda slouží k uložení metadat skladby
        /// </summary>
        /// <param name="song">skladba, u které chceme uložit metadata</param>
        public async Task Save(Song song)
        {
            await Task.Run(() =>
            {
                var soubor = TagLib.File.Create(song.CestaKSouboru);

                soubor.Tag.Title = song.Nazev;

                if (song.Interpret != null)
                {
                    soubor.Tag.Performers = new[] { song.Interpret };
                }
                else
                {
                    soubor.Tag.Performers = Array.Empty<string>();
                }

                soubor.Tag.Album = song.Album;

                if (song.PrebalAlba != null)
                {
                    // Uložení nového přebalu alba do metadat
                    Picture picture = new Picture(new ByteVector(song.PrebalAlba))
                    {
                        Type = PictureType.FrontCover
                    };

                    soubor.Tag.Pictures = new[] { picture };
                }

                soubor.Save();
            });
        }
    }
}
