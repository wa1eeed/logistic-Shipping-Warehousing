using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Logistico.Web.Client.Services;

/// <summary>
/// Connects to the server's <c>/hubs/ops</c> over SignalR (same-origin, so the sign-in cookie rides along and the
/// Content-Security-Policy's <c>connect-src 'self'</c> already allows it) and turns the server's signals into events.
/// Reconnects automatically; after a reconnection it refreshes everything once, since signals sent while the link was
/// down were missed. Opening the link is best-effort — if it never connects, the shell's polling loop still refreshes.
/// </summary>
internal sealed class OpsHubClient : IOpsLive
{
    private readonly HubConnection connection;

    public OpsHubClient(NavigationManager navigation)
    {
        connection = new HubConnectionBuilder()
            .WithUrl(navigation.ToAbsoluteUri("hubs/ops"))
            .WithAutomaticReconnect()
            .Build();
        connection.On("scans", RaiseScansChanged);
        connection.On("counters", RaiseCountersChanged);
        connection.Reconnected += _ =>
        {
            // Catch up on anything that changed while the link was down.
            RaiseScansChanged();
            RaiseCountersChanged();
            return Task.CompletedTask;
        };
    }

    public event Action? ScansChanged;

    public event Action? CountersChanged;

    public async Task StartAsync()
    {
        if (connection.State != HubConnectionState.Disconnected)
        {
            return;
        }

        try
        {
            await connection.StartAsync();
        }
        catch
        {
            // Best-effort: the shell's polling loop keeps the UI fresh until the link comes up (automatic reconnect).
        }
    }

    public ValueTask DisposeAsync() => connection.DisposeAsync();

    internal void RaiseScansChanged() => ScansChanged?.Invoke();

    internal void RaiseCountersChanged() => CountersChanged?.Invoke();
}
