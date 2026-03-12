using Microsoft.Win32;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Hudebni_Prehravac_OctaBeats
{
    /// <summary>
    /// Interakční logika pro App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Metoda slouží k načtení hodnot z registrů při startu aplikace
        /// </summary>
        /// <param name="e">startupEventArgs</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string language = "en-US";
            bool isDarkMode = true;

            try
            {
                // Čtení z registrů aplikace OctaBeats
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
                using (var key = baseKey.OpenSubKey(@"Software\OctaBeats"))
                {
                    // Získání hodnot z registrů
                    if (key != null)
                    {
                        language = key.GetValue("Language")?.ToString() ?? language;
                        isDarkMode = (key.GetValue("Theme")?.ToString() ?? "true").Equals("true", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }

            catch 
            {
                // Při chybě se nastaví výchozí hodnoty (en-US, true)
            }

            // Nastavení správného vzhledu do Resources
            string themePath = isDarkMode ? "Resources/Themes/DarkTheme.xaml" : "Resources/Themes/LightTheme.xaml";
            ResourceDictionary themeDict = new ResourceDictionary { Source = new Uri(themePath, UriKind.Relative) };

            // Odstranění starého tématu a přidání nového
            var existingTheme = Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme.xaml"));
            if (existingTheme != null)
            {
                Current.Resources.MergedDictionaries.Remove(existingTheme);
            }

            Current.Resources.MergedDictionaries.Add(themeDict);

            // Uložení do Application Settings
            Hudebni_Prehravac_OctaBeats.Properties.Settings.Default.Language = language;
            Hudebni_Prehravac_OctaBeats.Properties.Settings.Default.IsDarkMode = isDarkMode;
            Hudebni_Prehravac_OctaBeats.Properties.Settings.Default.Save();
        }
    }
}
