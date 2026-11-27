using System.Security.Claims;
using Logistico.SharedKernel.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Logistico.Web.Realtime;

/// <summary>
/// The one realtime hub for the operations console. Staff only; every connection joins the group of its own tenant,
/// read from the signed-in user's claim on the server — never from anything the client sends — so one company can
/// never receive another company's signals. The hub broadcasts only lightweight "something changed" signals that carry
/// no data (see <see cref="SignalROpsNotifier"/>); the client then refetches through the usual tenant-scoped API.
/// </summary>
[Authorize(StaffPolicies.Staff)]
internal sealed class OpsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (TenantGroupFor(Context.User) is { } group)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>The per-tenant group name. Derived only from the authenticated user, so it cannot be spoofed.</summary>
    internal static string Group(Guid tenantId) => $"tenant:{tenantId}";

    internal static string? TenantGroupFor(ClaimsPrincipal? user) =>
        Guid.TryParse(user?.FindFirst(LogisticoClaimTypes.TenantId)?.Value, out var tenantId) && tenantId != Guid.Empty
            ? Group(tenantId)
            : null;
}
