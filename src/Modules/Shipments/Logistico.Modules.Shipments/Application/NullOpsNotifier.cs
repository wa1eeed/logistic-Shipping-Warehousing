using Logistico.Modules.Shipments.Contracts;

namespace Logistico.Modules.Shipments.Application;

/// <summary>
/// The default notifier when the host wires no realtime transport (unit and integration tests, tooling): the module
/// works exactly as before, falling back to the client's polling. The host replaces this with a SignalR-backed one.
/// </summary>
internal sealed class NullOpsNotifier : IOpsNotifier
{
    public Task ScansChangedAsync(CancellationToken ct = default) => Task.CompletedTask;

    public Task CountersChangedAsync(CancellationToken ct = default) => Task.CompletedTask;
}
