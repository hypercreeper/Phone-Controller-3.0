using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using System.Text;
using Microsoft.UI.Xaml.Controls;

using Phone_Controller_3._0.ViewModels;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.DataTransfer;
using System.Diagnostics;
using Windows.Media.PlayTo;

namespace Phone_Controller_3._0.Views;

public sealed partial class FileExplorerPage : Page
{
    public FileExplorerViewModel ViewModel
    {
        get;
    }
    
    public FileExplorerPage()
    {
        ViewModel = App.GetService<FileExplorerViewModel>();
        InitializeComponent();
        refreshDirectories();
        
        new Thread(delegate()
        {
            while(true)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (RemoteFiles.SelectedItem != null)
                    {
                        DeleteBtn.IsEnabled = true;
                    }
                    else
                    {
                        DeleteBtn.IsEnabled = false;
                    }
                    if (LocalFiles.SelectedItem != null && RemoteFiles.SelectedItem != null)
                    {
                        PullFileBtn.IsEnabled = true;
                        PushFileBtn.IsEnabled = true;
                    }
                    else
                    {
                        PullFileBtn.IsEnabled = false;
                        PushFileBtn.IsEnabled = false;
                    }
                });
                Thread.Sleep(1000);
            }
        }).Start();
    }
    private void refreshDirectories(int sourceType = 5)
    {
        if (sourceType == SourceType.REMOTE || sourceType == 5)
        {
            var startinfo = new ProcessStartInfo() { FileName = Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            startinfo.Arguments = "-s " + ConnectionsPage.defaultDevice + " shell ls '" + RemotePath.Text + "'";
            new Thread(delegate ()
            {
                var process = Process.Start(startinfo);
                process.WaitForExit();
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                if (process.ExitCode != 0 && !error.Contains("/init: Permission denied"))
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        var contentdi = new ContentDialog();
                        contentdi.Title = "Error Occurred - ls";
                        contentdi.XamlRoot = this.XamlRoot;
                        contentdi.Content = error;
                        contentdi.PrimaryButtonText = "OK";
                        contentdi.ShowAsync();
                    });
                }
                else
                {
                    string line = output;
                    Collection<ListFile> remoteFiles = new Collection<ListFile>();

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        foreach (var file in line.Replace("\r", "").Split("\n"))
                        {
                            if (file != "")
                            {
                                var listFile = new ListFile(file, RemotePath.Text + "/" + file, SourceType.REMOTE);
                                remoteFiles.Add(listFile);
                            }
                        }
                        RemoteFiles.ItemsSource = remoteFiles;
                    });
                }
            }).Start();
        }
        if (sourceType == SourceType.LOCAL || sourceType == 5)
        {
            var fsEntries = Directory.EnumerateFileSystemEntries(LocalPath.Text);
            Collection<ListFile> localFiles = new Collection<ListFile>();
            foreach (var item in fsEntries)
            {
                localFiles.Add(new ListFile(Path.GetFileName(item), item, SourceType.LOCAL));
            }
            LocalFiles.ItemsSource = localFiles;
        }
    }
    private void PullFileBtn_Click(object sender, RoutedEventArgs e)
    {
        var startinfo = new ProcessStartInfo() { FileName = Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        startinfo.Arguments = "-s " + ConnectionsPage.defaultDevice + " pull \"" + (RemoteFiles.SelectedItem as ListFile).FilePath + "\" \"" + (LocalFiles.SelectedItem as ListFile).FilePath + "\"";
        new Thread(delegate ()
        {
            var process = Process.Start(startinfo);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var contentdi = new ContentDialog();
                    contentdi.Title = "Pull File - Error";
                    contentdi.XamlRoot = this.XamlRoot;
                    contentdi.Content = process.StandardError.ReadToEnd() + process.StandardOutput.ReadToEnd();
                    contentdi.PrimaryButtonText = "OK";
                    contentdi.ShowAsync();
                });
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var contentdi = new ContentDialog();
                    contentdi.Title = "Pull File";
                    contentdi.XamlRoot = this.XamlRoot;
                    contentdi.Content = "Pulled " + (RemoteFiles.SelectedItem as ListFile).FileName + " to " + (LocalFiles.SelectedItem as ListFile).FilePath;
                    contentdi.PrimaryButtonText = "OK";
                    contentdi.ShowAsync();
                });
            }
            DispatcherQueue.TryEnqueue(() =>
            {
                refreshDirectories(SourceType.LOCAL);
            });
        }).Start();
    }

    private void PushFileBtn_Click(object sender, RoutedEventArgs e)
    {
        var startinfo = new ProcessStartInfo() { FileName = Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        startinfo.Arguments = "-s " + ConnectionsPage.defaultDevice + " push \"" + (LocalFiles.SelectedItem as ListFile).FilePath + "\" \"" + (RemoteFiles.SelectedItem as ListFile).FilePath + "\"";
        new Thread(delegate ()
        {
            var process = Process.Start(startinfo);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var contentdi = new ContentDialog();
                    contentdi.Title = "Push File - Error";
                    contentdi.XamlRoot = this.XamlRoot;
                    contentdi.Content = process.StandardError.ReadToEnd();
                    contentdi.PrimaryButtonText = "OK";
                    contentdi.ShowAsync();
                });
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var contentdi = new ContentDialog();
                    contentdi.Title = "Push File";
                    contentdi.XamlRoot = this.XamlRoot;
                    contentdi.Content = "Pushed " + (LocalFiles.SelectedItem as ListFile).FileName + " to " + (RemoteFiles.SelectedItem as ListFile).FilePath;
                    contentdi.PrimaryButtonText = "OK";
                    contentdi.ShowAsync();
                });
            }
            DispatcherQueue.TryEnqueue(() =>
            {
                refreshDirectories(SourceType.REMOTE);
            });
        }).Start();
    }
    private async void CreateFolderBtn_Click(object sender, RoutedEventArgs e)
    {
        var foldername = new TextBox() { PlaceholderText = "Folder Name" };

        var dialog = new ContentDialog();
        dialog.Title = "Create Folder";
        dialog.XamlRoot = this.XamlRoot;
        var stackpanel = new StackPanel() { Orientation = Orientation.Vertical };
        stackpanel.Children.Add(new TextBlock() { Text = "Enter name of folder: ", Margin = new Thickness() { Bottom = 10 } });
        stackpanel.Children.Add(foldername);
        dialog.Content = stackpanel;
        dialog.SecondaryButtonText = "Create";
        dialog.PrimaryButtonText = "Cancel";
        dialog.DefaultButton = ContentDialogButton.Secondary;
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Secondary)
        {
            var startinfo = new ProcessStartInfo() { FileName = Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            startinfo.Arguments = "-s " + ConnectionsPage.defaultDevice + " shell mkdir '" + RemotePath.Text + "/" + foldername.Text + "'";
            new Thread(delegate ()
            {
                var process = Process.Start(startinfo);
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        var contentdi = new ContentDialog();
                        contentdi.Title = "Create Folder - Error";
                        contentdi.XamlRoot = this.XamlRoot;
                        contentdi.Content = process.StandardError.ReadToEnd();
                        contentdi.PrimaryButtonText = "OK";
                        contentdi.ShowAsync();
                    });
                }
                else
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        var contentdi = new ContentDialog();
                        contentdi.Title = "Create Folder";
                        contentdi.XamlRoot = this.XamlRoot;
                        contentdi.Content = "Created Folder " + RemotePath.Text + "/" + foldername.Text;
                        contentdi.PrimaryButtonText = "OK";
                        contentdi.ShowAsync();
                    });
                }
                DispatcherQueue.TryEnqueue(() =>
                {
                    refreshDirectories(SourceType.REMOTE);
                });
            }).Start();
        }
    }

    private async void DeleteBtn_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog();
        dialog.Title = "Delete " + ((RemoteFiles.SelectedItem as ListFile).isDirectory?"Folder":"File");
        dialog.XamlRoot = this.XamlRoot;
        if ((RemoteFiles.SelectedItem as ListFile).isDirectory)
            dialog.Content = "Are you sure you would like to delete " + (RemoteFiles.SelectedItem as ListFile).FileName + " and all of its contents?";
        else
            dialog.Content = "Are you sure you would like to delete " + (RemoteFiles.SelectedItem as ListFile).FileName;
        dialog.SecondaryButtonText = "Delete";
        dialog.PrimaryButtonText = "Cancel";
        dialog.DefaultButton = ContentDialogButton.Secondary;
        var result = await dialog.ShowAsync();
        if(result == ContentDialogResult.Secondary)
        {
            var startinfo = new ProcessStartInfo() { FileName = Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            startinfo.Arguments = "-s " + ConnectionsPage.defaultDevice + " shell rm -r '" + (RemoteFiles.SelectedItem as ListFile).FilePath + "'";
            new Thread(delegate ()
            {
                var process = Process.Start(startinfo);
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        var contentdi = new ContentDialog();
                        contentdi.Title = "Delete " + ((RemoteFiles.SelectedItem as ListFile).isDirectory ? "Folder" : "File") + " - Error";
                        contentdi.XamlRoot = this.XamlRoot;
                        contentdi.Content = process.StandardError.ReadToEnd();
                        contentdi.PrimaryButtonText = "OK";
                        contentdi.ShowAsync();
                    });
                }
                else
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        var contentdi = new ContentDialog();
                        contentdi.Title = "Delete " + ((RemoteFiles.SelectedItem as ListFile).isDirectory ? "Folder" : "File");
                        contentdi.XamlRoot = this.XamlRoot;
                        contentdi.Content = "Deleted " + (RemoteFiles.SelectedItem as ListFile).FileName;
                        contentdi.PrimaryButtonText = "OK";
                        contentdi.ShowAsync();
                    });
                }
                DispatcherQueue.TryEnqueue(() =>
                {
                    refreshDirectories(SourceType.REMOTE);
                });
            }).Start();
        }
    }

    private void RemoteFiles_ItemClick(object sender, ItemClickEventArgs e)
    {
        
    }
    private void LocalFiles_ItemClick(object sender, ItemClickEventArgs e)
    {
        
    }

    private void RemoteFiles_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        RemotePath.Text = (RemoteFiles.SelectedItem as ListFile).FilePath;
        refreshDirectories(SourceType.REMOTE);
    }

    private void LocalFiles_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        LocalPath.Text = (LocalFiles.SelectedItem as ListFile).FilePath;
        refreshDirectories(SourceType.LOCAL);
    }

    private void LocalPathBack_Click(object sender, RoutedEventArgs e)
    {
        if(LocalPath.Text.LastIndexOf("\\") != -1)
            LocalPath.Text = LocalPath.Text.Substring(0, LocalPath.Text.LastIndexOf("\\"));
        if(LocalPath.Text == "C:")
        {
            LocalPath.Text = "C:\\";
        }
        refreshDirectories(SourceType.LOCAL);
    }

    private void RemotePathBack_Click(object sender, RoutedEventArgs e)
    {
        if (RemotePath.Text.LastIndexOf("/") != 0)
            RemotePath.Text = RemotePath.Text.Substring(0, RemotePath.Text.LastIndexOf("/"));
        else
            RemotePath.Text = "/";
        refreshDirectories(SourceType.REMOTE);
    }

    private void RemotePath_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if(e.Key == Windows.System.VirtualKey.Enter)
        {
            refreshDirectories(SourceType.REMOTE);
        }
    }

    private void LocalPath_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            refreshDirectories(SourceType.LOCAL);
        }
    }
}
