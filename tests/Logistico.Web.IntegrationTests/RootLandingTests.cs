using System.Net;

namespace Logistico.Web.IntegrationTests;

/// <summary>The site root is the public marketing landing; the staff console lives under /admin.</summary>
[Collection("Api")]
public sealed class RootLandingTests(LogisticoApiFactory factory)
{
    [Fact]
    public async Task Root_serves_the_public_landing_page()
    {
        var anon = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await anon.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.ToString() ?? "", StringComparison.Ordinal);
        var html = await response.Content.ReadAsStringAsync();
        // The landing hero, not the app shell: proves the public page is served at the root.
        Assert.Contains("نوصّل شحناتك", html, StringComparison.Ordinal);
        Assert.Contains("/Account/Login", html, StringComparison.Ordinal);
    }
}
