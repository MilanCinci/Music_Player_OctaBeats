using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public NastaveniAudio? Load()
        {
            try
            {
                if (!File.Exists(NastaveniAudio.CestaKSouboru))
                {
                    return new NastaveniAudio();
                }

                return SpravaSouboru.NahrajZeSouboru<NastaveniAudio>(NastaveniAudio.CestaKSouboru) ?? new NastaveniAudio();
            }

            catch(Exception)
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Metoda slouží k uložení aktuálního nastavení audia
        /// </summary>
        /// <param name="nastaveniAudia">Aktuální nastavení audia, které chceme uložit</param>
        public void Save(NastaveniAudio nastaveniAudia)
        {
            try
            {
                 var adresar = Path.GetDirectoryName(NastaveniAudio.CestaKSouboru);

                 if (!Directory.Exists(adresar))
                 {
                     Directory.CreateDirectory(adresar!);
                 }

                 SpravaSouboru.UlozDoSouboru(NastaveniAudio.CestaKSouboru, nastaveniAudia);
            }   

            catch (Exception)
            {
                throw new Exception();
            }
        }
    }
}
