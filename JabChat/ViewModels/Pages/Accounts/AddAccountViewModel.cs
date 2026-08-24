using App.Messages;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using XMPP.Core;
using XMPP.Core.Address;

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
    public partial string ServerIp { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int ServerPort { get; set; } = 0;

    [ObservableProperty]
    public partial string Resource { get; set; }
        = Guid.CreateVersion7().ToString();

    [ObservableProperty]
    public partial bool ForceSsl { get; set; } = true;
    
    [ObservableProperty]
    public partial bool IsValidating { get; set; } = false;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial int ValidationCount { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial bool LastValidationResult { get; set; } = false;

    public bool CanAdd => ValidationCount == 0 && LastValidationResult;
    
    [RelayCommand]
    private void Cancel()
    {
        WeakReferenceMessenger.Default.Send<NavigatorPopMessage>();
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        
    }

    partial void OnUsernameChanged(string value)
    {
        _ = ValidateAsync();
    }

    partial void OnPasswordChanged(string value)
    {
        _ = ValidateAsync();
    }

    partial void OnHostChanged(string value)
    {
        _ = ValidateAsync();
    }

    partial void OnServerIpChanged(string value)
    {
        _ = ValidateAsync(true);
    }

    partial void OnServerPortChanged(int port)
    {
        _ = ValidateAsync(true);
    }

    partial void OnResourceChanged(string value)
    {
        _ = ValidateAsync();
    }

    partial void OnForceSslChanged(bool value)
    {
        _ = ValidateAsync();
    }
    
    private void SetValidationResult(bool result)
    {
        if (ValidationCount <= 0) return;
        
        ValidationCount--;
        if (ValidationCount <= 0) IsValidating = false;
        LastValidationResult = result;
    }

    // todo: move dispose async to interface

    private async Task OnClientConnected_Base(XmppClient client)
    {
        await client.Disconnect();
        
        client.OnClientConnected -= OnClientConnected;
        client.OnClientConnected -= OnClientConnectedNoChangeServer;
        client.ClientErrorRaised -= OnClientError;
        await client.DisposeAsync();
        
        Dispatcher.UIThread.Invoke(() => SetValidationResult(true));
    }
    
    private async void OnClientConnected(object? sender, EventArgs e)
    {
        if (sender is not XmppClient client) return;
        ServerIp = client.Address.Ip;
        ServerPort = client.Address.Port;
        await OnClientConnected_Base(client);
    }
    
    private async void OnClientConnectedNoChangeServer(object? sender, EventArgs e)
    {
        if (sender is not XmppClient client) return;
        await OnClientConnected_Base(client);
    }

    private void OnClientError(object? sender, EventArgs e)
    {
        if (sender is not XmppClient client) return;
        client.OnClientConnected -= OnClientConnectedNoChangeServer;
        client.OnClientConnected -= OnClientConnected;
        client.ClientErrorRaised -= OnClientError;
        
        Dispatcher.UIThread.Invoke(() => SetValidationResult(false));
    }

    private async Task ValidateAsync(bool serverSettingsChanged = false)
    {
        IsValidating = true;
        ValidationCount++;

        var builder = new XmppClientBuilder()
            .UseUsername(Username)
            .UsePassword(Password)
            .UseResourceForBinding(Resource)
            .UseHost(Host)
            .UseTcp();

        if (ForceSsl) builder.ForceTls();
        var builderResult = await builder.BuildAsync();
        if (!builderResult.IsT0)
        {
            SetValidationResult(false);
            return;
        }

        var client = builderResult.AsT0!;
        if (serverSettingsChanged)
            client.OnClientConnected += OnClientConnectedNoChangeServer;
        else
            client.OnClientConnected += OnClientConnected;
        client.ClientErrorRaised += OnClientError;
        
        var connectResult = await client.ConnectAsync();
        if (!connectResult.IsT0)
        {
            client.OnClientConnected -= OnClientConnectedNoChangeServer;
            client.OnClientConnected -= OnClientConnected;
            client.ClientErrorRaised -= OnClientError;
            await ((XmppClient)client).DisposeAsync();
            SetValidationResult(false);
        }
    }
}