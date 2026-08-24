using App.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using XMPP.Core;
using XMPP.Core.Errors;
using XMPP.Core.EventArgs;
using XMPP.Core.SaslErrors;

namespace App.ViewModels.Pages.Accounts;

public partial class AddAccountViewModel : ViewModelBase
{
    public enum ClientValidationStatus
    {
        None, 
        Generic,
        Unauthorized,
        BindError,
        ConnectError,
        BuildError
    }
    
    [ObservableProperty]
    public partial string Username { get; set; } = "";

    [ObservableProperty]
    public partial string Host { get; set; } = "";
    
    [ObservableProperty]
    public partial string Password { get; set; } = "";

    [ObservableProperty]
    public partial string ServerIp { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ServerPort { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Resource { get; set; }
        = Guid.CreateVersion7().ToString();

    [ObservableProperty]
    public partial bool ForceSsl { get; set; } = true;
    
    [ObservableProperty]
    public partial bool IsValidating { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasValidationError))]
    [NotifyPropertyChangedFor(nameof(ValidationMessage))]
    public partial ClientValidationStatus ValidationStatus { get; set; }
        = ClientValidationStatus.None;

    public bool HasValidationError => ValidationStatus != ClientValidationStatus.None;
    
    public string ValidationMessage => GetValidationErrorMessage(ValidationStatus);
        
    private bool CanAdd() => !IsValidating;
    
    [RelayCommand]
    private void Cancel()
    {
        WeakReferenceMessenger.Default.Send<NavigatorPopMessage>();
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private async Task AddAsync()
    {
        await ValidateAsync();
        Console.WriteLine(HasValidationError);
    }

    private static string GetValidationErrorMessage(ClientValidationStatus status) => status switch
    {
        ClientValidationStatus.None => string.Empty,
        ClientValidationStatus.Generic => Lang.Resources.AddAccountPage_ValidationError_Generic,
        ClientValidationStatus.Unauthorized => Lang.Resources.AddAccountPage_ValidationError_Unauthorized,
        ClientValidationStatus.BindError => Lang.Resources.AddAccountPage_ValidationError_BindError,
        ClientValidationStatus.ConnectError => Lang.Resources.AddAccountPage_ValidationError_ConnectError,
        ClientValidationStatus.BuildError => Lang.Resources.AddAccountPage_ValidationError_BuildError,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    private async Task ValidateAsync()
    {
        IsValidating = true;

        try
        {
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
                ValidationStatus = ClientValidationStatus.BuildError;
                return;
            }

            var client = builderResult.AsT0!;
            var tsk = new TaskCompletionSource<ClientValidationStatus>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.OnClientConnected += OnConnected;
            client.ClientErrorRaised += OnError;

            var connectResult = await client.ConnectAsync();
            if (!connectResult.IsT0)
            {
                ValidationStatus = ClientValidationStatus.ConnectError;
                return;
            }

            ValidationStatus = await tsk.Task;

            if (client.State == XmppState.Connected)
            {
                ServerIp = client.Address.Ip;
                ServerPort = client.Address.Port.ToString();
            }

            client.OnClientConnected -= OnConnected;
            client.ClientErrorRaised -= OnError;
            
            if (client.State == XmppState.Connected)
                await ((XmppClient)client).DisposeAsync();

            void OnConnected(object? sender, EventArgs args)
            {
                tsk.TrySetResult(ClientValidationStatus.None);
            }

            void OnError(object? sender, ClientErrorRaisedEventArgs args)
            {
                if (args.Error is NotAuthorized or MalformedRequest)
                    tsk.TrySetResult(ClientValidationStatus.Unauthorized);
                else if (args.Error is BindError)
                    tsk.TrySetResult(ClientValidationStatus.BindError);
                else
                    tsk.TrySetResult(ClientValidationStatus.Generic);
            }
        }
        finally
        {
            IsValidating = false;
        }
    }
}