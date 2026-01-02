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
    /// Třída sloužící ke správě souborů
    /// </summary>
    public static class SpravaSouboru
    {
        /// <summary>
        /// Generická metoda slouží k uložení vybraných dat do JSON souboru podle uvedené cesty
        /// </summary>
        /// <typeparam name="T">Generický datový typ T</typeparam>
        /// <param name="cesta">Cesta k souboru</param>
        /// <param name="data">Data, která chceme uložit</param>
        public static void UlozDoSouboru<T>(string cesta, T data)
        {
            if (String.IsNullOrEmpty(cesta))
            {
                throw new ArgumentException("Cesta pro uložení souboru nemůže být prázdná ani NULL!");
            }

            if (data == null)
            {
                throw new ArgumentNullException("Data nemůžou být NULL!");
            }

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

            File.WriteAllText(cesta, json);
        }

        /// <summary>
        /// Generická metoda slouží k získání dat ze souboru
        /// </summary>
        /// <typeparam name="T">Generický datový typ T</typeparam>
        /// <param name="cesta">Cesta k souboru</param>
        /// <returns>Vrací načtená data, pokud soubor neexistuje vrátí prázdný objekt definovaného datového typu</returns>
        public static T NahrajZeSouboru<T>(string cesta) where T : new()
        {
            if (!File.Exists(cesta))
            {
                return new T();
            }

            try
            {
                var json = File.ReadAllText(cesta);
                return JsonSerializer.Deserialize<T>(json) ?? new T();
            }

            catch (JsonException ex)
            {
                LogError(ex, "Čtení z JSON souboru");
                return new T();
            }

            catch(IOException ex)
            {
                LogError(ex, "Čtení z JSON souboru");
                throw;
            }
        }

        /// <summary>
        /// Metoda slouží k logování chyb do error logu
        /// </summary>
        /// <param name="ex">Název chyby, která nastala</param>
        /// <param name="kontext">Kontext chyby, kdy vznikla</param>
        public static void LogError(Exception ex, string doplnek = "", [CallerMemberName] string nazevMetody = "")
        {
            try
            {
                string cestaLogu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OctaBeats", "error_log.txt");
                string zprava = $"[{DateTime.Now}] Metoda: {nazevMetody} | Info: {doplnek}\nChyba: {ex.Message}";

                File.AppendAllText(cestaLogu, zprava);
            }

            catch 
            {
                
            }
        }
    }
}
