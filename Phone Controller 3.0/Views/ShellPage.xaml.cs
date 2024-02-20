using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using Phone_Controller_3._0.Contracts.Services;
using Phone_Controller_3._0.Helpers;
using Phone_Controller_3._0.ViewModels;
using Windows.Foundation;
using Windows.System;
using Microsoft.UI.Input;
using System.Diagnostics;
using System.Net;

namespace Phone_Controller_3._0.Views;

// TODO: Update NavigationViewItem titles and icons in ShellPage.xaml.
public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel
    {
        get;
    }

    public static TextBlock appTitleBar;
    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        // TODO: Set the title bar icon by updating /Assets/WindowIcon.ico.
        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        //var nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(window.AppWindow.Id);

        //// textbox on titlebar area
        //var txtBoxNonClientArea = UIHelper.FindElementByName(sender as UIElement, "AppTitleBarTextBox") as FrameworkElement;
        //GeneralTransform transformTxtBox = txtBoxNonClientArea.TransformToVisual(null);
        //Rect bounds = transformTxtBox.TransformBounds(new Rect(0, 0, txtBoxNonClientArea.ActualWidth, txtBoxNonClientArea.ActualHeight));

        //// Windows.Graphics.RectInt32[] rects defines the area which allows click throughs in custom titlebar
        //// it is non dpi-aware client coordinates. Hence, we convert dpi aware coordinates to non-dpi coordinates
        //var scale = WindowHelper.GetRasterizationScaleForElement(this);
        //var transparentRect = new Windows.Graphics.RectInt32(
        //    _X: (int)Math.Round(bounds.X * scale),
        //    _Y: (int)Math.Round(bounds.Y * scale),
        //    _Width: (int)Math.Round(bounds.Width * scale),
        //    _Height: (int)Math.Round(bounds.Height * scale)
        //);
        //var rects = new Windows.Graphics.RectInt32[] { transparentRect };

        //nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rects);
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();
        appTitleBar = AppTitleBarStatus;
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
        App.AppTitlebar = AppTitleBarText as UIElement;

    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }
}
