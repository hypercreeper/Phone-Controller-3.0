using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Phone_Controller_3._0.Views;
using Windows.Devices.Bluetooth.Advertisement;

namespace Phone_Controller_3._0
{
    class ListFile
    {
        public string FileName
        {
            get; private set;
        }
        public string FilePath
        {
            get; private set;
        }
        public int Source
        {
            get; private set;
        }
        public ListFile(string filename, string filepath, int source)
        {
            FileName = filename;
            FilePath = filepath;
            Source = source;
        }
        public bool isDirectory
        {
            get
            {
                if (this.Source == SourceType.LOCAL)
                {
                    try
                    {
                        var file = new DirectoryInfo(this.FilePath);
                        if (File.GetAttributes(this.FilePath).HasFlag(FileAttributes.Directory))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                {
                    var startinfo = new ProcessStartInfo() { FileName = Windows.ApplicationModel.Package.Current.InstalledPath + "/Assets/platform-tools/adb", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
                    startinfo.Arguments = "-s " + ConnectionsPage.defaultDevice + " shell ls '" + this.FilePath + "'";
                    var process = Process.Start(startinfo);
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        //var contentdi = new ContentDialog();
                        //contentdi.Title = "Push File - Error";
                        //contentdi.XamlRoot = new;
                        //contentdi.Content = process.StandardError.ReadToEnd();
                        //contentdi.PrimaryButtonText = "OK";
                        //contentdi.ShowAsync();
                        return false;
                    }
                    else
                    {
                        if (process.StandardOutput.ReadToEnd().Replace("\r", "").Replace("\n", "") == this.FilePath)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
        }
        public string FileIcon
        {
            get
            {
                if(this.isDirectory)
                {
                    return "\uE8B7"; // Folder Icon
                }
                else
                {
                    return "\uE8A5"; // File Icon
                }
            }
        }
        public async Task getFilesAsync()
        {
        
        }
    }
}
