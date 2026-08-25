using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Services.Ekvalizer
{
    /// <summary>
    /// Třída sloužící k implementování rozhraní INastaveniEkvalizeruService a obsluze daných metod
    /// </summary>
    public class NastaveniEkvalizeruService : INastaveniEkvalizeruService
    {
        /// <summary>
        /// Metoda slouží k načtení uloženého nastavení ekvalizéru
        /// </summary>
        /// <returns>Vrací nastavení ekvalizéru</returns>
        public async Task<NastaveniEkvalizer?> Load()
        {
            try
            {
                if (!File.Exists(NastaveniEkvalizer.CestaKSouboru))
                {
                    return new NastaveniEkvalizer();
                }

                return await SpravaSouboru.NahrajZeSouboru<NastaveniEkvalizer>(NastaveniEkvalizer.CestaKSouboru) ?? new NastaveniEkvalizer();
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "", nameof(Load));
                throw;
            }
        }

        /// <summary>
        /// Metoda slouží k uložení aktuálního nastavení ekvalizéru
        /// </summary>
        /// <param name="nastaveniEkvalizer">Aktuální nastavení ekvalizéru, které chceme uložit</param>
        /// <returns>Vrací Task</returns>
        public async Task Save(NastaveniEkvalizer nastaveniEkvalizer)
        {
            try
            {
                var adresar = Path.GetDirectoryName(NastaveniEkvalizer.CestaKSouboru);

                if (adresar == null)
                {
                    return;
                }

                if (!Directory.Exists(adresar))
                {
                    Directory.CreateDirectory(adresar);
                }

                await SpravaSouboru.UlozDoSouboru(NastaveniEkvalizer.CestaKSouboru, nastaveniEkvalizer);
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "", nameof(Save));
            }
        }
    }
}
