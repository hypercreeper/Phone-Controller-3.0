using Microsoft.UI.Xaml;

using Phone_Controller_3._0.Contracts.Services;
using Phone_Controller_3._0.ViewModels;

namespace Phone_Controller_3._0.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        _navigationService.NavigateTo(typeof(DeviceControlViewModel).FullName!, args.Arguments);

        await Task.CompletedTask;
    }
}
