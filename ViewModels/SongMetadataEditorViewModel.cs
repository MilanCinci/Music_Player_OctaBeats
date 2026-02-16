using Hudebni_Prehravac_OctaBeats.Commands;
using Hudebni_Prehravac_OctaBeats.Models;
using Hudebni_Prehravac_OctaBeats.Persistence;
using Hudebni_Prehravac_OctaBeats.Services.Dialog;
using Hudebni_Prehravac_OctaBeats.Services.Lokalizace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Hudebni_Prehravac_OctaBeats.ViewModels
{
    /// <summary>
    /// ViewModel pro obsluhu metod pro editaci metadat skladeb
    /// </summary>
    public class SongMetadataEditorViewModel : BaseViewModel, IDataErrorInfo
    {
        private readonly ILokalizaceService _lokalizaceService;
        private readonly IDialogService _dialogService;

        // Vlastnosti pro vazbu v XAML
        public string Nazev { get; set; }
        public string Interpret { get; set; }
        public string Album { get; set; }
        public string RokVydani { get; set; }
        public string Zanr { get; set; }
        public string Delka { get; set; }
        private byte[]? prebalAlba;
        public byte[]? PrebalAlba
        {
            get => prebalAlba;
            set { prebalAlba = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Maximální šířka přebalu alba (v px)
        /// </summary>
        private const int MaxSirkaObrazku = 1280;

        /// <summary>
        /// Maximální výška přebalu alba (v px)
        /// </summary>
        private const int MaxVyskaObrazku = 720;

        /* Příkazy pro obsluhu jednotlivých metod */
        public ICommand PotvrditCommand { get; }
        public ICommand VybratPrebalCommand { get; }
        public ICommand StahnoutPrebalCommand { get; }
        public ICommand OdstranitPrebalCommand { get; }

        /// <summary>
        /// Akce pro zavření dialogu
        /// </summary>
        public event Action<bool>? ZavritDialog;

        // Implementace IDataErrorInfo pro validaci
        public string Error => String.Empty;
        public string this[string columnName]
        {
            get
            {
                string? result = String.Empty;
                switch (columnName)
                {
                    case nameof(Nazev):
                        if (String.IsNullOrWhiteSpace(Nazev))
                        {
                            result = _lokalizaceService["ErrorNameEmpty"];                           
                        }
                        return result;

                    case nameof(RokVydani):
                        if (!String.IsNullOrEmpty(RokVydani))
                        {
                            if (!uint.TryParse(RokVydani, out _) || RokVydani.Length != 4)                               
                            {
                                result = _lokalizaceService["ErrorInvalidYearFormat"];
                            }

                            else if (int.Parse(RokVydani) > DateTime.Now.Year)
                            {
                                result = _lokalizaceService["ErrorYearFuture"];
                            }                           
                        }
                        return result;
                }

                // Pokud není detekována žádná chyba, tak použijeme lokalizaci
                return _lokalizaceService![columnName];
            }
        }

        /// <summary>
        /// Parametrický konstruktor pro inicializaci
        /// </summary>
        /// <param name="song">Skladba, kterou chceme editovat</param>
        /// <param name="lokalizaceService">Servis pro obsluhu metod lokalizace</param>
        /// <param name="dialogService">Servis pro zobrazení příslušných dialogů</param>
        public SongMetadataEditorViewModel(Song song, ILokalizaceService lokalizaceService, IDialogService dialogService)
        {
            _lokalizaceService = lokalizaceService;
            _dialogService = dialogService;

            // Načtení stávajících dat
            Nazev = song.Nazev ?? Path.GetFileNameWithoutExtension(song.CestaKSouboru);
            Interpret = song.Interpret ?? "Unknown";
            Album = song.Album ?? "Unknown";
            RokVydani = song.RokVydani?.ToString() ?? "";
            Zanr = song.Zanr ?? "";
            Delka = song.Delka.ToString(@"mm\:ss");
            PrebalAlba = song.PrebalAlba;

            PotvrditCommand = new RelayCommand(_ =>
            {
                if (JeValidni())
                {
                    ZavritDialog?.Invoke(true);
                }
            });

            VybratPrebalCommand = new RelayCommand(_ => VyberNovyPrebal());
            StahnoutPrebalCommand = new RelayCommand(_ => StahniPrebal());
            OdstranitPrebalCommand = new RelayCommand(_ => PrebalAlba = null);
        }

        /// <summary>
        /// Metoda slouží k výběru nového obrázku z disku a jeho automatickému zmenšení
        /// </summary>
        private void VyberNovyPrebal()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "Select album cover",
                    Filter = "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    byte[] data = File.ReadAllBytes(openFileDialog.FileName);

                    // Zmenšení obrázku před uložením do vlastnosti
                    PrebalAlba = ZmensiObrazek(data, MaxSirkaObrazku, MaxVyskaObrazku);
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while loading the new album cover!", nameof(VyberNovyPrebal));
                _dialogService.ShowError(String.Format(_lokalizaceService["ErrorLoadingAlbumCover"], ex.Message));
            }
        }

        /// <summary>
        /// Metoda slouží ke stažení přebalu alba na disk
        /// </summary>
        private void StahniPrebal()
        {
            if(PrebalAlba == null)
            {
                _dialogService.ShowWarning(_lokalizaceService["WarningAlbumCoverNull"]);
                return;
            }

            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Save album cover",
                    FileName = $"{Album} - Cover art",
                    Filter = "Image JPG (*.jpg)|*.jpg|Image PNG (*.png)|*.png",
                    DefaultExt = "jpg",
                    AddExtension = true // Automaticky přidá .jpg, pokud ho uživatel nenapíše
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {

                    string ext = Path.GetExtension(saveFileDialog.FileName).ToLower();
                    byte[]? dataToSave = PrebalAlba;

                    // Pokud uživatel vybral PNG, musí se JPG v paměti překódovat na PNG
                    if (ext == ".png")
                    {
                        using (var ms = new MemoryStream(PrebalAlba))
                        {
                            var bitmap = BitmapFrame.Create(ms);
                            using (var outMs = new MemoryStream())
                            {
                                var encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                                encoder.Save(outMs);
                                dataToSave = outMs.ToArray();
                            }
                        }
                    }

                    File.WriteAllBytes(saveFileDialog.FileName, dataToSave);
                    _dialogService.ShowInfo(_lokalizaceService["InfoSuccessDownloadAlbumCover"]);
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while downloading the album cover!", nameof(StahniPrebal));
                _dialogService.ShowError(String.Format(_lokalizaceService["ErrorSavingAlbumCover"], ex.Message));
            }
        }

        /// <summary>
        /// Metoda slouží ke změně velikosti přebalu v paměti, aby nebyl zbytečně obsáhlý
        /// </summary>
        /// <param name="data">Přebal, který chceme zmenšit</param>
        /// <param name="maxSirka">Maximální šířka přebalu</param>
        /// <param name="maxVyska">Maximální výška přebalu</param>
        /// <returns>Vrací zmenšený obrázek</returns>
        private byte[] ZmensiObrazek(byte[] data, uint maxSirka, uint maxVyska)
        {
            try
            {
                using (var ms = new MemoryStream(data))
                {
                    var bitmap = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                    // Vypočítání poměru, zda je potřeba přebal zmenšovat, aby nezabíral tolik místa
                    double ratioX = (double)maxSirka / bitmap.PixelWidth;
                    double ratioY = (double)maxVyska / bitmap.PixelHeight;
                    double ratio = Math.Min(ratioX, ratioY);

                    // Pokud je přebal už teď menší než požadované maximum, nemusíme ho zmenšovat
                    if (ratio >= 1)
                    {
                        return data;
                    }

                    var resized = new TransformedBitmap(bitmap, new ScaleTransform(ratio, ratio));

                    using (var outMs = new MemoryStream())
                    {
                        var encoder = new JpegBitmapEncoder { QualityLevel = 90 };
                        encoder.Frames.Add(BitmapFrame.Create(resized));
                        encoder.Save(outMs);
                        return outMs.ToArray();
                    }
                }
            }

            catch (Exception ex)
            {
                SpravaSouboru.LogError(ex, "Error occurred while resizing the album cover!", nameof(ZmensiObrazek));
                throw;
            }
        }

        /// <summary>
        /// Metoda slouží k validaci, zda jsou všechna pole správně vyplněna
        /// </summary>
        private bool JeValidni()
        {
            return String.IsNullOrEmpty(this[nameof(Nazev)]) && String.IsNullOrEmpty(this[nameof(RokVydani)]);
        }
    }
}