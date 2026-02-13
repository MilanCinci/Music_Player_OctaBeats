using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Hudebni_Prehravac_OctaBeats.Persistence
{
    /// <summary>
    /// Třída sloužící ke správě operací se soubory
    /// </summary>
    public static class SpravaSouboru
    {
        /// <summary>
        /// Generická metoda slouží k uložení vybraných dat do JSON souboru podle uvedené cesty
        /// </summary>
        /// <typeparam name="T">Generický datový typ T</typeparam>
        /// <param name="cesta">Cesta k souboru</param>
        /// <param name="data">Data, která chceme uložit</param>
        public static async Task UlozDoSouboru<T>(string cesta, T data)
        {
            if (String.IsNullOrEmpty(cesta))
            {
                throw new ArgumentException();
            }

            if (data == null)
            {
                throw new ArgumentNullException();
            }

            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

                string? adresar = Path.GetDirectoryName(cesta);
                if (!String.IsNullOrEmpty(adresar))
                {
                    Directory.CreateDirectory(adresar);
                }

                await File.WriteAllTextAsync(cesta, json);
            }

            catch (Exception ex)
            {
                LogError(ex, "Error occurred while saving to JSON!", nameof(SpravaSouboru));
                throw;
            }
        }

        /// <summary>
        /// Generická metoda slouží k získání dat ze souboru
        /// </summary>
        /// <typeparam name="T">Generický datový typ T</typeparam>
        /// <param name="cesta">Cesta k souboru</param>
        /// <returns>Vrací načtená data, pokud soubor neexistuje vrátí prázdný objekt definovaného datového typu</returns>
        public static async Task<T> NahrajZeSouboru<T>(string cesta) where T : new()
        {
            if (!File.Exists(cesta))
            {
                return new T();
            }

            try
            {
                var json = await File.ReadAllTextAsync(cesta);
                return JsonSerializer.Deserialize<T>(json) ?? new T();
            }

            catch (JsonException ex)
            {
                LogError(ex, "Error occurred while reading from JSON!", nameof(SpravaSouboru));
                return new T();
            }

            catch(IOException ex)
            {
                LogError(ex, "Error occurred while reading from JSON!", nameof(SpravaSouboru));
                throw;
            }
        }

        /// <summary>
        /// Metoda slouží k logování chyb do error logu
        /// </summary>
        /// <param name="ex">Název chyby, která nastala</param>
        /// <param name="doplnek">Doplňující informace o chybě</param>
        /// <param name="nazevMetody">Název metody, kterou výjimku zachytila</param>
        public static void LogError(Exception ex, string doplnek = "", [CallerMemberName] string nazevMetody = "")
        {
            try
            {
                string cestaLogu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OctaBeats", "error_log.txt");
                string zprava = $"[{DateTime.Now}] Method: {nazevMetody} | Info: {doplnek}\nError: {ex.Message}\n";

                File.AppendAllText(cestaLogu, zprava);
            }

            catch 
            {
                
            }
        }
    }
}
