using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Phone_Controller_3._0.Helpers;
using Phone_Controller_3._0.ViewModels;

namespace Phone_Controller_3._0.Views;

public sealed partial class DeviceControlPage : Page
{
    public DeviceControlViewModel ViewModel
    {
        get;
    }
    static public ProcessStartInfo ADBProcessInfo = new ProcessStartInfo() { FileName=Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools/adb", CreateNoWindow=true, UseShellExecute=false, RedirectStandardOutput=true, RedirectStandardError=true };

    void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        adbDownloadBar.Value = e.ProgressPercentage;
        adbDownloadStatus.Text = "Downloading... " + e.ProgressPercentage.ToString() + "%";
    }
    void wc_DownloadFinished(object sender, EventArgs e)
    {
        adbDownloadStatus.Text = "Extracting platform-tools...";
        adbDownloadBar.IsIndeterminate = true;
        System.IO.Compression.ZipFile.ExtractToDirectory(Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/temp/platform-tools_r34.0.5-windows.zip", Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/temp");
        adbDownloadStatus.Text = "Preparing platform-tools...";
        Directory.Move(Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/temp/platform-tools", Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools");
        adbDownloadStatus.Text = "Cleaning Up...";
        Directory.Delete(Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/temp", true);
        adbDownloadBar.Visibility = Visibility.Collapsed;
        adbDownloadStatus.Visibility = Visibility.Collapsed;
        alertBox.Visibility = Visibility.Collapsed;
    }
    private void DownloadADB()
    {
        adbDownloadBar.Visibility = Visibility.Visible;
        adbDownloadStatus.Visibility = Visibility.Visible;
        adbDownloadStatus.Text = "Deleting platform-tools Folder...";
        adbDownloadBar.IsIndeterminate = true;
        if (Directory.Exists(Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools"))
            Directory.Delete(Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools", true);
        adbDownloadStatus.Text = "Creating temp Folder...";
        var tempDir = Directory.CreateDirectory(Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/temp");
        adbDownloadStatus.Text = "Starting Download...";
        adbDownloadBar.IsIndeterminate = false;
        using (WebClient wc = new WebClient())
        {
            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
            wc.DownloadFileCompleted += wc_DownloadFinished;
            wc.DownloadFileAsync(
                // Param1 = Link of file
                new System.Uri("https://dl.google.com/android/repository/platform-tools_r34.0.5-windows.zip"),
                // Param2 = Path to save
                Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/temp/platform-tools_r34.0.5-windows.zip"
            );
        }
    }

    public DeviceControlPage()
    {
        ViewModel = App.GetService<DeviceControlViewModel>();
        InitializeComponent();
    }
    public void processADBCommand(string command, Button senderBtn, bool hasProgressBar, bool hasLogBox, bool noPopup, TextBox LogBox = null, XamlRoot xamlroot = null, string task = "", string title = "", string successMessage = "", ProgressBar progressBar = null)
    {
        senderBtn.IsEnabled = false;
        if(hasProgressBar)
            progressBar.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        var startinfo = new ProcessStartInfo() { FileName = Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        startinfo.Arguments = "-s " + ConnectionsPage.defaultDevice + " " + command;


        new Thread(delegate ()
        {
            var process = Process.Start(startinfo);
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            if (hasLogBox)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    LogBox.Text += output;
                    LogBox.Text += error;
                });
            }
            if (process.ExitCode != 0)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var contentdi = new ContentDialog();
                    contentdi.Title = "Error Occurred - " + task;
                    contentdi.XamlRoot = xamlroot;
                    contentdi.Content = error;
                    contentdi.PrimaryButtonText = "OK";
                    contentdi.ShowAsync();
                });
            }
            else
            {
                if (!noPopup)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        senderBtn.IsEnabled = true;
                        if (hasProgressBar)
                            progressBar.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        var contentdi = new ContentDialog();
                        contentdi.Title = (title != ""? title:"Success");
                        contentdi.XamlRoot = xamlroot;
                        contentdi.Content = (successMessage != ""? successMessage:output + "\n\n" + error);
                        contentdi.PrimaryButtonText = "OK";
                        contentdi.ShowAsync();
                    });
                }
            }
            DispatcherQueue.TryEnqueue(() =>
            {
                senderBtn.IsEnabled = true;
                if(hasProgressBar)
                    progressBar.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            });
        }).Start();
    }
    private void MoveUpButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent 19", (Button)sender, false, false, true, xamlroot:this.XamlRoot);
    }
    private void MoveDownButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent 20", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void MoveLeftButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent 21", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void MoveRightButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent 22", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void EnterButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent 23", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void RecentsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent KEYCODE_APP_SWITCH", (Button)sender, false, false, true, xamlroot:this.XamlRoot);
    }
    private void HomeButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent 3", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent 4", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void VolUpButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent 24", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void VolDownButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent 25", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void MuteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent 164", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void NextButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent 87", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void PlayPauseButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent 85", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void PreviousButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent 88", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void PowerButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent 26", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void CameraButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("shell input keyevent 27", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void RebootButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("reboot", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void RebootRecoveryButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("reboot recovery", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void RebootBootloaderButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("reboot bootloader", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }
    private void RebootSideloadButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        processADBCommand("reboot sideload", (Button)sender, false, false, true, xamlroot: this.XamlRoot);
    }

    private void SendPasswordButton_Click(object sender, RoutedEventArgs e)
    {
        processADBCommand("shell input text '" + passwordBox.Password + "'", (Button)sender, false, false, false, xamlroot: this.XamlRoot);
    }
    private void SendKeyboardButton_Click(object sender, RoutedEventArgs e)
    {
        processADBCommand("shell input text '" + keyboardBox.Text + "'", (Button)sender, false, false, false, xamlroot: this.XamlRoot);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        var startinfo = new ProcessStartInfo() { FileName = Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        startinfo.Arguments = "--version";
        new Thread(delegate ()
        {
            try
            {
                var process = Process.Start(startinfo);
                process.WaitForExit();
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                if (process.ExitCode != 0)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        alertWarningLabel.Text = "ADB is not installed, would you like to download it now?";
                        alertBox.Visibility = Visibility.Visible;
                        DownloadADBBtn.Click += delegate
                        {
                            DownloadADB();
                        };
                    });
                }
                else
                {
                    new Thread(delegate ()
                    {
                        var abort = false;
                        while (!abort)
                        {
                            if (ConnectionsPage.defaultDevice == "")
                            {
                                var startinfo = new ProcessStartInfo() { FileName = Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
                                startinfo.Arguments = "devices";
                                var thread = new Thread(delegate ()
                                {
                                    var process = Process.Start(startinfo);
                                    process.WaitForExit();
                                    var output = process.StandardOutput.ReadToEnd();
                                    var error = process.StandardError.ReadToEnd();
                                    if (process.ExitCode == 0)
                                    {
                                        DispatcherQueue.TryEnqueue(() =>
                                        {
                                            output = output.Replace("\r", "");
                                            output = output.Replace("List of devices attached", "");
                                            output = output.Replace("* daemon not running; starting now at tcp:5037\n* daemon started successfully", "");
                                            output = output.Trim();
                                            var devices = output.Split("\n");
                                            if (devices.Count() > 0 && devices[0] != "")
                                            {
                                                var device = devices[0].Replace("device", "").Trim();
                                                ConnectionsPage.defaultDevice = device;
                                                ShellPage.appTitleBar.Text = " - Connected to " + device;
                                                ShellPage.appTitleBar.Foreground = new SolidColorBrush(Colors.LimeGreen);
                                                ConnectionsPage.defaultDevice = device;
                                            }
                                            else
                                            {
                                                ShellPage.appTitleBar.Text = " - Not Connected";
                                                ShellPage.appTitleBar.Foreground = new SolidColorBrush(Colors.Red);
                                            }
                                        });
                                    }
                                    else
                                    {
                                        DispatcherQueue.TryEnqueue(() =>
                                        {
                                            var dialog = new ContentDialog();
                                            dialog.XamlRoot = this.XamlRoot;
                                            dialog.Title = "ADB Devices - Error";
                                            dialog.Content = error;
                                            dialog.PrimaryButtonText = "OK";
                                            dialog.ShowAsync();
                                        });
                                    }
                                });
                                thread.Start();
                            }
                            else
                            {
                                var startinfo = new ProcessStartInfo() { FileName = Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
                                startinfo.Arguments = "devices";
                                new Thread(delegate ()
                                {
                                    var process = Process.Start(startinfo);
                                    process.WaitForExit();
                                    var output = process.StandardOutput.ReadToEnd();
                                    var error = process.StandardError.ReadToEnd();
                                    if (process.ExitCode == 0)
                                    {
                                        DispatcherQueue.TryEnqueue(() =>
                                        {
                                            output = output.Replace("\r", "");
                                            output = output.Replace("List of devices attached", "");
                                            output = output.Replace("* daemon not running; starting now at tcp:5037\n* daemon started successfully", "");
                                            output = output.Trim();
                                            var devices = output.Split("\n");
                                            if (devices[0] == "")
                                            {
                                                ShellPage.appTitleBar.Text = " - Not Connected";
                                                ShellPage.appTitleBar.Foreground = new SolidColorBrush(Colors.Red);
                                            }
                                            else
                                            {
                                                ShellPage.appTitleBar.Text = " - Connected to " + ConnectionsPage.defaultDevice;
                                                ShellPage.appTitleBar.Foreground = new SolidColorBrush(Colors.LimeGreen);
                                            }
                                        });
                                    }
                                    else
                                    {
                                        DispatcherQueue.TryEnqueue(() =>
                                        {
                                            var dialog = new ContentDialog();
                                            dialog.XamlRoot = this.XamlRoot;
                                            dialog.Title = "ADB Devices - Error";
                                            dialog.Content = error;
                                            dialog.PrimaryButtonText = "OK";
                                            dialog.ShowAsync();
                                        });
                                    }
                                }).Start();
                            }
                            Thread.Sleep(2000);
                        }
                    }).Start();
                }
            }
            catch
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    alertWarningLabel.Text = "ADB is not installed, would you like to download it now?";
                    alertBox.Visibility = Visibility.Visible;
                    DownloadADBBtn.Click += delegate
                    {
                        DownloadADB();
                    };
                });
            }
        }).Start();
    }
}
