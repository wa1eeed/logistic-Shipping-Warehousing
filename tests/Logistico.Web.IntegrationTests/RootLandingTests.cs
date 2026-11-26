using System.Net;
using System.Text.RegularExpressions;

namespace Logistico.Web.IntegrationTests;

/// <summary>The site root is the public marketing landing; the staff console lives under /admin.</summary>
[Collection("Api")]
public sealed partial class RootLandingTests(LogisticoApiFactory factory)
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
        // The landing's primary call to action opens the account gateway, not the signup form directly.
        Assert.Contains("/Account/Start", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Start_gateway_offers_both_sign_in_and_sign_up()
    {
        var anon = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await anon.GetAsync("/Account/Start");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        // The gateway presents the welcome and both paths — create an account and sign in — plus the no-account tracking route.
        Assert.Contains("أهلاً بك في لوجستيكو", html, StringComparison.Ordinal);
        Assert.Contains("Account/Join", html, StringComparison.Ordinal);
        Assert.Contains("Account/Login", html, StringComparison.Ordinal);
        Assert.Contains("\"track\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Landing_inline_script_runs_under_the_content_security_policy()
    {
        var anon = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await anon.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        // The script the map, scroll reveals and tracking box depend on carries this response's CSP nonce, so the browser
        // runs it instead of blocking it — the placeholder must be gone and the nonce must match the policy header.
        Assert.DoesNotContain("__CSP_NONCE__", html, StringComparison.Ordinal);
        var scriptNonce = ScriptNonce().Match(html);
        Assert.True(scriptNonce.Success, "the landing script should carry a nonce");
        var csp = Assert.Single(response.Headers.GetValues("Content-Security-Policy"));
        Assert.Contains($"'nonce-{scriptNonce.Groups[1].Value}'", csp, StringComparison.Ordinal);
    }

    [GeneratedRegex("<script nonce=\"([0-9A-F]+)\"")]
    private static partial Regex ScriptNonce();
}
