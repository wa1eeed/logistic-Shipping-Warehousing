namespace Logistico.Modules.Shipments.Contracts;

/// <summary>
/// Announces that operational data changed for the current tenant, so live screens refresh at once instead of waiting
/// for the next poll. The signals carry no data: a listener fetches the change through the usual tenant-scoped API, so
/// no operational data leaves the module through this channel and no company can see another company's activity.
/// The host wires this to a realtime transport (SignalR); outside the host it is a no-op (see NullOpsNotifier).
/// </summary>
public interface IOpsNotifier
{
    /// <summary>A hub scan was recorded, so the live radar and today's hub counters changed.</summary>
    Task ScansChangedAsync(CancellationToken ct = default);

    /// <summary>The side-menu operational counters (alerts, requests, confirmations, …) changed.</summary>
    Task CountersChangedAsync(CancellationToken ct = default);
}
