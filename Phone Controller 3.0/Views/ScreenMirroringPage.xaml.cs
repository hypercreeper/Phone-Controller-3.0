using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Phone_Controller_3._0.ViewModels;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Storage;
using WinRT.Interop;
using System.Net;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System;

namespace Phone_Controller_3._0.Views;

public sealed partial class ScreenMirroringPage : Page
{
    public ScreenMirroringViewModel ViewModel
    {
        get;
    }
    private string scrcpypath = "";
    private async void checkForScrcpy()
    {
        ProcessStartInfo scrcpyStartInfo = new ProcessStartInfo();
        scrcpyStartInfo.FileName = scrcpypath;
        scrcpyStartInfo.Arguments = "-v";
        scrcpyStartInfo.CreateNoWindow = true;
        try
        {
            var p = Process.Start(scrcpyStartInfo);
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                startScrcpyButton.IsEnabled = false;
                stopScrcpyButton.IsEnabled = false;
                scrcpyStatus.Title = "scrcpy.exe Not Found";
                scrcpyStatus.Severity = InfoBarSeverity.Error;
                scrcpyStatus.Message = "scrcpy.exe was not found, do you want to download it?\n" + scrcpypath;
                var downloadBtn = new Button() { Content = "Download" };
                downloadBtn.Click += DownloadSCRCPYButton_Click;
                scrcpyStatus.ActionButton = downloadBtn;
                scrcpyStatus.IsOpen = true;
            }
            else
            {
                startScrcpyButton.IsEnabled = true;
                stopScrcpyButton.IsEnabled = false;
                scrcpyStatus.Title = "scrcpy.exe Found";
                scrcpyStatus.Severity = InfoBarSeverity.Success;
                scrcpyStatus.Message = "scrcpy.exe was found, ready to mirror.";
                scrcpyStatus.ActionButton = null;
                scrcpyStatus.IsOpen = true;
            }
        }
        catch(Exception e)
        {
            startScrcpyButton.IsEnabled = false;
            stopScrcpyButton.IsEnabled = false;
            scrcpyStatus.Title = "scrcpy.exe Not Found";
            scrcpyStatus.Severity = InfoBarSeverity.Error;
            scrcpyStatus.Message = "scrcpy.exe was not found, do you want to download it?\n" + scrcpypath + "\n" + e.Message.ToString();
            var downloadBtn = new Button() { Content = "Download" };
            downloadBtn.Click += DownloadSCRCPYButton_Click;
            scrcpyStatus.ActionButton = downloadBtn;
            scrcpyStatus.IsOpen = true;
            
        }
    }
    public ScreenMirroringPage()
    {
        ViewModel = App.GetService<ScreenMirroringViewModel>();
        InitializeComponent();
        scrcpypath = ApplicationData.Current.RoamingFolder.Path + "/Assets/scrcpy/scrcpy-win64-v2.3.1/scrcpy.exe";
        checkForScrcpy();
    }

    private void StartScrcpyButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessStartInfo scrcpyStartInfo = new ProcessStartInfo();
        scrcpyStartInfo.FileName = scrcpypath;
        scrcpyStartInfo.Arguments = "-s " + ConnectionsPage.defaultDevice + (" -m" + RAMBox.Text) + ((bool)FPSBox.IsChecked?" --print-fps":"") + ((bool)MirrorAudioBox.IsChecked?"":" --no-audio-playback") + ((bool)FullscreenBox.IsChecked?" -f":"") + (" -b " + VideoBitrateBox.Text) + (CropBox.Text != ""?" --crop=" + CropBox.Text:"");
        scrcpyStartInfo.CreateNoWindow = true;
        scrcpyStartInfo.RedirectStandardOutput = true;
        scrcpyStartInfo.RedirectStandardError = true;
        scrcpyProcess = Process.Start(scrcpyStartInfo);
        scrcpyProcess.Exited += delegate
        {
            startScrcpyButton.IsEnabled = true;
            stopScrcpyButton.IsEnabled = false;
        };
        scrcpyProcess.OutputDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (e.Data != null)
                {
                    ScreenMirroringLog.Text += e.Data + "\n";
                }
            });
        };
        scrcpyProcess.BeginOutputReadLine();
        scrcpyProcess.ErrorDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (e.Data != null)
                {
                    ScreenMirroringLog.Text += e.Data + "\n";
                }
            });
        };
        scrcpyProcess.BeginErrorReadLine();
        startScrcpyButton.IsEnabled = false;
        stopScrcpyButton.IsEnabled = true;
    }
    private Process scrcpyProcess;
    private void StopScrcpyButton_Click(object sender, RoutedEventArgs e)
    {
        scrcpyProcess.Kill();
        startScrcpyButton.IsEnabled = true;
        stopScrcpyButton.IsEnabled = false;
    }
    void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        scrcpyDownloadBar.Value = e.ProgressPercentage;
        scrcpyDownloadStatus.Text = "Downloading... " + e.ProgressPercentage.ToString() + "%";
    }
    void wc_DownloadFinished(object sender, EventArgs e)
    {
        scrcpyDownloadStatus.Text = "Extracting scrcpy...";
        scrcpyDownloadBar.IsIndeterminate = true;
        Directory.CreateDirectory(ApplicationData.Current.RoamingFolder.Path + "/Assets/temp/scrcpy");
        System.IO.Compression.ZipFile.ExtractToDirectory(ApplicationData.Current.RoamingFolder.Path + "/Assets/temp/scrcpy-win64-v2.3.1.zip", ApplicationData.Current.RoamingFolder.Path + "/Assets/temp/scrcpy");
        scrcpyDownloadStatus.Text = "Preparing scrcpy...";
        Directory.Move(ApplicationData.Current.RoamingFolder.Path + "/Assets/temp/scrcpy", ApplicationData.Current.RoamingFolder.Path + "/Assets/scrcpy");
        scrcpyDownloadStatus.Text = "Cleaning Up...";
        Directory.Delete(ApplicationData.Current.RoamingFolder.Path + "/Assets/temp", true);
        scrcpyDownloadBar.Visibility = Visibility.Collapsed;
        scrcpyDownloadStatus.Visibility = Visibility.Collapsed;
        checkForScrcpy();
    }
    private void DownloadSCRCPYButton_Click(object sender, RoutedEventArgs e)
    {
        scrcpyDownloadBar.Visibility = Visibility.Visible;
        scrcpyDownloadStatus.Visibility = Visibility.Visible;
        scrcpyDownloadStatus.Text = "Deleting scrcpy Folder...";
        scrcpyDownloadBar.IsIndeterminate = true;
        if(Directory.Exists(ApplicationData.Current.RoamingFolder.Path + "/Assets/scrcpy"))
            Directory.Delete(ApplicationData.Current.RoamingFolder.Path + "/Assets/scrcpy", true );
        scrcpyDownloadStatus.Text = "Creating temp Folder...";
        var tempDir = Directory.CreateDirectory(ApplicationData.Current.RoamingFolder.Path + "/Assets/temp");
        scrcpyDownloadStatus.Text = "Starting Download...";
        scrcpyDownloadBar.IsIndeterminate = false;
        using (WebClient wc = new WebClient())
        {
            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
            wc.DownloadFileCompleted += wc_DownloadFinished;
            wc.DownloadFileAsync(
                // Param1 = Link of file
                new System.Uri("https://github.com/Genymobile/scrcpy/releases/download/v2.3.1/scrcpy-win64-v2.3.1.zip"),
                // Param2 = Path to save
                ApplicationData.Current.RoamingFolder.Path + "/Assets/temp/scrcpy-win64-v2.3.1.zip"
            );
        }
    }

    private void SetMetaQ2Button_Click(object sender, RoutedEventArgs e)
    {
        CropBox.Text = (string)((sender as Button).Tag);
    }
}
