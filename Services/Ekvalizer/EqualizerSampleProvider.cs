using Hudebni_Prehravac_OctaBeats.Models;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudebni_Prehravac_OctaBeats.Services.Ekvalizer
{
    /// <summary>
    /// Třída sloužící k implementování rozhraní ISampleProvider a obsluze daných metod
    /// </summary>
    public class EqualizerSampleProvider : ISampleProvider
    {
        /// <summary>
        /// Zdroj zvukových vzorků
        /// </summary>
        private readonly ISampleProvider sourceProvider;

        /// <summary>
        /// Jednotlivá pásma ekvalizéru
        /// </summary>
        private readonly IList<PasmoEkvalizeru> pasma;

        /// <summary>
        /// Filtry pro jednotlivé kanály a pásma
        /// </summary>
        private readonly BiQuadFilter[,] filters;

        /// <summary>
        /// Počet audio kanálů
        /// </summary>
        private readonly int channels;

        /// <summary>
        /// Počet pásem ekvalizéru
        /// </summary>
        private readonly int bandCount;

        /// <summary>
        /// Určuje, zda je potřeba přenastavit nové filtry (kanály) zvuku
        /// </summary>
        private bool updated;

        /// <summary>
        /// Určuje, zda je ekvalizér aktuálně povolený
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="sourceProvider">Rozhraní pro zdroj zvukových vzorků</param>
        /// <param name="pasma">Jednotlivá pásma ekvalizéru</param>
        public EqualizerSampleProvider(ISampleProvider sourceProvider, IList<PasmoEkvalizeru> pasma)
        {
            this.sourceProvider = sourceProvider;
            this.pasma = pasma;

            channels = sourceProvider.WaveFormat.Channels;
            bandCount = pasma.Count;

            filters = new BiQuadFilter[channels, bandCount];

            CreateFilters();
        }

        /// <summary>
        /// Metoda slouží k vytvoření nových zvukových kanálů (filtrů)
        /// </summary>
        private void CreateFilters()
        {
            for (int bandIndex = 0; bandIndex < bandCount; bandIndex++)
            {
                PasmoEkvalizeru pasmo = pasma[bandIndex];

                for (int channel = 0; channel < channels; channel++)
                {
                    if (filters[channel, bandIndex] == null)
                    {
                        filters[channel, bandIndex] =
                            BiQuadFilter.PeakingEQ(
                                sourceProvider.WaveFormat.SampleRate,
                                pasmo.Frekvence,
                                pasmo.SirkaPasma,
                                pasmo.Zesileni);
                    }

                    else
                    {
                        filters[channel, bandIndex].SetPeakingEq(
                            sourceProvider.WaveFormat.SampleRate,
                            pasmo.Frekvence,
                            pasmo.SirkaPasma,
                            pasmo.Zesileni);
                    }
                }
            }
        }

        /// <summary>
        /// Metoda slouží k informování o provedení změň kanálů
        /// </summary>
        public void Update()
        {
            updated = true;
        }

        /// <summary>
        /// Metoda slouží k aktualizaci hodnot jednotlivých pásem ekvalizéru
        /// </summary>
        /// <param name="novaPasma">Nová pásma</param>
        public void UpdateEqualizer(IList<PasmoEkvalizeru> novaPasma)
        {
            for (int i = 0; i < bandCount && i < novaPasma.Count; i++)
            {
                pasma[i].Zesileni = novaPasma[i].Zesileni;
                pasma[i].SirkaPasma = novaPasma[i].SirkaPasma;
            }

            Update();
        }

        /// <summary>
        /// Formát přehrávaného zvuku
        /// </summary>
        public WaveFormat WaveFormat => sourceProvider.WaveFormat;

        /// <summary>
        /// Metoda slouží k čtení zvukových vzorků
        /// </summary>
        /// <param name="buffer">Pole vzorků</param>
        /// <param name="offset">Index začátku zápisu</param>
        /// <param name="count">Maximální počet vzorků</param>
        /// <returns>Celkový počet načtených a zpracovaných vzorků</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = sourceProvider.Read(buffer, offset, count);

            // Pokud není ekvalizér povolený, tak se použijí výchozí nulová pásma
            if (!IsEnabled)
            {
                return samplesRead;
            }

            if (updated)
            {
                CreateFilters();
                updated = false;
            }

            for (int sample = 0; sample < samplesRead; sample++)
            {
                int channel = sample % channels;

                float value = buffer[offset + sample];

                for (int band = 0; band < bandCount; band++)
                {
                    value = filters[channel, band].Transform(value);
                }

                buffer[offset + sample] = value;
            }

            return samplesRead;
        }
    }
}
