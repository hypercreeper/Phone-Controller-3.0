using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml.Controls;

using Phone_Controller_3._0.ViewModels;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT;

namespace Phone_Controller_3._0.Views;

public sealed partial class PackageManagerPage : Page
{
    public PackageManagerViewModel ViewModel
    {
        get;
    }

    public PackageManagerPage()
    {
        ViewModel = App.GetService<PackageManagerViewModel>();
        InitializeComponent();
    }

    private void UninstallBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ((Button)sender).IsEnabled = false;
        UninstallProgressBar.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        var startinfo = new ProcessStartInfo() { FileName = Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        startinfo.Arguments = "-s " + ConnectionsPage.defaultDevice + " uninstall " + UninstallAppComboBox.Text;
        new Thread(delegate ()
        {
            var process = Process.Start(startinfo);
            process.WaitForExit();
            DispatcherQueue.TryEnqueue(() =>
            {
                PackageManagerLog.Text += process.StandardOutput.ReadToEnd();
                PackageManagerLog.Text += process.StandardError.ReadToEnd();
            });
            if (process.ExitCode != 0) 
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var contentdi = new ContentDialog();
                    contentdi.Title = "Error Occurred - Uninstall";
                    contentdi.XamlRoot = this.XamlRoot;
                    contentdi.Content = process.StandardError.ReadToEnd();
                    contentdi.PrimaryButtonText = "OK";
                    contentdi.ShowAsync();
                });
            }
            else
            {
                string line = process.StandardOutput.ReadToEnd();
                DispatcherQueue.TryEnqueue(() =>
                {
                    ((Button)sender).IsEnabled = true;
                    UninstallProgressBar.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    var contentdi = new ContentDialog();
                    contentdi.Title = "Uninstall App";
                    contentdi.XamlRoot = this.XamlRoot;
                    contentdi.Content = UninstallAppComboBox.Text + " Uninstalled Successfuly";
                    contentdi.PrimaryButtonText = "OK";
                    contentdi.ShowAsync();
                    UninstallAppComboBox.Text = "";
                });
            }
        }).Start();
    }
    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        // Since selecting an item will also change the text,
        // only listen to changes caused by user entering text.
        DeviceControlPage.ADBProcessInfo.Arguments = "shell cmd package list packages --user 0";
        var process = Process.Start(DeviceControlPage.ADBProcessInfo);
        //process.WaitForExit();
        string line = process.StandardOutput.ReadToEnd();
        if (process.ExitCode != 0)
        {
            line = process.StandardError.ReadToEnd();
        }

        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var suitableItems = new List<string>();
            var splitText = sender.Text.ToLower().Split(" ");
            foreach (var package in line.Replace("\r", "").Replace("package:", "").Split("\n"))
            {
                var found = splitText.All((key) =>
                {
                    return package.ToLower().Contains(key);
                });
                if (found)
                {
                    suitableItems.Add(package);
                }
            }
            if (suitableItems.Count == 0)
            {
                suitableItems.Add("No results found");
            }
            sender.ItemsSource = suitableItems;
        }
    }
    private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        UninstallAppComboBox.Text = args.SelectedItem.ToString();
    }

    [ComImport, System.Runtime.InteropServices.Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInitializeWithWindow
    {
        void Initialize([In] IntPtr hwnd);
    }

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto, PreserveSig = true, SetLastError = false)]
    public static extern IntPtr GetActiveWindow();


    private async void APKInstallBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var filepicker = new FileOpenPicker();
        var initializeWithWindowWrapper = filepicker.As<IInitializeWithWindow>();
        initializeWithWindowWrapper.Initialize(GetActiveWindow());
        filepicker.CommitButtonText = "Install";
        filepicker.SuggestedStartLocation = PickerLocationId.Downloads;
        filepicker.FileTypeFilter.Clear();
        filepicker.FileTypeFilter.Add(".apk");
        var file = await filepicker.PickSingleFileAsync();
        if (file != null)
        {
            ((Button)sender).IsEnabled = false;
            InstallAPKProgressBar.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            var startinfo = new ProcessStartInfo() { FileName = Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            if ((bool)APKInstallOverrideCheckbox.IsChecked)
            {
                startinfo.Arguments = "-s " + ConnectionsPage.defaultDevice + " install -r '" + file.Path + "'";
            }
            else
            {
                startinfo.Arguments = "-s " + ConnectionsPage.defaultDevice + " install '" + file.Path + "'";
            }
            new Thread(delegate ()
            {
                var process = Process.Start(startinfo);
                process.WaitForExit();
                DispatcherQueue.TryEnqueue(() =>
                {
                    PackageManagerLog.Text += process.StandardOutput.ReadToEnd();
                    PackageManagerLog.Text += process.StandardError.ReadToEnd();
                });
                if (process.ExitCode != 0)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        var contentdi = new ContentDialog();
                        contentdi.Title = "Error Occurred - Install";
                        contentdi.XamlRoot = this.XamlRoot;
                        contentdi.Content = process.StandardError.ReadToEnd();
                        contentdi.PrimaryButtonText = "OK";
                        contentdi.ShowAsync();
                    });
                }
                else
                {
                    string line = process.StandardOutput.ReadToEnd();
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        ((Button)sender).IsEnabled = true;
                        InstallAPKProgressBar.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        var contentdi = new ContentDialog();
                        contentdi.Title = "Install App";
                        contentdi.XamlRoot = this.XamlRoot;
                        contentdi.Content = file.Name + " Installed Successfuly";
                        contentdi.PrimaryButtonText = "OK";
                        contentdi.ShowAsync();
                    });
                }
                DispatcherQueue.TryEnqueue(() =>
                {
                    ((Button)sender).IsEnabled = true;
                    InstallAPKProgressBar.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                });
            }).Start();
        }
    }
    private StorageFile sideloadFile;
    private async void SideloadLoadFileButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var filepicker = new FileOpenPicker();
        var initializeWithWindowWrapper = filepicker.As<IInitializeWithWindow>();
        initializeWithWindowWrapper.Initialize(GetActiveWindow());
        filepicker.CommitButtonText = "Install";
        filepicker.SuggestedStartLocation = PickerLocationId.Downloads;
        filepicker.FileTypeFilter.Clear();
        filepicker.FileTypeFilter.Add(".zip");
        sideloadFile = await filepicker.PickSingleFileAsync();
        SideloadFileLabel.Text = sideloadFile.Name;
        if (sideloadFile != null)
        {
            SideloadButton.IsEnabled = true;
        }
        else
        {
            SideloadButton.IsEnabled = false;
        }
    }

    private void SideloadButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sideloadFile != null)
        {
            ((Button)sender).IsEnabled = false;
            SideloadProgressBar.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            var startinfo = new ProcessStartInfo() { FileName = Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            startinfo.Arguments = "-s " + ConnectionsPage.defaultDevice + " sideload '" + sideloadFile.Path + "'";
            new Thread(delegate ()
            {
                var process = Process.Start(startinfo);
                process.WaitForExit();
                DispatcherQueue.TryEnqueue(() =>
                {
                    PackageManagerLog.Text += process.StandardOutput.ReadToEnd();
                    PackageManagerLog.Text += process.StandardError.ReadToEnd();
                });
                if (process.ExitCode != 0)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        var contentdi = new ContentDialog();
                        contentdi.Title = "Error Occurred - Sideload";
                        contentdi.XamlRoot = this.XamlRoot;
                        contentdi.Content = process.StandardError.ReadToEnd();
                        contentdi.PrimaryButtonText = "OK";
                        contentdi.ShowAsync();
                    });
                }
                else
                {
                    string line = process.StandardOutput.ReadToEnd();
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        ((Button)sender).IsEnabled = true;
                        InstallAPKProgressBar.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        var contentdi = new ContentDialog();
                        contentdi.Title = "Sideload Zip";
                        contentdi.XamlRoot = this.XamlRoot;
                        contentdi.Content = sideloadFile.Name + " Sideloaded Successfuly";
                        contentdi.PrimaryButtonText = "OK";
                        contentdi.ShowAsync();
                    });
                }
                DispatcherQueue.TryEnqueue(() =>
                {
                    SideloadProgressBar.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                });
            }).Start();
        }
    }
}