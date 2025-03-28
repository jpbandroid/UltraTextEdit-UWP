﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using UTE_UWP_.Helpers;
using UTE_UWP_.Services;

using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UTE_UWP_.Views
{
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set { Set(ref _elementTheme, value); }
        }

        private string _versionDescription;

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { Set(ref _versionDescription, value); }
        }

        public List<string> accentcolors = new List<string>
        {
            "Default",
            "Blue",
            "Seafoam",
            "Slate Green",
            "Crimson",
            "Lilac"
        };

        public SettingsPage()
        {
            InitializeComponent();
                
            this.Background = new SolidColorBrush(Colors.Transparent);

            var LocalSettings = ApplicationData.Current.LocalSettings;

            if ((string)LocalSettings.Values["AccentTheme"] == "Slate Green")
            {
                AccentBox.SelectedItem = "Slate Green";
            }
            if ((string)LocalSettings.Values["AccentTheme"] == "Lilac")
            {
                AccentBox.SelectedItem = "Lilac";
            }
            if ((string)LocalSettings.Values["AccentTheme"] == "Crimson")
            {
                AccentBox.SelectedItem = "Crimson";
            }
            if ((string)LocalSettings.Values["AccentTheme"] == "Seafoam")
            {
                AccentBox.SelectedItem = "Seafoam";
            }
            if ((string)LocalSettings.Values["AccentTheme"] == "Blue")
            {
                AccentBox.SelectedItem = "Blue";
            }
            if ((string)LocalSettings.Values["AccentTheme"] == "Default")
            {
                AccentBox.SelectedItem = "Default";
            }


            if ((string)LocalSettings.Values["TextWrapping"] == "No wrap")
            {
                TextWrapComboBox.SelectedItem = "No wrap";
            }
            if ((string)LocalSettings.Values["TextWrapping"] == "Wrap")
            {
                TextWrapComboBox.SelectedItem = "Wrap";
            }
            if ((string)LocalSettings.Values["TextWrapping"] == "Wrap whole words")
            {
                TextWrapComboBox.SelectedItem = "Wrap whole words";
            }


            if (LocalSettings.Values["SpellCheck"] != null)
            {
                if ((string)LocalSettings.Values["SpellCheck"] == "On")
                {
                    spellcheckBox.IsChecked = true;

                }
                if ((string)LocalSettings.Values["SpellCheck"] == "Off")
                {
                    spellcheckBox.IsChecked = false;
                }
            }
            else
            {
                LocalSettings.Values["SpellCheck"] = "Off";
                spellcheckBox.IsChecked = false;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            VersionDescription = GetVersionDescription();
            await Task.CompletedTask;
        }

        private string GetVersionDescription()
        {
            var appName = "AppDisplayName".GetLocalized();
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private async void ThemeChanged_CheckedAsync(object sender, RoutedEventArgs e)
        {
            var param = (sender as RadioButton)?.CommandParameter;

            if (param != null)
            {
                await ThemeSelectorService.SetThemeAsync((ElementTheme)param);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        public List<string> Fonts;

        #region Appearance
        public int DocumentViewPadding;

        public string DefaultFont;

        // Modes:
        // 0. No wrap
        // 1. Wrap
        // 2. Wrap whole words

        public int TextWrapping;

        // Modes:
        // 0. Light
        // 1. Dark
        // 2. Default

        public int Theme;
        #endregion

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void GH_Navigate(object sender, RoutedEventArgs e)
        {
            // The URI to launch
            string uriToLaunch = @"https://github.com/jpbandroid/UltraTextEdit-UWP";

            // Create a Uri object from a URI string 
            var uri = new Uri(uriToLaunch);

            // Launch the URI
            async void DefaultLaunch()
            {
                // Launch the URI
                var success = await Windows.System.Launcher.LaunchUriAsync(uri);

                if (success)
                {
                    // URI launched
                }
                else
                {
                    // URI launch failed
                }
            }
            DefaultLaunch();
        }

        private void VIDsButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Content is Frame rootFrame)
            {
                rootFrame.Navigate(typeof(VelocityIDsPage));
            }
        }

        private void AccentBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var LocalSettings = ApplicationData.Current.LocalSettings;
            if (AccentBox.SelectedItem != null)
            {
                if ((string)AccentBox.SelectedItem == "Default")
                {
                    LocalSettings.Values["AccentTheme"] = "Default";
                } else if ((string)AccentBox.SelectedItem == "Slate Green")
                {
                    LocalSettings.Values["AccentTheme"] = "Slate Green";
                } else if ((string)AccentBox.SelectedItem == "Lilac")
                {
                    LocalSettings.Values["AccentTheme"] = "Lilac";
                }
                else if ((string)AccentBox.SelectedItem == "Seafoam")
                {
                    LocalSettings.Values["AccentTheme"] = "Seafoam";
                }
                else if ((string)AccentBox.SelectedItem == "Blue")
                {
                    LocalSettings.Values["AccentTheme"] = "Blue";
                }
                else if ((string)AccentBox.SelectedItem == "Crimson")
                {
                    LocalSettings.Values["AccentTheme"] = "Crimson";
                }
            }   
        }

        private void spellcheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var LocalSettings = ApplicationData.Current.LocalSettings;
            if (LocalSettings.Values["SpellCheck"] != null)
            {
                LocalSettings.Values["SpellCheck"] = "On";
            }
        }

        private void spellcheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var LocalSettings = ApplicationData.Current.LocalSettings;
            if (LocalSettings.Values["SpellCheck"] != null)
            {
                LocalSettings.Values["SpellCheck"] = "Off";
            }
        }

        private void TextWrapComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var LocalSettings = ApplicationData.Current.LocalSettings;
            if (TextWrapComboBox.SelectedItem != null) {
                if (LocalSettings.Values["TextWrapping"] != null)
                {
                    LocalSettings.Values["TextWrapping"] = TextWrapComboBox.SelectedItem.ToString();
                } else
                {
                    LocalSettings.Values["TextWrapping"] = "Wrap";
                }
            }
        }

        private void LocalizationContrib_Navigate(object sender, RoutedEventArgs e)
        {
            // The URI to launch
            string uriToLaunch = @"https://crowdin.com/project/ultratextedit-uwp";

            // Create a Uri object from a URI string 
            var uri = new Uri(uriToLaunch);

            // Launch the URI
            async void DefaultLaunch()
            {
                // Launch the URI
                var success = await Windows.System.Launcher.LaunchUriAsync(uri);

                if (success)
                {
                    // URI launched
                }
                else
                {
                    // URI launch failed
                }
            }
            DefaultLaunch();
        }

        public void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UTEUpdateLauncher updateLauncher = new UTEUpdateLauncher();
            updateLauncher.LaunchUTEUpdate();
        }
    }
}
