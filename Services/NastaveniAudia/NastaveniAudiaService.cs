using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Services.NastaveniAudia
{
    /// <summary>
    /// Třída sloužící k implementování rozhraní INastaveniAudiaService a obsluze daných metod
    /// </summary>
    public class NastaveniAudiaService : INastaveniAudiaService
    {
        /// <summary>
        /// Metoda slouží k načtení uloženého nastavení audia
        /// </summary>
        /// <returns>Vrací nastavení audia</returns>
        public async Task<NastaveniAudio?> Load()
        {
            try
            {
                if (!File.Exists(NastaveniAudio.CestaKSouboru))
                {
                    return new NastaveniAudio();
                }

                return await SpravaSouboru.NahrajZeSouboru<NastaveniAudio>(NastaveniAudio.CestaKSouboru) ?? new NastaveniAudio();
            }

            catch (Exception)
            {
                //TODO
                return new NastaveniAudio();
            }
        }

        /// <summary>
        /// Metoda slouží k uložení aktuálního nastavení audia
        /// </summary>
        /// <param name="nastaveniAudia">Aktuální nastavení audia, které chceme uložit</param>
        public async Task Save(NastaveniAudio nastaveniAudia)
        {
            try
            {
                var adresar = Path.GetDirectoryName(NastaveniAudio.CestaKSouboru);

                if (!Directory.Exists(adresar))
                {
                    Directory.CreateDirectory(adresar!);
                }

                await SpravaSouboru.UlozDoSouboru(NastaveniAudio.CestaKSouboru, nastaveniAudia);
            }

            catch (Exception)
            {
                //TODO
            }
        }
    }
}
