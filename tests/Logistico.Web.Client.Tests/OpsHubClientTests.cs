using Logistico.Web.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Logistico.Web.Client.Tests;

public sealed class OpsHubClientTests : ClientTestContext
{
    [Fact]
    public void Hub_signals_become_events_the_shell_can_listen_to()
    {
        var navigation = Services.GetRequiredService<NavigationManager>();
        var client = new OpsHubClient(navigation);
        var scans = 0;
        var counters = 0;
        client.ScansChanged += () => scans++;
        client.CountersChanged += () => counters++;

        client.RaiseScansChanged();
        client.RaiseScansChanged();
        client.RaiseCountersChanged();

        Assert.Equal(2, scans);
        Assert.Equal(1, counters);
    }
}
