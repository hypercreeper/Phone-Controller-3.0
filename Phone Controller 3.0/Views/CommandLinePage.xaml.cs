using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;

using Phone_Controller_3._0.ViewModels;

namespace Phone_Controller_3._0.Views;

public sealed partial class CommandLinePage : Page
{
    public CommandLineViewModel ViewModel
    {
        get;
    }

    public CommandLinePage()
    {
        ViewModel = App.GetService<CommandLineViewModel>();
        InitializeComponent();
    }
    private List<string> PreviousCommands = new List<string>();
    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var suitableItems = new List<string>();
            var splitText = sender.Text.ToLower().Split(" ");
            foreach (var package in PreviousCommands)
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
        cmdLineInput.Text = args.SelectedItem.ToString();
        cmdLineInput.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }
    private void runFunction()
    {
        cmdLineOutput.Text = "";
        cmdLineInput.IsEnabled = false;
        sendCommand.IsEnabled = false;
        if(!PreviousCommands.Contains(cmdLineInput.Text))
            PreviousCommands.Add(cmdLineInput.Text);
        var startinfo = new ProcessStartInfo() { WorkingDirectory = "C:\\platform-tools", CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        startinfo.FileName = "cmd";
        startinfo.Arguments = "/c " + cmdLineInput.Text;
        cmdLineProgressIndicator.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        new Thread(delegate ()
        {
            var process = Process.Start(startinfo);
            process.OutputDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (e.Data != null)
                    {
                        cmdLineOutput.Text += e.Data + "\n";
                    }
                });
            };
            process.BeginOutputReadLine();
            process.ErrorDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (e.Data != null)
                    {
                        cmdLineOutput.Text += e.Data + "\n";
                    }
                });
            };
            process.BeginErrorReadLine();
            process.WaitForExit();
            DispatcherQueue.TryEnqueue(() =>
            {
                cmdLineProgressIndicator.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                cmdLineInput.Text = "";
                cmdLineInput.IsEnabled = true;
                sendCommand.IsEnabled = true;
            });
    }).Start();
    }
    private void cmdLineInput_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if(e.Key == Windows.System.VirtualKey.Enter)
        {
            runFunction();
        }
    }

    private void sendCommand_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        runFunction();
    }
}
