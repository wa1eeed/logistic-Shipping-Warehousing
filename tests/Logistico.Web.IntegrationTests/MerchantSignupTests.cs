using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Logistico.Modules.Merchants.Contracts;
using Logistico.SharedKernel;
using Logistico.SharedKernel.Identity;
using Logistico.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Logistico.Web.IntegrationTests;

/// <summary>A shop signs itself up from the public site and becomes a seller in the admin console.</summary>
public sealed partial class MerchantSignupTests(LogisticoApiFactory factory) : IClassFixture<LogisticoApiFactory>
{
    [Fact]
    public async Task A_shop_signs_itself_up_and_appears_among_the_sellers()
    {
        var browser = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var done = await SubmitAsync(browser, new()
        {
            ["Input.StoreName"] = "متجر الاختبار",
            ["Input.ContactName"] = "سارة محمد",
            ["Input.Phone"] = "07701234567",
            ["Input.Region"] = "IQ-BG",
            ["Input.DailyShipments"] = nameof(MerchantVolumeBand.From10To50),
            ["Input.Email"] = "newshop@example.com",
            ["Input.Password"] = "NewShop#2026",
            ["Input.ConfirmPassword"] = "NewShop#2026",
        });

        Assert.Equal(HttpStatusCode.Redirect, done.StatusCode);
        Assert.EndsWith("seller", done.Headers.Location!.OriginalString, StringComparison.Ordinal);

        // The shop now shows up among the sellers in the admin console, flagged as self-registered with its tier.
        var admin = factory.CreateUserClient(tenant: Tenants.Default);
        var list = (await admin.GetFromJsonAsync<PagedResult<MerchantListItem>>("/" + MerchantsApiRoutes.Merchants + "?q=" + Uri.EscapeDataString("متجر الاختبار")))!;
        var shop = Assert.Single(list.Items);
        Assert.True(shop.SelfRegistered);
        Assert.Equal(MerchantVolumeBand.From10To50, shop.DailyShipments);

        // The account is a seller linked to that shop.
        await using var scope = factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync("newshop@example.com");
        Assert.NotNull(user);
        Assert.True(await users.IsInRoleAsync(user, StaffRoles.Seller));
        Assert.Contains(await users.GetClaimsAsync(user), c => c.Type == LogisticoClaimTypes.MerchantId && c.Value == shop.Id.ToString());
    }

    internal static async Task<HttpResponseMessage> SubmitAsync(HttpClient browser, Dictionary<string, string> fields)
    {
        var page = await (await browser.GetAsync("/Account/Join")).Content.ReadAsStringAsync();
        var token = AntiforgeryToken().Match(page);
        var form = new Dictionary<string, string>(fields) { ["_handler"] = "join" };
        if (token.Success)
        {
            form["__RequestVerificationToken"] = WebUtility.HtmlDecode(token.Groups[1].Value);
        }

        return await browser.PostAsync("/Account/Join", new FormUrlEncodedContent(form));
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" value=\"([^\"]+)\"")]
    private static partial Regex AntiforgeryToken();
}
