using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Models
{
    /// <summary>
    /// Statická třída pro inicializaci a uchování jednotlivých pásem ekvalizéru
    /// </summary>
    public static class Presety
    {
        /// <summary>
        /// Slovník jednotlivých přednastavení (presetů)
        /// </summary>
        public static Dictionary<TypPrednastaveni, float[]> PresetyEkvalizeru { get; }

        /// <summary>
        /// Výchozí hodnota zesílení pásem
        /// </summary>
        private const float VychoziHodnotaZesileni = 0f;

        /// <summary>
        /// Statický konstruktor pro inicializaci slovníku přednastavení
        /// </summary>
        static Presety()
        {
            PresetyEkvalizeru = new Dictionary<TypPrednastaveni, float[]>()
            {
                { TypPrednastaveni.Custom, new float[] { VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni } },
                { TypPrednastaveni.Rock, new float[] {   4.8f,   3.1f,  -5.8f,  -8.0f,  -3.3f,   4.0f,   8.9f,  11.1f,  11.1f,  11.1f } },
                { TypPrednastaveni.Pop,  new float[] {  -1.8f,   4.9f,   7.1f,   8.0f,   5.8f,  -1.8f,  -2.7f,  -2.7f,  -1.8f,  -1.8f } },
                { TypPrednastaveni.Reggae, new float[] {   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,  -5.8f,   VychoziHodnotaZesileni,   6.7f,   6.7f,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni } },
                { TypPrednastaveni.Techno, new float[] {   8.0f,   5.8f,   VychoziHodnotaZesileni,  -5.8f,  -5.3f,   VychoziHodnotaZesileni,   8.0f,   9.8f,   9.8f,   8.9f } },
                { TypPrednastaveni.Ska, new float[] {  -2.7f,  -4.9f,  -4.0f,   VychoziHodnotaZesileni,   4.0f,   5.8f,   8.9f,   9.8f,  11.1f,   9.8f } },
                { TypPrednastaveni.Classical, new float[] {   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,  -7.1f,  -7.1f,  -7.1f,  -9.8f } },
                { TypPrednastaveni.Club, new float[] {   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   2.2f,   3.6f,   3.6f,   3.6f,   2.2f,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni } },
                { TypPrednastaveni.Dance, new float[] {   9.8f,   7.1f,   2.2f,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni,  -5.8f,  -7.1f,  -7.1f,   VychoziHodnotaZesileni,   VychoziHodnotaZesileni } },
                { TypPrednastaveni.Headphones, new float[] {   4.9f,  11.1f,   5.8f,  -3.3f,  -2.7f,   1.8f,   4.9f,   9.8f,  12.0f,  12.0f } },
                { TypPrednastaveni.Vocal, new float[] {  -2.7f,  -5.8f,  -5.8f,   1.8f,   5.8f,   5.8f,   3.6f,   1.8f,   VychoziHodnotaZesileni,  -4.0f } }
            };
        }
    }
}
