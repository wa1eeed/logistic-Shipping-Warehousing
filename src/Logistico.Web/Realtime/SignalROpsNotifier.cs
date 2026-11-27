using Logistico.Modules.Shipments.Contracts;
using Logistico.SharedKernel;
using Microsoft.AspNetCore.SignalR;

namespace Logistico.Web.Realtime;

/// <summary>
/// Sends the module's "ops changed" signals to the current tenant's group over SignalR. The signals carry no data, and
/// the target group comes from the request's own tenant (<see cref="ITenantContext"/>), so no operational data leaves
/// the server through this channel and no company is signalled about another company's activity.
/// </summary>
internal sealed class SignalROpsNotifier(IHubContext<OpsHub> hub, ITenantContext tenant) : IOpsNotifier
{
    public Task ScansChangedAsync(CancellationToken ct = default) => SignalAsync("scans", ct);

    public Task CountersChangedAsync(CancellationToken ct = default) => SignalAsync("counters", ct);

    private Task SignalAsync(string signal, CancellationToken ct) =>
        hub.Clients.Group(OpsHub.Group(tenant.TenantId)).SendAsync(signal, ct);
}
