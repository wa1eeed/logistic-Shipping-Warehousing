using System.Net;
using Logistico.SharedKernel.Identity;

namespace Logistico.Web.IntegrationTests;

/// <summary>
/// The realtime hub is the one new way into the server, so it must be as locked down as the API: staff only, and the
/// tenant it signals is taken from the connection's own claim (OpsHub), never from the client. Here we prove the door
/// itself: anonymous and non-staff callers cannot even open a connection; staff can.
/// </summary>
[Collection("Api")]
public sealed class RealtimeHubTests(LogisticoApiFactory factory)
{
    private const string Negotiate = "/hubs/ops/negotiate";

    [Fact]
    public async Task Anonymous_callers_cannot_open_a_connection()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsync(Negotiate, null);

        // Refused with no connection handed out. The exact refusal code depends on the challenge scheme (the test host
        // adds the bearer scheme, which answers 400; in the deployed app the cookie scheme answers 401) — either way
        // there is no negotiated connection, which is what protects the hub.
        Assert.False(response.IsSuccessStatusCode);
        Assert.DoesNotContain("connectionId", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Signed_in_users_without_a_staff_role_cannot_open_a_connection()
    {
        using var client = factory.CreateUserClient(role: "");

        var response = await client.PostAsync(Negotiate, null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Staff_can_negotiate_a_connection()
    {
        using var client = factory.CreateUserClient(role: StaffRoles.Operations);

        var response = await client.PostAsync(Negotiate, null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("connectionId", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }
}
