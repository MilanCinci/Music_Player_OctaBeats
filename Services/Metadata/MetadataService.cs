using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
using System;
using System.Drawing.Printing;
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
        private readonly ILokalizaceService _lokalizaceService;

        /// <summary>
        /// Výchozí hodnota, když nejsou uvedeny konkrétní řetězce
        /// </summary>
        private static string VychoziHodnotaNeznama = "Unknown";

        /// <summary>
        /// Výchozí hodnota, když není uveden rok vydání skladby(alba)
        /// </summary>
        private const int VychoziRokVydani = 0;

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="lokalizaceService">Servis pro obsluhu metod lokalizace aplikace</param>
        public MetadataService(ILokalizaceService lokalizaceService)
        {
            _lokalizaceService = lokalizaceService;
        }

        /// <summary>
        /// Metoda slouží k načtení uložených metadat o skladbě
        /// </summary>
        /// <param name="cestaKSouboru">Cesta k souboru se skladbami</param>
        /// <returns>Vrací metadata skladby</returns>
        public async Task<Song> Load(string cestaKSouboru)
        {
            if (String.IsNullOrEmpty(cestaKSouboru))
            {
                throw new ArgumentException(_lokalizaceService["ErrorInvalidPath"]);
            }

            // Kontrola, zda soubor na disku vůbec existuje
            if (!System.IO.File.Exists(cestaKSouboru))
            {
                throw new FileNotFoundException(_lokalizaceService["ErrorFileNotFound"]);
            }

            try
            {
                return await Task.Run(() =>
                {
                    using (var soubor = TagLib.File.Create(cestaKSouboru))
                    {
                        byte[]? prebalAlba = null;

                        // Získání přebalu alba, pokud je k dispozici
                        if (soubor.Tag.Pictures != null && soubor.Tag.Pictures.Length > 0)
                        {
                            prebalAlba = soubor.Tag.Pictures[0].Data.Data;
                        }

                        return new Song
                        {
                            Nazev = soubor.Tag.Title ?? Path.GetFileNameWithoutExtension(cestaKSouboru),
                            Interpret = soubor.Tag.FirstPerformer ?? VychoziHodnotaNeznama,
                            Album = soubor.Tag.Album ?? VychoziHodnotaNeznama,
                            Delka = soubor.Properties.Duration,
                            PrebalAlba = prebalAlba,
                            Zanr = soubor.Tag.FirstGenre ?? String.Empty,
                            RokVydani = soubor.Tag.Year,
                            CestaKSouboru = cestaKSouboru
                        };
                    }
                });
            }

            catch (TagLib.UnsupportedFormatException)
            {
                throw new InvalidDataException(String.Format(_lokalizaceService["ErrorUnsupportedFileFormat"], cestaKSouboru));
            }

            catch (TagLib.CorruptFileException)
            {
                throw new InvalidDataException(String.Format(_lokalizaceService["ErrorCorruptedFile"], cestaKSouboru));
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while loading the song metadata!", nameof(Load));
                throw new IOException(String.Format(_lokalizaceService["ErrorMetadataNotLoaded"], ex.Message));
            }
        }

        /// <summary>
        /// Metoda slouží k uložení metadat skladby
        /// </summary>
        /// <param name="song">skladba, u které chceme uložit metadata</param>
        /// <returns>Vrací Task</returns>
        public async Task Save(Song song)
        {
            if (song == null || String.IsNullOrEmpty(song.CestaKSouboru))
            {
                return;
            }

            if (!System.IO.File.Exists(song.CestaKSouboru))
            {
                return;
            }

                try
                {
                    await Task.Run(() =>
                    {
                        using (var soubor = TagLib.File.Create(song.CestaKSouboru))
                        {                          
                            soubor.Tag.Title = song.Nazev;

                            if (song.Interpret != null)
                            {
                                if(song.Interpret.Equals(VychoziHodnotaNeznama, StringComparison.OrdinalIgnoreCase))
                                {
                                    soubor.Tag.Performers = Array.Empty<string>();
                                }

                                else
                                {
                                    soubor.Tag.Performers = new[] { song.Interpret };
                                }                                  
                            }

                            else
                            {
                                soubor.Tag.Performers = Array.Empty<string>();
                            }
                    
                            if(song.Album != null)
                            {
                                if (song.Album.Equals(VychoziHodnotaNeznama, StringComparison.OrdinalIgnoreCase))
                                {
                                    soubor.Tag.Album = String.Empty;
                                }

                                else
                                {
                                    soubor.Tag.Album = song.Album;
                                }
                            }

                            else
                            {
                                soubor.Tag.Album = String.Empty;
                            }

                            if (song.PrebalAlba != null)
                            {
                                Picture picture = new Picture(new ByteVector(song.PrebalAlba))
                                {
                                    Type = PictureType.FrontCover
                                };
                    
                                soubor.Tag.Pictures = new[] { picture };
                            }

                            else
                            {
                                soubor.Tag.Pictures = Array.Empty<Picture>();
                            }

                            if (song.RokVydani != null)
                            {
                                soubor.Tag.Year = (uint)song.RokVydani;
                            }

                            else
                            {
                                soubor.Tag.Year = VychoziRokVydani;
                            }

                            if(song.Zanr != null)
                            {
                                soubor.Tag.Genres = new string[] { song.Zanr };
                            }

                            else
                            {
                                soubor.Tag.Genres = Array.Empty<string>();
                            }

                            soubor.Save();
                        }
                    });
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while saving the song metadata!", nameof(Save));
                throw;
            }
        }
    }
}