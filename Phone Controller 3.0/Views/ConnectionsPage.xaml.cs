using System.Diagnostics;
using System.Drawing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Phone_Controller_3._0.Models;
using Phone_Controller_3._0.ViewModels;
using Windows.Storage;

namespace Phone_Controller_3._0.Views;

public sealed partial class ConnectionsPage : Page
{
    public ConnectionsViewModel ViewModel
    {
        get;
    }

    public ConnectionsPage()
    {
        ViewModel = App.GetService<ConnectionsViewModel>();
        InitializeComponent();
        RefreshConnectedDevicesList_Click(RefreshConnectedDevicesList, new Microsoft.UI.Xaml.RoutedEventArgs());
    }
    public static string defaultDevice = "";
    private void usbConnectBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var startinfo = new ProcessStartInfo() { FileName = ApplicationData.Current.RoamingFolder.Path + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        startinfo.Arguments = "usb";
        new Thread(delegate ()
        {
            var process = Process.Start(startinfo);
            process.WaitForExit();
            DispatcherQueue.TryEnqueue(() =>
            {
                ConnectionsLog.Text += process.StandardOutput.ReadToEnd();
                ConnectionsLog.Text += process.StandardError.ReadToEnd();
            });
            if (process.ExitCode == 0)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var dialog = new ContentDialog();
                    dialog.XamlRoot = this.XamlRoot;
                    dialog.Content = "Reconnected USB Devices to ADB";
                    dialog.Title = "USB Connection";
                    dialog.PrimaryButtonText = "OK";
                    dialog.ShowAsync();
                });
            }
        }).Start();
    }
    private string[] previouslyConnectedIPs = new string[] { "Connect WSA" };
    private void IPPortConnectBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (!previouslyConnectedIPs.Contains(IPPortConnectBox.Text))
        {
            previouslyConnectedIPs = previouslyConnectedIPs.Append(IPPortConnectBox.Text).ToArray();
        }
        var startinfo = new ProcessStartInfo() { FileName = ApplicationData.Current.RoamingFolder.Path + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        if (IPPortConnectBox.Text == "Connect WSA")
        {
            startinfo.Arguments = "connect localhost:58526";
        }
        else
        {
            startinfo.Arguments = "connect " + IPPortConnectBox.Text;
        }
        //IPPortConnectBox.Text = "";
        IPPortConnectBox.ItemsSource = previouslyConnectedIPs;
        new Thread(delegate ()
        {
            var process = Process.Start(startinfo);
            DispatcherQueue.TryEnqueue(() =>
            {
                IPPortProgressIndicator.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            });
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            DispatcherQueue.TryEnqueue(() =>
            {
                ConnectionsLog.Text += output;
                ConnectionsLog.Text += error;
                IPPortProgressIndicator.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            });
            if (process.ExitCode == 0)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    if(output.Contains("cannot")) {
                        var dialog = new ContentDialog();
                        dialog.XamlRoot = (sender as Button).XamlRoot;
                        dialog.Title = "Wireless Connection - Error";
                        dialog.Content = output;
                        dialog.PrimaryButtonText = "OK";
                        dialog.ShowAsync();
                    }
                    else
                    {
                        var dialog = new ContentDialog();
                        dialog.XamlRoot = (sender as Button).XamlRoot;
                        dialog.Title = "Wireless Connection";
                        if (IPPortConnectBox.Text == "Connect WSA")
                            dialog.Content = "Connected Successfully to WSA";
                        else
                            dialog.Content = "Connected Successfully to " + IPPortConnectBox.Text;
                        dialog.PrimaryButtonText = "OK";
                        dialog.ShowAsync();
                    }
                });
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var dialog = new ContentDialog();
                    dialog.XamlRoot = (sender as Button).XamlRoot;
                    dialog.Title = "Wireless Connection - Error";
                    dialog.Content = error;
                    dialog.PrimaryButtonText = "OK";
                    dialog.ShowAsync();
                });
            }
        }).Start();
    }
    PairDeviceInfo[] PreviouslyConnectedPairedDevices = { };
    private void PairDeviceBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (!PreviouslyConnectedPairedDevices.Contains(new PairDeviceInfo(PairDeviceIPBox.Text, PairDevicePortBox.Text, PairDeviceCodeBox.Text)))
        {
            PreviouslyConnectedPairedDevices.Append(new PairDeviceInfo(PairDeviceIPBox.Text, PairDevicePortBox.Text, PairDeviceCodeBox.Text));
        }
        var startinfo = new ProcessStartInfo() { FileName = ApplicationData.Current.RoamingFolder.Path + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        startinfo.Arguments = "pair " + PairDeviceIPBox.Text + ":" + PairDevicePortBox.Text + " " + PairDeviceCodeBox.Text;
        var ips = new string[] { };
        var ports = new string[] { };
        foreach (var PairedDevice in PreviouslyConnectedPairedDevices)
        {
            ips.Append(PairedDevice.IP);
            ports.Append(PairedDevice.Port);
        }
        PairDeviceIPBox.ItemsSource = ips;
        PairDevicePortBox.ItemsSource = ports;
        new Thread(delegate ()
        {
            var process = Process.Start(startinfo);
            DispatcherQueue.TryEnqueue(() =>
            {
                PairDeviceProgressIndicator.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            });
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            DispatcherQueue.TryEnqueue(() =>
            {
                ConnectionsLog.Text += output;
                ConnectionsLog.Text += error;
                PairDeviceProgressIndicator.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            });
            if (process.ExitCode == 0)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (output.Contains("cannot"))
                    {
                        var dialog = new ContentDialog();
                        dialog.XamlRoot = (sender as Button).XamlRoot;
                        dialog.Title = "Wireless Connection - Error";
                        dialog.Content = output;
                        dialog.PrimaryButtonText = "OK";
                        dialog.ShowAsync();
                    }
                    else
                    {
                        var dialog = new ContentDialog();
                        dialog.XamlRoot = (sender as Button).XamlRoot;
                        dialog.Title = "Wireless Connection";
                        if (IPPortConnectBox.Text == "Connect WSA")
                            dialog.Content = "Connected Successfully to WSA";
                        else
                            dialog.Content = "Connected Successfully to " + IPPortConnectBox.Text;
                        dialog.PrimaryButtonText = "OK";
                        dialog.ShowAsync();
                    }
                });
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var dialog = new ContentDialog();
                    dialog.XamlRoot = (sender as Button).XamlRoot;
                    dialog.Title = "Wireless Connection - Error";
                    dialog.Content = error;
                    dialog.PrimaryButtonText = "OK";
                    dialog.ShowAsync();
                });
            }
        }).Start();
    }

    private void StartServerBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var startinfo = new ProcessStartInfo() { FileName = ApplicationData.Current.RoamingFolder.Path + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        startinfo.Arguments = "start-server";
        new Thread(delegate ()
        {
            var process = Process.Start(startinfo);
            DispatcherQueue.TryEnqueue(() =>
            {
                StartServerProgressIndicator.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            });
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            DispatcherQueue.TryEnqueue(() =>
            {
                ConnectionsLog.Text += output;
                ConnectionsLog.Text += error;
                StartServerProgressIndicator.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            });
            if (process.ExitCode == 0)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var dialog = new ContentDialog();
                    dialog.XamlRoot = (sender as Button).XamlRoot;
                    dialog.Title = "ADB Server";
                    dialog.Content = "ADB Server Started Successfully";
                    dialog.PrimaryButtonText = "OK";
                    dialog.ShowAsync();
                });
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var dialog = new ContentDialog();
                    dialog.XamlRoot = (sender as Button).XamlRoot;
                    dialog.Title = "ADB Server - Error";
                    dialog.Content = error;
                    dialog.PrimaryButtonText = "OK";
                    dialog.ShowAsync();
                });
            }
        }).Start();
    }

    private void StopServerBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var startinfo = new ProcessStartInfo() { FileName = ApplicationData.Current.RoamingFolder.Path + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        startinfo.Arguments = "kill-server";
        new Thread(delegate ()
        {
            var process = Process.Start(startinfo);
            DispatcherQueue.TryEnqueue(() =>
            {
                StopServerProgressIndicator.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            });
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            DispatcherQueue.TryEnqueue(() =>
            {
                ConnectionsLog.Text += output;
                ConnectionsLog.Text += error;
                StopServerProgressIndicator.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            });
            if (process.ExitCode == 0)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var dialog = new ContentDialog();
                    dialog.XamlRoot = (sender as Button).XamlRoot;
                    dialog.Title = "ADB Server";
                    dialog.Content = "ADB Server Stopped Successfully";
                    dialog.PrimaryButtonText = "OK";
                    dialog.ShowAsync();
                });
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var dialog = new ContentDialog();
                    dialog.XamlRoot = (sender as Button).XamlRoot;
                    dialog.Title = "ADB Server - Error";
                    dialog.Content = error;
                    dialog.PrimaryButtonText = "OK";
                    dialog.ShowAsync();
                });
            }
        }).Start();
    }

    private void RefreshConnectedDevicesList_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var startinfo = new ProcessStartInfo() { FileName = ApplicationData.Current.RoamingFolder.Path + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        startinfo.Arguments = "devices";
        new Thread(delegate ()
        {
            var process = Process.Start(startinfo);
            DispatcherQueue.TryEnqueue(() =>
            {
                RefreshConnectedDevicesProgressIndicator.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            });
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            DispatcherQueue.TryEnqueue(() =>
            {
                ConnectionsLog.Text += error;
                RefreshConnectedDevicesProgressIndicator.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            });
            if (process.ExitCode == 0)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    output = output.Replace("\r", "");
                    output = output.Replace("List of devices attached", "");
                    output = output.Replace("* daemon not running; starting now at tcp:5037\n* daemon started successfully", "");
                    output = output.Trim();
                    var devices = output.Split("\n");
                    //var temp = devices.ToList();
                    //temp.RemoveAt(0);
                    //devices = temp.ToArray();
                    var header = new ListViewHeaderItem() { Content = "Devices" };
                    ConnectedDevicesList.Items.Clear();
                    ConnectedDevicesList.Items.Add(header);
                    foreach (var device in devices)
                    {
                        var dev = new ListViewItem();
                        var stackpanel = new StackPanel();
                        stackpanel.Orientation = Orientation.Horizontal;
                        var text = new TextBlock();
                        var temptxt = device.Replace("device", "");
                        temptxt = temptxt.Replace("offline", "");
                        temptxt = temptxt.Replace("unauthorized", "");
                        temptxt = temptxt.Trim();
                        text.Text = temptxt;
                        text.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
                        var disconnectButton = new Button();
                        disconnectButton.Content = new FontIcon() { Glyph = "\uE711" };
                        disconnectButton.Margin = new Microsoft.UI.Xaml.Thickness() { Left = 10 };
                        disconnectButton.Tag = temptxt;
                        disconnectButton.Click += DisconnectDeviceButton_Click;
                        var setDeviceButton = new Button();
                        setDeviceButton.Content = new FontIcon() { Glyph = "\uE703" };
                        setDeviceButton.Margin = new Microsoft.UI.Xaml.Thickness() { Left = 10 };
                        setDeviceButton.Tag = temptxt;
                        setDeviceButton.Click += SetDeviceAsDefaultButton_Click;
                        stackpanel.Children.Add(text);
                        stackpanel.Children.Add(setDeviceButton);
                        stackpanel.Children.Add(disconnectButton);
                        dev.Content = stackpanel;
                        if (device.Contains("device"))
                        {
                            dev.Background = new SolidColorBrush() { Color = Windows.UI.Color.FromArgb(100, 0, 255, 0) };
                        }
                        else if(device.Contains("offline"))
                        {
                            dev.Background = new SolidColorBrush() { Color = Windows.UI.Color.FromArgb(100, 255, 0, 0) };
                        }
                        else if(device.Contains("unauthorized"))
                        {
                            dev.Background = new SolidColorBrush() { Color = Windows.UI.Color.FromArgb(100, 255, 0, 255) };
                        }
                        else
                        {
                            dev.Background = new SolidColorBrush() { Color = Windows.UI.Color.FromArgb(100, 255, 255, 0) };
                        }
                        if(device != "")
                            ConnectedDevicesList.Items.Add(dev);
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
    private void DisconnectDeviceButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var btn = (sender as Button);
        var startinfo = new ProcessStartInfo() { FileName = ApplicationData.Current.RoamingFolder.Path + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        startinfo.Arguments = "disconnect " + btn.Tag;
        new Thread(delegate ()
        {
            var process = Process.Start(startinfo);
            DispatcherQueue.TryEnqueue(() =>
            {
                btn.IsEnabled = false;
            });
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            DispatcherQueue.TryEnqueue(() =>
            {
                ConnectionsLog.Text += output;
                ConnectionsLog.Text += error;
                if(process.ExitCode != 0)
                    btn.IsEnabled = true;
            });
            if (process.ExitCode == 0)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var dialog = new ContentDialog();
                    dialog.XamlRoot = (sender as Button).XamlRoot;
                    dialog.Title = "Disconnect Device";
                    dialog.Content = "Disconneted " + btn.Tag + " Successfully";
                    dialog.PrimaryButtonText = "OK";
                    dialog.ShowAsync();
                });
                RefreshConnectedDevicesList_Click(sender, e);
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var dialog = new ContentDialog();
                    dialog.XamlRoot = (sender as Button).XamlRoot;
                    dialog.Title = "ADB Devices - Error";
                    dialog.Content = error;
                    dialog.PrimaryButtonText = "OK";
                    dialog.ShowAsync();
                });
                RefreshConnectedDevicesList_Click(sender, e);
            }
        }).Start();
    }
    private void SetDeviceAsDefaultButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var btn = sender as Button;
        defaultDevice = (string)btn.Tag;
        var dialog = new ContentDialog();
        dialog.XamlRoot = (sender as Button).XamlRoot;
        dialog.Title = "Set Default Device";
        dialog.Content = "Now Using " + defaultDevice;
        dialog.PrimaryButtonText = "OK";
        dialog.ShowAsync();
    }
}
