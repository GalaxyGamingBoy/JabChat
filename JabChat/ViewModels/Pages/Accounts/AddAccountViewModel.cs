using App.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using XMPP.Core;

namespace App.ViewModels.Pages.Accounts;

public partial class AddAccountViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Username { get; set; } = "";

    [ObservableProperty]
    public partial string Host { get; set; } = "";
    
    [ObservableProperty]
    public partial string Password { get; set; } = "";

    [ObservableProperty]
    public partial string ServerHost { get; set; } = "";

    [ObservableProperty]
    public partial string ServerPort { get; set; } = "";

    [ObservableProperty]
    public partial string Resource { get; set; }
        = Guid.CreateVersion7().ToString();

    [ObservableProperty]
    public partial bool ForceSsl { get; set; } = true;
    
    [ObservableProperty]
    public partial bool IsValidating { get; set; } = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial bool IsAccountValid { get; set; } = false;

    private async Task CheckAccountValidity()
    {
        IsValidating = true;
        
        var builder = new XmppClientBuilder()
            .UseUsername(Username)
            .UsePassword(Password)
            .UseHost(Host)
            .UseResourceForBinding(Resource);

        if (ForceSsl)
            builder.ForceTls();

        var clientResult = await builder.BuildAsync();
        if (!clientResult.IsT0)
        {
            IsValidating = false;
            IsAccountValid = false;
            return;
        }

        var client = clientResult.AsT0;
        var connectResult = await client.ConnectAsync();
        if (!connectResult.IsT0)
        {
            IsValidating = false;
            IsAccountValid = false;
            return;
        }

        _ = await client.DisconnectWithStreamCloseAsync();
        
        IsAccountValid = true;
        IsValidating = false;
    }

    private bool CanAdd() => IsAccountValid;

    [RelayCommand]
    private void Cancel()
    {
        WeakReferenceMessenger.Default.Send<NavigatorPopMessage>();
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        
    }
}