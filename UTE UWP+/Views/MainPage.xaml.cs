using DocumentFormat.OpenXml.Bibliography;
using MicaForUWP.Media;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTE_UWP_.Helpers;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics.Printing;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WordPad.Helpers;

namespace UTE_UWP_.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private bool saved;
        private bool _wasOpen;
        private string fileNameWithPath;
        private bool _openDialog;
        private string originalDocText;
        public string docText;
        private const double V = 10.5;
        private const int V1 = 28;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            InitializeComponent();

            if (BuildInfo.BeforeWin11)
            {
                if (App.Current.RequestedTheme == ApplicationTheme.Light)
                {
                    Application.Current.Resources["AppTitleBarBrush"] = new BackdropMicaBrush()
                    {
                        LuminosityOpacity = 0.8F,
                        TintOpacity = 0F,
                        BackgroundSource = BackgroundSource.WallpaperBackdrop,
                        Opacity = 1,
                        TintColor = Windows.UI.Color.FromArgb(255, 230, 230, 230),
                        FallbackColor = Windows.UI.Color.FromArgb(255, 230, 230, 230)
                    };
                    this.Background = (Brush)Application.Current.Resources["AppTitleBarBrush"];
                    HomeMenu.Background = (Brush)Application.Current.Resources["AppTitleBarBrush"];
                }
                else
                {
                    Application.Current.Resources["AppTitleBarBrush"] = new BackdropMicaBrush()
                    {
                        LuminosityOpacity = 0.8F,
                        TintOpacity = 0F,
                        BackgroundSource = BackgroundSource.WallpaperBackdrop,
                        Opacity = 1,
                        TintColor = Windows.UI.Color.FromArgb(255, 25, 25, 25),
                        FallbackColor = Windows.UI.Color.FromArgb(25, 25, 25, 25)
                    };
                    this.Background = (Brush)Application.Current.Resources["AppTitleBarBrush"];
                    HomeMenu.Background = (Brush)Application.Current.Resources["AppTitleBarBrush"];
                }

            } else {

            }

            ShareSourceLoad();

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            OdtHelper odtHelper = new OdtHelper();

            var appViewTitleBar = ApplicationView.GetForCurrentView().TitleBar;

            appViewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            appViewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            appViewTitleBar.ButtonForegroundColor = (Windows.UI.Color)Resources["SystemAccentColor"];

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);

            Window.Current.SetTitleBar(AppTitleBar);

            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
            Window.Current.Activated += Current_Activated;

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;

            NavigationCacheMode = NavigationCacheMode.Required;

            EditButton.IsChecked = true;
            CommentsButton.Visibility = Visibility.Collapsed;
            Insert.Visibility = Visibility.Collapsed;
            Comments.Visibility = Visibility.Collapsed;
            Developer.Visibility = Visibility.Collapsed;
            Layout.Visibility = Visibility.Collapsed;
            View.Visibility = Visibility.Collapsed;
            Help.Visibility = Visibility.Collapsed;

            ShareSourceLoad();
            InitializeVIDs();
        }

        private void InitializeVIDs()
        {
            ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
            if (LocalSettings.Values["SpellCheck"] != null)
            {
                if (LocalSettings.Values["SpellCheck"].ToString() == "On")
                {
                    editor.IsSpellCheckEnabled = true;
                }
                else
                {
                    editor.IsSpellCheckEnabled = false;
                }
            }
            else
            {
                LocalSettings.Values["SpellCheck"] = "Off";
            }
            if (LocalSettings.Values["TextWrapping"] != null) {
                if ((string)LocalSettings.Values["TextWrapping"] == "No wrap")
                {
                    editor.TextWrapping = TextWrapping.NoWrap;
                }
                if ((string)LocalSettings.Values["TextWrapping"] == "Wrap")
                {
                    editor.TextWrapping = TextWrapping.Wrap;
                }
                if ((string)LocalSettings.Values["TextWrapping"] == "Wrap whole words")
                {
                    editor.TextWrapping = TextWrapping.WrapWholeWords;
                }
            } else {
                LocalSettings.Values["TextWrapping"] = "Wrap";
            }
            if (LocalSettings.Values["DialogsInRibbonVID"] != null)
            {
                if (LocalSettings.Values["DialogsInRibbonVID"].ToString() == "On")
                {
                    changelogButton.Visibility = Visibility.Visible;
                    firstrunButton.Visibility = Visibility.Visible;
                }
                else
                {
                    changelogButton.Visibility = Visibility.Collapsed;
                    firstrunButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                LocalSettings.Values["DialogsInRibbonVID"] = "Off";
            }
            if (LocalSettings.Values["OdtTextVID"] != null) { }
            else
            {
                LocalSettings.Values["OdtTextVID"] = "Off";
            }
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            AppTitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        // Update the TitleBar based on the inactive/active state of the app
        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            SolidColorBrush defaultForegroundBrush = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources["SystemAccentColor"]);
            SolidColorBrush inactiveForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorDisabledBrush"];

            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                if (App.Current.RequestedTheme == ApplicationTheme.Light)
                {
                    AppTitle.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    AppTitle.Foreground = new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                AppTitle.Foreground = defaultForegroundBrush;
            }
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;

            // Ensure the custom title bar does not overlap window caption controls
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, currMargin.Right, currMargin.Bottom);
        }

        private async void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            if (!saved) { e.Handled = true; await ShowUnsavedDialog(); }
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile(true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = AppTitle.Text.Replace(" - " + "UTE UWP", "");
            if (fileName == "Untitled")
            {
                SaveFile(true);
            } else {
                SaveFile(false);
            }
        }

        public async void SaveFile(bool isCopy)
        {
            string fileName = AppTitle.Text.Replace(" - " + "UTE UWP", "");
            if (isCopy || fileName == "Untitled")
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("Rich Text", new List<string>() { ".rtf" });
                savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });

                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = "New Document";

                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Prevent updates to the remote version of the file until we
                    // finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);
                    // write to file
                    using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))

                        if (file.Name.EndsWith(".txt"))
                        {
                            editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.None, randAccStream);
                        }
                        else
                        {
                            editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.FormatRtf, randAccStream);
                        }

                    // Let Windows know that we're finished changing the file so the
                    // other app can update the remote version of the file.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status != FileUpdateStatus.Complete)
                    {
                        Windows.UI.Popups.MessageDialog errorBox = new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                        await errorBox.ShowAsync();
                    }
                    saved = true;
                    fileNameWithPath = file.Path;
                    AppTitle.Text = file.Name + " - " + "UTE UWP";
                    Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                }
            }
            else if (!isCopy || fileName != "Untitled")
            {
                string path = fileNameWithPath.Replace("\\" + fileName, "");
                try
                {
                    StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync("CurrentlyOpenFile");
                    if (file != null)
                    {
                        // Prevent updates to the remote version of the file until we
                        // finish making changes and call CompleteUpdatesAsync.
                        CachedFileManager.DeferUpdates(file);
                        // write to file
                        using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                            if (file.Name.EndsWith(".txt"))
                            {
                                editor.Document.SaveToStream(TextGetOptions.None, randAccStream);
                            }
                            else
                            {
                                editor.Document.SaveToStream(TextGetOptions.FormatRtf, randAccStream);
                            }


                        // Let Windows know that we're finished changing the file so the
                        // other app can update the remote version of the file.
                        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                        if (status != FileUpdateStatus.Complete)
                        {
                            Windows.UI.Popups.MessageDialog errorBox = new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                            await errorBox.ShowAsync();
                        }
                        saved = true;
                        AppTitle.Text = file.Name + " - " + "UTE UWP";
                        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove("CurrentlyOpenFile");
                    }
                }
                catch (Exception)
                {
                    SaveFile(true);
                }
            }
        }

        private async void Print_Click(object sender, RoutedEventArgs e)
        {
            if (PrintManager.IsSupported())
            {
                try
                {
                    // Show print UI
                    await PrintManager.ShowPrintUIAsync();
                }
                catch
                {
                    // Printing cannot proceed at this time
                    ContentDialog noPrintingDialog = new ContentDialog()
                    {
                        Title = "Printing error",
                        Content = "Sorry, printing can't proceed at this time.",
                        PrimaryButtonText = "OK"
                    };
                    await noPrintingDialog.ShowAsync();
                }
            }
            else
            {
                // Printing is not supported on this device
                ContentDialog noPrintingDialog = new ContentDialog()
                {
                    Title = "Printing not supported",
                    Content = "Sorry, printing is not supported on this device.",
                    PrimaryButtonText = "OK"
                };
                await noPrintingDialog.ShowAsync();
            }
        }

        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            ITextSelection selectedText = editor.Document.Selection;
            if (selectedText != null)
            {
                ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                charFormatting.Bold = FormatEffect.Toggle;
                selectedText.CharacterFormat = charFormatting;
                BB.IsChecked = editor.Document.Selection.CharacterFormat.Bold == FormatEffect.On;
            }
        }

        private async void NewDoc_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView currentAV = ApplicationView.GetForCurrentView();
            CoreApplicationView newAV = CoreApplication.CreateNewView();
            await newAV.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var newWindow = Window.Current;
                var newAppView = ApplicationView.GetForCurrentView();
                newAppView.Title = $"Untitled - UTE UWP";

                var frame = new Frame();
                frame.Navigate(typeof(MainPage));
                newWindow.Content = frame;
                newWindow.Activate();

                await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newAppView.Id,
                    ViewSizePreference.UseMinimum, currentAV.Id, ViewSizePreference.UseMinimum);
            });
        }

        private void SubscriptButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.Text.ITextSelection selectedText = editor.Document.Selection;
            if (selectedText != null)
            {
                Windows.UI.Text.ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                charFormatting.Subscript = Windows.UI.Text.FormatEffect.Toggle;
                selectedText.CharacterFormat = charFormatting;
            }
            Windows.UI.Text.ITextSelection selectedText2 = comments.Document.Selection;
            if (selectedText2 != null)
            {
                Windows.UI.Text.ITextCharacterFormat charFormatting2 = selectedText2.CharacterFormat;
                charFormatting2.Subscript = Windows.UI.Text.FormatEffect.Toggle;
                selectedText2.CharacterFormat = charFormatting2;
            }
        }

        private void SuperscriptButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.Text.ITextSelection selectedText = editor.Document.Selection;
            if (selectedText != null)
            {
                Windows.UI.Text.ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                charFormatting.Superscript = Windows.UI.Text.FormatEffect.Toggle;
                selectedText.CharacterFormat = charFormatting;
            }
            Windows.UI.Text.ITextSelection selectedText2 = comments.Document.Selection;
            if (selectedText2 != null)
            {
                Windows.UI.Text.ITextCharacterFormat charFormatting2 = selectedText2.CharacterFormat;
                charFormatting2.Superscript = Windows.UI.Text.FormatEffect.Toggle;
                selectedText2.CharacterFormat = charFormatting2;
            }
        }

        private void AlignRightButton_Click(object sender, RoutedEventArgs e)
        {
            ITextSelection selectedText = editor.Document.Selection;
            if (selectedText != null)
            {
                // Apply the list style to the selected text.
                var paragraphFormatting = selectedText.ParagraphFormat;
                paragraphFormatting.Alignment = ParagraphAlignment.Right;

            }
            ITextSelection selectedText2 = comments.Document.Selection;
            if (selectedText2 != null)
            {
                // Apply the list style to the selected text.
                var paragraphFormatting2 = selectedText2.ParagraphFormat;
                paragraphFormatting2.Alignment = ParagraphAlignment.Right;

            }
        }

        private void AlignCenterButton_Click(object sender, RoutedEventArgs e)
        {
            ITextSelection selectedText = editor.Document.Selection;
            if (selectedText != null)
            {
                // Apply the list style to the selected text.
                var paragraphFormatting = selectedText.ParagraphFormat;
                paragraphFormatting.Alignment = ParagraphAlignment.Center;

            }
            ITextSelection selectedText2 = comments.Document.Selection;
            if (selectedText2 != null)
            {
                // Apply the list style to the selected text.
                var paragraphFormatting2 = selectedText.ParagraphFormat;
                paragraphFormatting2.Alignment = ParagraphAlignment.Center;

            }
        }

        private void AlignLeftButton_Click(object sender, RoutedEventArgs e)
        {
            ITextSelection selectedText = editor.Document.Selection;
            if (selectedText != null)
            {
                // Apply the list style to the selected text.
                var paragraphFormatting = selectedText.ParagraphFormat;
                paragraphFormatting.Alignment = ParagraphAlignment.Left;

            }
            ITextSelection selectedText2 = comments.Document.Selection;
            if (selectedText2 != null)
            {
                // Apply the list style to the selected text.
                var paragraphFormatting2 = selectedText2.ParagraphFormat;
                paragraphFormatting2.Alignment = ParagraphAlignment.Left;

            }
        }

        private void ItalicButton_Click(object sender, RoutedEventArgs e)
        {
            ITextSelection selectedText = editor.Document.Selection;
            if (selectedText != null)
            {
                ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                charFormatting.Italic = FormatEffect.Toggle;
                selectedText.CharacterFormat = charFormatting;
                IB.IsChecked = editor.Document.Selection.CharacterFormat.Italic == FormatEffect.On;
            }
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            // Open a text file.
            FileOpenPicker open = new FileOpenPicker()
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

            open.FileTypeFilter.Add(".rtf");
            open.FileTypeFilter.Add(".txt");
            if ((string)LocalSettings.Values["OdtTextVID"] == "On") {
                open.FileTypeFilter.Add(".odt");
            }

            StorageFile file = await open.PickSingleFileAsync();
            OdtHelper odtHelper = new OdtHelper();

            if (file != null)
            {
                if (file.FileType == ".rtf")
                {
                    using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        IBuffer buffer = await FileIO.ReadBufferAsync(file);
                        var reader = DataReader.FromBuffer(buffer);
                        reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                        string text = reader.ReadString(buffer.Length);
                        // Load the file into the Document property of the RichEditBox.
                        editor.Document.LoadFromStream(TextSetOptions.FormatRtf, randAccStream);
                        editor.Document.GetText(TextGetOptions.UseObjectText, out originalDocText);
                    }
                }
                else if (file.FileType == ".odt" && (string)LocalSettings.Values["OdtTextVID"] == "On")
                {
                    using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        // Read the file as a stream
                        using (Stream stream = randAccStream.AsStreamForRead())
                        {
                            // Use ZipArchive to extract ODT contents
                            using (var archive = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Read))
                            {
                                // Find the content.xml file inside the ODT archive
                                var contentEntry = archive.GetEntry("content.xml");
                                var stylesEntry = archive.GetEntry("styles.xml");
                                if (contentEntry != null && stylesEntry != null)
                                {
                                    string contentXml, stylesXml;

                                    // Read content.xml
                                    using (var contentStream = contentEntry.Open())
                                    using (var reader = new StreamReader(contentStream))
                                        contentXml = await reader.ReadToEndAsync();

                                    // Read styles.xml
                                    using (var stylesStream = stylesEntry.Open())
                                    using (var reader = new StreamReader(stylesStream))
                                        stylesXml = await reader.ReadToEndAsync();

                                    // Load the ODT content into the RichEditBox
                                    await odtHelper.LoadOdtContentWithStyling(contentXml, stylesXml, archive, editor);
                                }
                                else
                                {
                                    // Handle case where content.xml is missing
                                    await new Windows.UI.Popups.MessageDialog("Invalid ODT file: content.xml not found.").ShowAsync();
                                }
                            }
                        }
                    }
                }
                else if (file.FileType == ".txt")
                {
                    using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        using (Stream stream = randAccStream.AsStreamForRead())
                        {
                            // Use StreamReader with the appropriate encoding (e.g., UTF-8)
                            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                string text = await reader.ReadToEndAsync();

                                // Load the file into the Document property of the RichEditBox.
                                editor.Document.SetText(TextSetOptions.None, text);
                            }
                        }
                    }
                    AppTitle.Text = file.Name + " - " + "UTE UWP";
                    fileNameWithPath = file.Path;
                    saved = true;
                    _wasOpen = true;
                    Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("CurrentlyOpenFile", file);
                }
            }
        }

        private async void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Open an image file.
            FileOpenPicker open = new FileOpenPicker();
            open.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            open.FileTypeFilter.Add(".png");
            open.FileTypeFilter.Add(".jpg");
            open.FileTypeFilter.Add(".jpeg");

            StorageFile file = await open.PickSingleFileAsync();

            if (file != null)
            {
                IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.Read);
                var properties = await file.Properties.GetImagePropertiesAsync();
                int width = (int)properties.Width;
                int height = (int)properties.Height;

                ImageOptionsDialog dialog = new ImageOptionsDialog()
                {
                    DefaultWidth = width,
                    DefaultHeight = height
                };

                ContentDialogResult result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    editor.Document.Selection.InsertImage((int)dialog.DefaultWidth, (int)dialog.DefaultHeight, 0, VerticalCharacterAlignment.Baseline, string.IsNullOrWhiteSpace(dialog.Tag) ? "Image" : dialog.Tag, randAccStream);
                    return;
                }

                // Insert an image
                editor.Document.Selection.InsertImage(width, height, 0, VerticalCharacterAlignment.Baseline, "Image", randAccStream);
            }
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            // Extract the color of the button that was clicked.
            Button clickedColor = (Button)sender;
            var color = (clickedColor.Background as SolidColorBrush).Color;
            editor.Document.Selection.CharacterFormat.ForegroundColor = color;
            //FontColorMarker.SetValue(ForegroundProperty, new SolidColorBrush(color));
            editor.Focus(FocusState.Keyboard);
        }

        private void HighlightButton_Click(object sender, RoutedEventArgs e)
        {
            // Extract the color of the button that was clicked.
            Button clickedColor = (Button)sender;
            var color = (clickedColor.Background as SolidColorBrush).Color;
            editor.Document.Selection.CharacterFormat.BackgroundColor = color;
            //FontColorMarker.SetValue(ForegroundProperty, new SolidColorBrush(color));
            editor.Focus(FocusState.Keyboard);
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            editor.Document.Selection.Copy();
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            editor.Document.Selection.Cut();
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            editor.Document.Selection.Paste(0);
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            editor.Document.Undo();
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            editor.Document.Redo();
        }

        private Task DisplayAboutDialog()
        {
            return Task.CompletedTask;
        }

        public async Task ShowUnsavedDialog()
        {
            string fileName = AppTitle.Text.Replace(" - " + "UTE UWP", "");
            ContentDialog aboutDialog = new ContentDialog
            {
                Title = "Do you want to save changes to " + fileName + "?",
                Content = "There are unsaved changes, want to save them?",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save changes",
                SecondaryButtonText = "No",
                DefaultButton = ContentDialogButton.Primary
            };

            aboutDialog.CloseButtonClick += (s, e) => this._openDialog = false;

            ContentDialogResult result = await aboutDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                SaveFile(true);
            }
            else if (result == ContentDialogResult.Secondary)
            {
                await ApplicationView.GetForCurrentView().TryConsolidateAsync();
            }
        }

        private async void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            await DisplayAboutDialog();
        }

        private void editor_TextChanged(object sender, RoutedEventArgs e)
        {

            var ST = editor.Document.Selection;
            var CF = ST.CharacterFormat;

            editor.Document.GetText(TextGetOptions.UseObjectText, out string textStart);

            if (textStart == "" || string.IsNullOrWhiteSpace(textStart) || _wasOpen)
            {
                saved = true;
            }
            else
            {
                saved = false;
            }

            if (!saved) UnsavedTextBlock.Visibility = Visibility.Visible;
            else UnsavedTextBlock.Visibility = Visibility.Collapsed;
            if (!(FontSizeBox == null))
            {
                if (ST.Length is > 0 or < 0) FontSizeBox.Text = double.NaN.ToString();
                else FontSizeBox.Text = CF.Size.ToString();
            }

            if (ST.Length is 0)
            {
                FontBox.SelectedIndex = FontBox.Items.IndexOf(CF.Name.ToString());
                FontBox.PlaceholderText = "Segoe UI (Default)";
            }
            else
            {
                FontBox.SelectedItem = null;
                FontBox.PlaceholderText = "Multiple";
            }
        }

        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (saved)
            {
                await ApplicationView.GetForCurrentView().TryConsolidateAsync();
            }
            else await ShowUnsavedDialog();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is IActivatedEventArgs args)
            {
                if (args.Kind == ActivationKind.File)
                {
                    var fileArgs = args as FileActivatedEventArgs;
                    StorageFile file = (StorageFile)fileArgs.Files[0];
                    using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        IBuffer buffer = await FileIO.ReadBufferAsync(file);
                        var reader = DataReader.FromBuffer(buffer);
                        reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                        string text = reader.ReadString(buffer.Length);
                        // Load the file into the Document property of the RichEditBox.
                        editor.Document.LoadFromStream(TextSetOptions.FormatRtf, randAccStream);
                        //editor.Document.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, text);
                        AppTitle.Text = file.Name + " - " + "UTE UWP";
                        fileNameWithPath = file.Path;
                    }
                    saved = true;
                    fileNameWithPath = file.Path;
                    Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("CurrentlyOpenFile", file);
                    _wasOpen = true;
                }
            }
        }

        private void FontSizeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (editor != null && editor.Document.Selection != null)
            {
                ITextSelection selectedText = editor.Document.Selection;
                selectedText.CharacterFormat.Size = float.Parse(FontSizeBox.SelectedValue.ToString());
            }
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Content is Frame rootFrame)
            {
                rootFrame.Navigate(typeof(SettingsPageContainer));
            }
        }

        public async void uteverclick(object sender, RoutedEventArgs e)
        {
            AboutUTE aboutUTE = new AboutUTE();
            ContentDialog aboutdialog = new ContentDialog();
            aboutdialog.DefaultButton = ContentDialogButton.Primary;
            aboutdialog.PrimaryButtonText = "OK";
            aboutdialog.Content = aboutUTE;
            await aboutdialog.ShowAsync();
        }

        private void FindButton2_Click(object sender, RoutedEventArgs e)
        {
            textsplitview.IsPaneOpen = true;
        }

        private void closepane(object sender, RoutedEventArgs e)
        {
            textsplitview.IsPaneOpen = false;
        }

        private void RichEditBox_TextChanged(object sender, RoutedEventArgs e)
        {
            editor.Document.GetText(TextGetOptions.UseObjectText, out string textStart);

            if (textStart == "" || string.IsNullOrWhiteSpace(textStart))
            {
                saved = true;
            }
            else
            {
                saved = false;
            }

            if (!saved) UnsavedTextBlock.Visibility = Visibility.Visible;
            else UnsavedTextBlock.Visibility = Visibility.Collapsed;

        }

        private void editor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var ST = editor.Document.Selection;
            //BoldButton.IsChecked = editor.Document.Selection.CharacterFormat.Bold == FormatEffect.On;
            //ItalicButton.IsChecked = editor.Document.Selection.CharacterFormat.Italic == FormatEffect.On;
            //UnderlineButton.IsChecked = editor.Document.Selection.CharacterFormat.Underline == UnderlineType.Single;
            //Selected words
            if (ST.Length > 0 || ST.Length < 0)
            {
                SelWordGrid.Visibility = Visibility.Visible;
                editor.Document.Selection.GetText(TextGetOptions.None, out var seltext);
                var selwordcount = seltext.Split(new char[] { ' ', '\n', '\t', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
                SelWordCount.Text = $"Selected words: {selwordcount}";
            }
            else
            {
                SelWordGrid.Visibility = Visibility.Collapsed;
            }
            editor.Document.GetText(TextGetOptions.None, out var text);
            if (text.Length > 0 && text != " " && text != "" && text != null)
            {
                var separatorList = new char[] { ' ', '\n', '\t', '\r' };
                var wordcount = text.Split(separatorList, StringSplitOptions.RemoveEmptyEntries).Length;
                WordCount.Text = $"Word count: {wordcount}";
            }
            else
            {
                WordCount.Text = $"Word count: 0";
            }
        }

        //To see this code in action, add a call to ShareSourceLoad to your constructor or other
        //initializing function.
        private void ShareSourceLoad()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.DataRequested);
        }

        private void DataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            request.Data.Properties.Title = "UltraTextEdit Share Service";
            request.Data.Properties.Description = "Text sharing for the UTE UWP app";
            request.Data.SetText(editor.TextDocument.ToString());
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            ShareSourceLoad();
            DataTransferManager.ShowShareUI();
        }

        private void CommentsButton_Click(object sender, RoutedEventArgs e)
        {
            commentsplitview.IsPaneOpen = true;
            CommentsButton.Visibility = Visibility.Visible;
        }

        private void closecomments(object sender, RoutedEventArgs e)
        {
            commentsplitview.IsPaneOpen = false;
            Comments.Visibility = Visibility.Collapsed;
            CommentsButton.Visibility = Visibility.Collapsed;
            Home.Visibility = Visibility.Visible;
            EditButton.IsChecked = true;
        }

        /* Method to create a table format string which can directly be set to 
   RichTextBoxControl. Rows, columns and cell width are passed as parameters 
   rather than hard coding as in previous example.*/
        private String InsertTableInRichTextBox(int rows, int cols, int width)
        {
            //Create StringBuilder Instance
            StringBuilder strTableRtf = new StringBuilder();

            //beginning of rich text format
            strTableRtf.Append(@"{\rtf1 ");

            //Variable for cell width
            int cellWidth;

            //Start row
            strTableRtf.Append(@"\trowd");

            //Loop to create table string
            for (int i = 0; i < rows; i++)
            {
                strTableRtf.Append(@"\trowd");

                for (int j = 0; j < cols; j++)
                {
                    //Calculate cell end point for each cell
                    cellWidth = (j + 1) * width;

                    //A cell with width 1000 in each iteration.
                    strTableRtf.Append(@"\cellx" + cellWidth.ToString());
                }

                //Append the row in StringBuilder
                strTableRtf.Append(@"\intbl \cell \row");
            }
            strTableRtf.Append(@"\pard");
            strTableRtf.Append(@"}");
            var strTableString = strTableRtf.ToString();
            editor.Document.Selection.SetText(TextSetOptions.FormatRtf, strTableString);
            return strTableString;

        }

        private async void AddTableButton_Click(object sender, RoutedEventArgs e)
        {
            var dialogtable = new TableDialog();
            await dialogtable.ShowAsync();
            InsertTableInRichTextBox(dialogtable.rows, dialogtable.columns, 1000);
        }

        private void AddSymbolButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void SymbolButton_Click(object sender, RoutedEventArgs e)
        {
            // Extract the symbol of the button that was clicked.
            Button clickedSymbol = (Button)sender;
            string rectangle = clickedSymbol.Content.ToString();
            string text = rectangle;

            var myDocument = editor.Document;
            string oldText;
            myDocument.GetText(TextGetOptions.None, out oldText);
            editor.Document.Selection.Text = text;

            Symbols_Insert.Flyout.Hide();
            editor.Focus(FocusState.Keyboard);
        }

        private async void DateInsertionAsync(object sender, RoutedEventArgs e)
        { // Create a ContentDialog
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Insert current date and time";

            // Create a ListView for the user to select the date format
            ListView listView = new ListView();
            listView.SelectionMode = ListViewSelectionMode.Single;

            // Create a list of date formats to display in the ListView
            List<string> dateFormats = new List<string>();
            dateFormats.Add(DateTime.Now.ToString("dd.M.yyyy"));
            dateFormats.Add(DateTime.Now.ToString("M.dd.yyyy"));
            dateFormats.Add(DateTime.Now.ToString("dd MMM yyyy"));
            dateFormats.Add(DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            dateFormats.Add(DateTime.Now.ToString("dd MMMM yyyy"));
            dateFormats.Add(DateTime.Now.ToString("hh:mm:ss tt"));
            dateFormats.Add(DateTime.Now.ToString("HH:mm:ss"));
            dateFormats.Add(DateTime.Now.ToString("dddd, dd MMMM yyyy, HH:mm:ss"));
            dateFormats.Add(DateTime.Now.ToString("dd MMMM yyyy, HH:mm:ss"));
            dateFormats.Add(DateTime.Now.ToString("MMM dd, yyyy"));

            // Set the ItemsSource of the ListView to the list of date formats
            listView.ItemsSource = dateFormats;

            // Set the content of the ContentDialog to the ListView
            dialog.Content = listView;

            // Make the insert button colored
            dialog.DefaultButton = ContentDialogButton.Primary;

            // Add an "Insert" button to the ContentDialog
            dialog.PrimaryButtonText = "OK";
            dialog.PrimaryButtonClick += (s, args) =>
            {
                string selectedFormat = listView.SelectedItem as string;
                string formattedDate = dateFormats[listView.SelectedIndex];
                editor.Document.Selection.Text = formattedDate;
            };

            // Add a "Cancel" button to the ContentDialog
            dialog.SecondaryButtonText = "Cancel";

            // Show the ContentDialog
            await dialog.ShowAsync();
        }

        #region Find and Replace

        private void RepAllBTN_Click(object Sender, RoutedEventArgs EvArgs)
        {
            if (ReplaceBox.Text == FindTextBox.Text)
            {
            }
            else if (ReplaceBox.Text.ToLower() == FindTextBox.Text.ToLower() && CaseSensBox.IsChecked == true && FullWordsBox.IsChecked == true)
            {

            }
            else if (ReplaceBox.Text.ToLower() == FindTextBox.Text.ToLower() && CaseSensBox.IsChecked != true)
            {

            }
            else
            {
                Replace(editor, FindTextBox.Text, ReplaceBox.Text, true, CaseSensBox.IsChecked, FullWordsBox.IsChecked, true, ReplaceBox);
            }
        }

        public static void Replace(RichEditBox RichEditBox, string TextToFind, string TextToReplace, bool ReplaceAll, bool? caseSensitive, bool? matchWords, bool? none, TextBox replaceBox)
        {
            int i = 0;

            if (ReplaceAll == true)
            {
                string Value = GetText(RichEditBox);
                if (!(string.IsNullOrWhiteSpace(Value) && string.IsNullOrWhiteSpace(TextToFind) && string.IsNullOrWhiteSpace(TextToReplace)))
                {
                    RichEditBox.Document.Selection.SetRange(0, GetText(RichEditBox).Length);
                    if (caseSensitive == true)
                    {
                        i = FindAsInt(TextToFind, FindOptions.Case, RichEditBox);
                        _ = RichEditBox.Document.Selection.FindText(TextToFind, GetText(RichEditBox).Length, FindOptions.Case);
                        if (i > j)
                        {
                            try
                            {
                                RichEditBox.Document.Selection.SetText(TextSetOptions.FormatRtf, replaceBox.Text);
                            }
                            catch (StackOverflowException)
                            {
                                return;
                            }
                            RichEditBox.Document.Selection.SetText(TextSetOptions.FormatRtf, replaceBox.Text);
                            _ = RichEditBox.Focus(FocusState.Pointer);
                            Replace(RichEditBox, TextToFind, TextToReplace, true, true, false, false, replaceBox);
                        }
                        else
                        {
                            j = 0;
                            return;
                        }
                    }
                    if (matchWords == true)
                    {
                        i = FindAsInt(TextToFind, FindOptions.Word, RichEditBox);
                        _ = RichEditBox.Document.Selection.FindText(TextToFind, GetText(RichEditBox).Length, FindOptions.Word);
                        if (i > j)
                        {
                            try
                            {
                                RichEditBox.Document.Selection.SetText(TextSetOptions.FormatRtf, replaceBox.Text);
                            }
                            catch (StackOverflowException)
                            {
                                return;
                            }
                            RichEditBox.Document.Selection.SetText(TextSetOptions.FormatRtf, replaceBox.Text);
                            _ = RichEditBox.Focus(FocusState.Pointer);
                            Replace(RichEditBox, TextToFind, TextToReplace, true, false, true, false, replaceBox);
                        }
                        else
                        {
                            j = 0;
                            return;
                        }
                    }
                    if (none == true)
                    {
                        i = FindAsInt(TextToFind, FindOptions.None, RichEditBox);
                        _ = RichEditBox.Document.Selection.FindText(TextToFind, GetText(RichEditBox).Length, FindOptions.None);
                        if (i > j)
                        {
                            try
                            {
                                RichEditBox.Document.Selection.SetText(TextSetOptions.FormatRtf, replaceBox.Text);
                            }
                            catch (StackOverflowException)
                            {
                                return;
                            }
                            _ = RichEditBox.Focus(FocusState.Pointer);
                            Replace(RichEditBox, TextToFind, TextToReplace, true, false, false, true, replaceBox);
                        }
                        else
                        {
                            j = 0;
                            return;
                        }
                    }
                    _ = RichEditBox.Focus(FocusState.Pointer);
                }
            }
            else
            {
                RichEditBox.Document.Selection.SetRange(0, GetText(RichEditBox).Length);
                if (caseSensitive == true)
                {
                    _ = RichEditBox.Document.Selection.FindText(TextToFind, GetText(RichEditBox).Length, FindOptions.Case);
                    RichEditBox.Document.Selection.SetText(TextSetOptions.FormatRtf, replaceBox.Text);
                    _ = RichEditBox.Focus(FocusState.Pointer);
                }
                if (matchWords == true)
                {
                    _ = RichEditBox.Document.Selection.FindText(TextToFind, GetText(RichEditBox).Length, FindOptions.Word);
                    RichEditBox.Document.Selection.SetText(TextSetOptions.FormatRtf, replaceBox.Text);
                    _ = RichEditBox.Focus(FocusState.Pointer);
                }
                if (none == true)
                {
                    _ = RichEditBox.Document.Selection.FindText(TextToFind, GetText(RichEditBox).Length, FindOptions.None);
                    RichEditBox.Document.Selection.SetText(TextSetOptions.FormatRtf, replaceBox.Text);
                    _ = RichEditBox.Focus(FocusState.Pointer);
                }
                _ = RichEditBox.Focus(FocusState.Pointer);
            }
        }

        public static int FindAsInt(string textToFind, FindOptions options, RichEditBox FindREB)
        {
            ITextRange searchRange = FindREB.Document.GetRange(0, 0);
            int x = 0;
            while (searchRange.FindText(textToFind, TextConstants.MaxUnitCount, options) > 0)
            {
                x++;
            }
            return x;
        }

        private static int j = 0;

        public static string GetText(RichEditBox RichEditor)
        {
            RichEditor.Document.GetText(TextGetOptions.FormatRtf, out string Text);
            ITextRange Range = RichEditor.Document.GetRange(0, Text.Length);
            Range.GetText(TextGetOptions.FormatRtf, out string Value);
            return Value;
        }

        private void FindBTN_Click(object Sender, RoutedEventArgs EvArgs)
        {
            editor.Document.Selection.SetRange(0, GetText(editor).Length);
            if (CaseSensBox.IsChecked == true)
            {
                _ = editor.Document.Selection.FindText(FindTextBox.Text, GetText(editor).Length, FindOptions.Case);
                _ = editor.Focus(FocusState.Pointer);
            }
            if (FullWordsBox.IsChecked == true)
            {
                _ = editor.Document.Selection.FindText(FindTextBox.Text, GetText(editor).Length, FindOptions.Word);
                _ = editor.Focus(FocusState.Pointer);
            }
            if (!CaseSensBox.IsChecked == true && !FullWordsBox.IsChecked == true)
            {
                _ = editor.Document.Selection.FindText(FindTextBox.Text, GetText(editor).Length, FindOptions.None);
                _ = editor.Focus(FocusState.Pointer);
            }
        }

        private void RepBTN_Click(object Sender, RoutedEventArgs EvArgs)
        {
            Replace(editor, FindTextBox.Text, ReplaceBox.Text, true, CaseSensBox.IsChecked, FullWordsBox.IsChecked, true, ReplaceBox);
        }

        private void CancelFindRepBTN_Click(object Sender, RoutedEventArgs EvArgs)
        {
            _ = editor.Focus(FocusState.Pointer);
        }

        #endregion Find and Replace

        public List<string> fonts
        {
            get
            {
                return CanvasTextFormat.GetSystemFontFamilies().OrderBy(f => f).ToList();
            }
        }

        public List<string> fontsizes = new List<string> {"8", "9", "10", "11", "12", "14", "16", "18", "20", "22", "24", "26", "28", "36", "48", "72"};

        private async void more_symbols(object sender, RoutedEventArgs e)
        {
            // Create a ContentDialog
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Insert symbol";

            // Create a ListView for the user to select the date format
            GridView listView = new GridView();
            listView.SelectionMode = ListViewSelectionMode.Single;

            // Create a list of date formats to display in the ListView
            List<string> symbols = new List<string>();
            symbols.Add("×");
            symbols.Add("÷");
            symbols.Add("←");
            symbols.Add("→");
            symbols.Add("°");
            symbols.Add("§");
            symbols.Add("µ");
            symbols.Add("π");
            symbols.Add("α");
            symbols.Add("β");
            symbols.Add("γ");
            symbols.Add("©️");
            symbols.Add("®️");
            symbols.Add("™️");
            symbols.Add("±");
            symbols.Add("℮");
            symbols.Add("≠");
            symbols.Add("≡");
            symbols.Add("≈");
            symbols.Add("≤");
            symbols.Add("≥");

            // Set the ItemsSource of the ListView to the list of date formats
            listView.ItemsSource = symbols;

            // Set the content of the ContentDialog to the ListView
            dialog.Content = listView;

            // Make the insert button colored
            dialog.DefaultButton = ContentDialogButton.Primary;

            // Add an "Insert" button to the ContentDialog
            dialog.PrimaryButtonText = "OK";
            dialog.PrimaryButtonClick += (s, args) =>
            {
                string selectedFormat = listView.SelectedItem as string;
                string formattedDate = symbols[listView.SelectedIndex];
                editor.Document.Selection.Text = formattedDate;
            };

            // Add a "Cancel" button to the ContentDialog
            dialog.SecondaryButtonText = "Cancel";

            // Show the ContentDialog
            await dialog.ShowAsync();
        }

        private void ComputeHash_Click(object sender, RoutedEventArgs e)
        {
            editor.TextDocument.GetText(TextGetOptions.NoHidden, out docText);
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Compute hashes";
            dialog.Content = new ComputeHash();
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Close;
            dialog.ShowAsync();
        }

        public async Task ShowUnsavedDialog2()
        {
            string fileName = AppTitle.Text.Replace(" - " + "UTE UWP", "");
            ContentDialog aboutDialog = new ContentDialog
            {
                Title = "Do you want to save changes to " + fileName + "?",
                Content = "There are unsaved changes, want to save them?",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save changes",
                SecondaryButtonText = "No",
                DefaultButton = ContentDialogButton.Primary
            };

            aboutDialog.CloseButtonClick += (s, e) => this._openDialog = false;

            ContentDialogResult result = await aboutDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                SaveFile(true);
            }
            else if (result == ContentDialogResult.Secondary)
            {
                editor.TextDocument.SetText(TextSetOptions.FormatRtf, "");
                AppTitle.Text = "Untitled" + " - " + "UTE UWP";
                fileNameWithPath = "";
            }
        }

        private void AlignJustifyButton_Click(object sender, RoutedEventArgs e)
        {
            var ST = editor.Document.Selection;
            if (ST != null)
            {
                var CF = ST.ParagraphFormat.Alignment;
                if (CF != ParagraphAlignment.Justify) CF = ParagraphAlignment.Justify;
                else CF = ParagraphAlignment.Left;
                ST.ParagraphFormat.Alignment = CF;
            }
        }

        private void FontBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (editor.Document.Selection != null)
            {
                editor.Document.Selection.CharacterFormat.Name = FontBox.SelectedValue.ToString();
            }
        }

        private void StrikethroughButton_Click(object sender, RoutedEventArgs e)
        {
            ITextSelection ST = editor.Document.Selection;
            if (!(ST == null))
            {
                FormatEffect CF = ST.CharacterFormat.Strikethrough;
                switch (CF)
                {
                    case FormatEffect.Off:
                        CF = FormatEffect.On;
                        STB.IsChecked = true;
                        break;
                    default:
                        CF = FormatEffect.Off;
                        STB.IsChecked = false;
                        break;
                }
                ST.CharacterFormat.Strikethrough = CF;
            }
        }

        private void Button_Click_32(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_37(object sender, RoutedEventArgs e)
        {
            Home.Visibility = Visibility.Collapsed;
            Insert.Visibility = Visibility.Collapsed;
            Developer.Visibility = Visibility.Collapsed;
            Comments.Visibility = Visibility.Visible;
            Layout.Visibility = Visibility.Collapsed;
            View.Visibility = Visibility.Collapsed;
            Help.Visibility = Visibility.Collapsed;
            EditButton.IsChecked = false;
            InsertButton.IsChecked = false;
            DeveloperButton.IsChecked = false;
            LayoutButton.IsChecked = false;
            ViewButton.IsChecked = false;
            CommentsButton.IsChecked = true;
            HelpButton.IsChecked = false;
        }

        private void Button_Click_33(object sender, RoutedEventArgs e)
        {
            Home.Visibility = Visibility.Visible;
            Insert.Visibility = Visibility.Collapsed;
            Developer.Visibility = Visibility.Collapsed;
            Comments.Visibility = Visibility.Collapsed;
            Layout.Visibility = Visibility.Collapsed;
            View.Visibility = Visibility.Collapsed;
            Help.Visibility = Visibility.Collapsed;
            EditButton.IsChecked = true;
            InsertButton.IsChecked = false;
            CommentsButton.IsChecked = false;
            DeveloperButton.IsChecked = false;
            LayoutButton.IsChecked = false;
            ViewButton.IsChecked = false;
            HelpButton.IsChecked = false;
        }

        private void Button_Click_34(object sender, RoutedEventArgs e)
        {
            Home.Visibility = Visibility.Collapsed;
            Insert.Visibility = Visibility.Visible;
            Developer.Visibility = Visibility.Collapsed;
            Comments.Visibility = Visibility.Collapsed;
            Layout.Visibility = Visibility.Collapsed;
            View.Visibility = Visibility.Collapsed;
            Help.Visibility = Visibility.Collapsed;
            EditButton.IsChecked = false;
            InsertButton.IsChecked = true;
            DeveloperButton.IsChecked = false;
            CommentsButton.IsChecked = false;
            LayoutButton.IsChecked = false;
            ViewButton.IsChecked = false;
            HelpButton.IsChecked = false;
        }

        private void Button_Click_35(object sender, RoutedEventArgs e)
        {
            Home.Visibility = Visibility.Collapsed;
            Insert.Visibility = Visibility.Collapsed;
            Developer.Visibility = Visibility.Collapsed;
            Comments.Visibility = Visibility.Collapsed;
            Layout.Visibility = Visibility.Collapsed;
            View.Visibility = Visibility.Collapsed;
            Help.Visibility = Visibility.Visible;
            EditButton.IsChecked = false;
            InsertButton.IsChecked = false;
            DeveloperButton.IsChecked = false;
            CommentsButton.IsChecked = false;
            LayoutButton.IsChecked = false;
            ViewButton.IsChecked = false;
            HelpButton.IsChecked = true;
        }

        #region Templates

        private void Template1_Click(object Sender, RoutedEventArgs EvArgs)
        {
            //Normal
            var ST = editor.Document.Selection;
            if (!(ST == null))
            {
                var CF = ST.CharacterFormat;
                CF.Bold = FormatEffect.Off;

                CF.Italic = FormatEffect.Off;
                CF.Name = "Segoe UI";
                FontBox.SelectedItem = "Segoe UI";

                CF.Outline = FormatEffect.Off;
                CF.Size = (float)V;
                FontSizeBox.Text = V.ToString();
                CF.Underline = UnderlineType.None;
                ST.CharacterFormat = CF;
            }
        }

        private void Template2_Click(object Sender, RoutedEventArgs EvArgs)
        {
            //Title
            var ST = editor.Document.Selection;
            if (!(ST == null))
            {
                var CF = ST.CharacterFormat;
                var PF = ST.ParagraphFormat;
                PF.Alignment = ParagraphAlignment.Center;
                CF.Bold = FormatEffect.Off;
                CF.Italic = FormatEffect.Off;
                CF.Name = "Segoe UI";
                FontBox.SelectedItem = "Segoe UI";

                CF.Outline = FormatEffect.Off;
                CF.Size = 28;
                FontSizeBox.Text = V1.ToString();
                CF.Underline = UnderlineType.None;
                ST.CharacterFormat = CF;
            }
        }

        private void Template3_Click(object Sender, RoutedEventArgs EvArgs)
        {
            //Title 2
            var ST = editor.Document.Selection;
            if (!(ST == null))
            {
                var CF = ST.CharacterFormat;
                var PF = ST.ParagraphFormat;
                PF.Alignment = ParagraphAlignment.Center;
                CF.Bold = FormatEffect.Off;

                CF.Italic = FormatEffect.Off;
                CF.Name = "Segoe UI";
                FontBox.SelectedItem = "Segoe UI";

                CF.Outline = FormatEffect.Off;
                CF.Size = 22;
                FontSizeBox.Text = 22.ToString();
                CF.Underline = UnderlineType.None;
                ST.CharacterFormat = CF;
            }
        }

        private void Template4_Click(object Sender, RoutedEventArgs EvArgs)
        {
            //Important
            var ST = editor.Document.Selection;
            if (!(ST == null))
            {
                var CF = ST.CharacterFormat;
                CF.Bold = FormatEffect.On;

                CF.Italic = FormatEffect.On;
                CF.Name = "Segoe UI";
                FontBox.SelectedItem = "Segoe UI";

                CF.Outline = FormatEffect.Off;
                CF.Size = 16;
                FontSizeBox.Text = 16.ToString();
                CF.Underline = UnderlineType.None;
                ST.CharacterFormat = CF;
            }
        }

        private void Template5_Click(object Sender, RoutedEventArgs EvArgs)
        {
            //Header
            var ST = editor.Document.Selection;
            if (!(ST == null))
            {
                var CF = ST.CharacterFormat;
                CF.Bold = FormatEffect.Off;

                CF.Italic = FormatEffect.Off;
                CF.Name = "Segoe UI";
                FontBox.SelectedItem = "Segoe UI";

                CF.Outline = FormatEffect.Off;
                CF.Size = 14;
                FontSizeBox.Text = 14.ToString();
                CF.Underline = UnderlineType.None;
                ST.CharacterFormat = CF;
            }
        }

        private void Template6_Click(object Sender, RoutedEventArgs EvArgs)
        {
            //Medium
            var ST = editor.Document.Selection;
            if (!(ST == null))
            {
                var CF = ST.CharacterFormat;
                CF.Bold = FormatEffect.Off;

                CF.Italic = FormatEffect.Off;
                CF.Name = "Segoe UI";
                FontBox.SelectedItem = "Segoe UI";

                CF.Outline = FormatEffect.Off;
                CF.Size = 18;
                FontSizeBox.Text = 18.ToString();
                CF.Underline = UnderlineType.None;
                ST.CharacterFormat = CF;
            }
        }

        private void Template7_Click(object Sender, RoutedEventArgs EvArgs)
        {
            //Subtitle
            var ST = editor.Document.Selection;
            if (!(ST == null))
            {
                var CF = ST.CharacterFormat;
                CF.Bold = FormatEffect.Off;

                CF.Italic = FormatEffect.Off;
                CF.Name = "Segoe UI";
                FontBox.SelectedItem = "Segoe UI";

                CF.Outline = FormatEffect.Off;
                CF.Size = 20;
                FontSizeBox.Text = 20.ToString();
                CF.Underline = UnderlineType.None;
                ST.CharacterFormat = CF;
            }
        }

        private void Template8_Click(object Sender, RoutedEventArgs EvArgs)
        {
            //Strong
            var ST = editor.Document.Selection;
            if (!(ST == null))
            {
                var CF = ST.CharacterFormat;
                CF.Bold = FormatEffect.On;

                CF.Italic = FormatEffect.Off;
                CF.Name = "Segoe UI";
                FontBox.SelectedItem = "Segoe UI";

                CF.Outline = FormatEffect.Off;
                CF.Size = 18;
                FontSizeBox.Text = 18.ToString();
                CF.Underline = UnderlineType.None;
                ST.CharacterFormat = CF;
            }
        }

        private void Template9_Click(object Sender, RoutedEventArgs EvArgs)
        {
            //Content
            var ST = editor.Document.Selection;
            if (!(ST == null))
            {
                var CF = ST.CharacterFormat;
                CF.Bold = FormatEffect.Off;

                CF.Italic = FormatEffect.Off;
                CF.Name = "Segoe UI";
                FontBox.SelectedItem = "Segoe UI";

                CF.Outline = FormatEffect.Off;
                CF.Size = 16;
                FontSizeBox.Text = 16.ToString();
                CF.Underline = UnderlineType.None;
                ST.CharacterFormat = CF;
            }
        }

        private void Template10_Click(object Sender, RoutedEventArgs EvArgs)
        {
            //Finished
            var ST = editor.Document.Selection;
            if (!(ST == null))
            {
                var CF = ST.CharacterFormat;
                CF.Bold = FormatEffect.Off;

                CF.Italic = FormatEffect.On;
                CF.Name = "Segoe UI";
                FontBox.SelectedItem = "Segoe UI";

                CF.Outline = FormatEffect.Off;
                CF.Size = 14;
                FontSizeBox.Text = 14.ToString();
                CF.Underline = UnderlineType.None;
                ST.CharacterFormat = CF;
            }
        }

        private void Template11_Click(object Sender, RoutedEventArgs EvArgs)
        {
            //Unfinished
            var ST = editor.Document.Selection;
            if (!(ST == null))
            {
                var CF = ST.CharacterFormat;
                CF.Bold = FormatEffect.On;

                CF.Italic = FormatEffect.Off;
                CF.Name = "Segoe UI";
                FontBox.SelectedItem = "Segoe UI";

                CF.Outline = FormatEffect.Off;
                CF.Size = 14;
                FontSizeBox.Text = 14.ToString();
                CF.Underline = UnderlineType.None;
                ST.CharacterFormat = CF;
            }
        }

        private void Template12_Click(object Sender, RoutedEventArgs EvArgs)
        {
            //Strong header
            var ST = editor.Document.Selection;
            if (!(ST == null))
            {
                var CF = ST.CharacterFormat;
                CF.Bold = FormatEffect.Off;
                CF.Italic = FormatEffect.On;
                CF.Name = "Segoe UI";
                FontBox.SelectedItem = "Segoe UI";

                CF.Outline = FormatEffect.Off;
                CF.Size = 18;
                CF.ForegroundColor = Colors.DimGray;
                FontSizeBox.Text = 18.ToString();
                CF.Underline = UnderlineType.None;
                ST.CharacterFormat = CF;
            }
        }


        #endregion Templates

        private void AddLinkButton2_Click(object sender, RoutedEventArgs e)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent("Windows.UI.Xaml.FrameworkElement", "AllowFocusOnInteraction"))
                hyperlinkText2.AllowFocusOnInteraction = true;
            editor.Document.Selection.Link = $"\"{hyperlinkText2.Text}\"";
            editor.Document.Selection.CharacterFormat.ForegroundColor = (Windows.UI.Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), "#6194c7");
            LinkInsert.Flyout.Hide();
        }

        private void MenuFlyoutItem_Click_1(object Sender, RoutedEventArgs EvArgs)
        {
            //Configure underline
            var MFItem = (MenuFlyoutItem)Sender;
            ITextSelection ST = editor.Document.Selection;
            if (!(ST == null))
            {
                MarkerType CF = ST.ParagraphFormat.ListType;
                if (MFItem.Text == "None") CF = MarkerType.None;
                if (MFItem.Text == "Bullet") CF = MarkerType.Bullet;
                if (MFItem.Text == "Numbered") CF = MarkerType.CircledNumber;
                if (MFItem.Text == "Lowercase") CF = MarkerType.LowercaseEnglishLetter;
                if (MFItem.Text == "Uppercase") CF = MarkerType.UppercaseEnglishLetter;
                if (MFItem.Text == "Roman numerals") CF = MarkerType.UppercaseRoman;
                ST.ParagraphFormat.ListType = CF;
                editor.ContextFlyout.Hide();
            }
        }

        private void NullBackgroundButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            //Configure underline
            var MFItem = (MenuFlyoutItem)sender;
            ITextSelection ST = editor.Document.Selection;
            if (ST != null)
            {
                UnderlineType CF = ST.CharacterFormat.Underline;
                if (MFItem.Text == "None") CF = UnderlineType.None;
                if (MFItem.Text == "Single") CF = UnderlineType.Single;
                if (MFItem.Text == "Dash") CF = UnderlineType.Dash;
                if (MFItem.Text == "Dotted") CF = UnderlineType.Dotted;
                if (MFItem.Text == "Double") CF = UnderlineType.Double;
                if (MFItem.Text == "Thick") CF = UnderlineType.Thick;
                if (MFItem.Text == "Wave") CF = UnderlineType.Wave;
                ST.CharacterFormat.Underline = CF;
                editor.ContextFlyout.Hide();
            }
        }

        private void Button_Click_36(object sender, RoutedEventArgs e)
        {
            Home.Visibility = Visibility.Collapsed;
            Insert.Visibility = Visibility.Collapsed;
            Developer.Visibility = Visibility.Visible;
            Comments.Visibility = Visibility.Collapsed;
            Layout.Visibility = Visibility.Collapsed;
            View.Visibility = Visibility.Collapsed;
            Help.Visibility = Visibility.Collapsed;
            EditButton.IsChecked = false;
            InsertButton.IsChecked = false;
            DeveloperButton.IsChecked = true;
            CommentsButton.IsChecked = false;
            LayoutButton.IsChecked = false;
            ViewButton.IsChecked = false;
            HelpButton.IsChecked = false;
        }

        private async void FirstRunClick(object sender, RoutedEventArgs e)
        {
            FirstRunDialog firstrun = new FirstRunDialog();
            await firstrun.ShowAsync();
        }

        public async void ChangelogClick(object sender, RoutedEventArgs e)
        {
            WhatsNewDialog whatsNew = new WhatsNewDialog();
            await whatsNew.ShowAsync();
        }

        private void HomeMenuButton_Click(object sender, RoutedEventArgs e)
        {
            HomeNavView.SelectedItem = HomeItem;
            HomeMenuContentFrame.Content = HomePage;
            HomeMenu.Visibility = Visibility.Visible;
            BlankDocumentButton.Visibility = Visibility.Visible;
            NewDocText.Visibility = Visibility.Collapsed;
        }

        private void NavigationView_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            HomeMenu.Visibility = Visibility.Collapsed;
        }

        private void NewDocumentExpander_Expanding(Microsoft.UI.Xaml.Controls.Expander sender, Microsoft.UI.Xaml.Controls.ExpanderExpandingEventArgs args)
        {
            BlankDocumentButton.Visibility = Visibility.Collapsed;
            NewDocText.Visibility = Visibility.Visible;
        }

        private void NewDocumentExpander_Collapsed(Microsoft.UI.Xaml.Controls.Expander sender, Microsoft.UI.Xaml.Controls.ExpanderCollapsedEventArgs args)
        {
            BlankDocumentButton.Visibility = Visibility.Visible;
            NewDocText.Visibility = Visibility.Collapsed;
        }

        private void HomeNavView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (Microsoft.UI.Xaml.Controls.NavigationViewItem)args.SelectedItem;
            string selectedItemTag = (string)selectedItem.Tag;
            if (args.IsSettingsSelected)
            {
                HomeMenuContentFrame.Visibility = Visibility.Visible;
                NewPage.Visibility = Visibility.Collapsed;
                HomeMenuContentFrame.Navigate(typeof(SettingsPage));
            }
            if (selectedItemTag == "Home")
            {
                HomeMenuContentFrame.Visibility = Visibility.Visible;
                NewPage.Visibility = Visibility.Collapsed;
                HomeMenuContentFrame.Content = HomePage;
            } else if (selectedItemTag == "Help") {
                HomeMenuContentFrame.Visibility = Visibility.Visible;
                NewPage.Visibility = Visibility.Collapsed;
                HomeMenuContentFrame.Navigate(typeof(HelpPage));
            } else if (selectedItemTag == "New") {
                HomeMenuContentFrame.Visibility = Visibility.Collapsed;
                NewPage.Visibility = Visibility.Visible;
            }
        }

        private void CreateBlankDocument(object sender, RoutedEventArgs e)
        {
            editor.Document.SetText(TextSetOptions.None, "");
        }

        private async void CreateImageArticleDocument(object sender, RoutedEventArgs e)
        {
            var template = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Templates/ImageArticleTemplate.rtf"));
            var stream = await template.OpenAsync(FileAccessMode.Read);
            editor.Document.LoadFromStream(TextSetOptions.FormatRtf, stream);
        }

        private async void CreateCalendarDocument(object sender, RoutedEventArgs e)
        {
            var template = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Templates/CalendarTemplate.rtf"));
            var stream = await template.OpenAsync(FileAccessMode.Read);
            editor.Document.LoadFromStream(TextSetOptions.FormatRtf, stream);
        }

        private async void CreateSongLyricsDocument(object sender, RoutedEventArgs e)
        {
            var template = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Templates/SongLyricsTemplate.rtf"));
            var stream = await template.OpenAsync(FileAccessMode.Read);
            editor.Document.LoadFromStream(TextSetOptions.FormatRtf, stream);
        }

        private void OnKeyboardAcceleratorInvoked(Windows.UI.Xaml.Input.KeyboardAccelerator sender, Windows.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            ITextSelection selectedText = editor.Document.Selection;
            switch (sender.Key)
            {
                case Windows.System.VirtualKey.B:
                    if (selectedText != null)
                    {
                        ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                        charFormatting.Bold = FormatEffect.Toggle;
                        selectedText.CharacterFormat = charFormatting;
                    }
                    BB.IsChecked = editor.Document.Selection.CharacterFormat.Bold == FormatEffect.On;
                    args.Handled = true;
                    break;
                case Windows.System.VirtualKey.I:
                    if (selectedText != null)
                    {
                        ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                        charFormatting.Italic = FormatEffect.Toggle;
                        selectedText.CharacterFormat = charFormatting;
                    }
                    IB.IsChecked = editor.Document.Selection.CharacterFormat.Italic == FormatEffect.On;
                    args.Handled = true;
                    break;
                case Windows.System.VirtualKey.U:
                    if (selectedText != null)
                    {
                        UnderlineType characterFormat = selectedText.CharacterFormat.Underline;
                        characterFormat = UnderlineType.Single;
                        selectedText.CharacterFormat.Underline = characterFormat;
                    }
                    args.Handled = true;
                    break;
                case Windows.System.VirtualKey.S:
                    SaveFile(true);
                    args.Handled = true;
                    break;
            }
        }

        private void OpenLayoutTab(object sender, RoutedEventArgs e)
        {
            Home.Visibility = Visibility.Collapsed;
            Insert.Visibility = Visibility.Collapsed;
            Developer.Visibility = Visibility.Collapsed;
            Comments.Visibility = Visibility.Collapsed;
            Layout.Visibility = Visibility.Visible;
            View.Visibility = Visibility.Collapsed;
            Help.Visibility = Visibility.Collapsed;
            EditButton.IsChecked = false;
            InsertButton.IsChecked = false;
            CommentsButton.IsChecked = false;
            DeveloperButton.IsChecked = false;
            LayoutButton.IsChecked = true;
            ViewButton.IsChecked = false;
            HelpButton.IsChecked = false;
        }

        private void OpenViewTab(object sender, RoutedEventArgs e)
        {
            Home.Visibility = Visibility.Collapsed;
            Insert.Visibility = Visibility.Collapsed;
            Developer.Visibility = Visibility.Collapsed;
            Comments.Visibility = Visibility.Collapsed;
            Layout.Visibility = Visibility.Collapsed;
            View.Visibility = Visibility.Visible;
            Help.Visibility = Visibility.Collapsed;
            EditButton.IsChecked = false;
            InsertButton.IsChecked = false;
            CommentsButton.IsChecked = false;
            DeveloperButton.IsChecked = false;
            LayoutButton.IsChecked = false;
            ViewButton.IsChecked = true;
            HelpButton.IsChecked = false;
        }

        private void A4Page(object sender, RoutedEventArgs e)
        {
            editor.Width = 595;
        }

        private void A3Page(object sender, RoutedEventArgs e)
        {
            editor.Width = 842;
        }

        private void LetterPage(object sender, RoutedEventArgs e)
        {
            editor.Width = 612;
        }

        private void PrintPage(object sender, RoutedEventArgs e)
        {
            editor.Width = 595;
            sizeA4.IsEnabled = true;
            sizeA3.IsEnabled = true;
            sizeLetter.IsEnabled = true;
        }

        private void FullWidthPage(object sender, RoutedEventArgs e)
        {
            editor.Width = EditorGrid.Width;
            sizeA4.IsEnabled = false;
            sizeA3.IsEnabled = false;
            sizeLetter.IsEnabled = false;
        }
    }
}
