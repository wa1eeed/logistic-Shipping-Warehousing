namespace Logistico.Web.Client.Services;

/// <summary>
/// The live link to the server's operations hub. It raises an event when the server signals that data changed, so the
/// shell and pages refetch at once instead of waiting for the polling fallback. The signals carry no data; a listener
/// fetches the change through the usual API. Best-effort: when the link is down the polling loop keeps the UI fresh.
/// </summary>
internal interface IOpsLive : IAsyncDisposable
{
    /// <summary>A hub scan was recorded: the live radar and today's hub counters changed.</summary>
    event Action? ScansChanged;

    /// <summary>The side-menu operational counters changed.</summary>
    event Action? CountersChanged;

    /// <summary>Opens the link once. Safe to call again; a second call while connected does nothing.</summary>
    Task StartAsync();
}
