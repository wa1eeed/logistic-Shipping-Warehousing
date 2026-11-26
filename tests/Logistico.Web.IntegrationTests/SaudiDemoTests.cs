using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Logistico.Modules.Merchants.Contracts;
using Logistico.Modules.Shipments.Contracts;
using Logistico.Modules.Shipments.Contracts.Rules;
using Logistico.SharedKernel;
using Logistico.SharedKernel.Identity;

namespace Logistico.Web.IntegrationTests;

/// <summary>The demo seeded for a Saudi company (Seed:Country=SA): Saudi regions, hubs, riyals and +966 numbers.</summary>
public sealed class SaudiDemoFactory : LogisticoApiFactory
{
    public const int Orders = 120;

    protected override IEnumerable<(string Key, string Value)> ExtraSettings =>
    [
        ("Seed:DemoShipments", Orders.ToString(CultureInfo.InvariantCulture)),
        ("Seed:Country", "SA"),
        ("Seed:AdminEmail", "admin@logistico.local"),
        ("Seed:AdminPassword", "Logistico#2026"),
        ("Seed:DemoTeam", "true"),
    ];
}

/// <summary>The Saudi demo is complete and consistent: Saudi network and places, orders at every stage, and money that adds up.</summary>
public sealed class SaudiDemoTests(SaudiDemoFactory factory) : IClassFixture<SaudiDemoFactory>
{
    private HttpClient Admin => factory.CreateUserClient(tenant: Tenants.Default);

    [Fact]
    public async Task The_saudi_demo_is_complete_and_local()
    {
        // The network is the built-in Saudi one, and every place is a Saudi region.
        var warehouses = (await Admin.GetFromJsonAsync<WarehouseList>("/" + ShipmentsApiRoutes.Warehouses))!;
        Assert.Equal("SA", warehouses.Country);
        Assert.Contains(warehouses.Items, w => w.Code == SaudiHubs.Riyadh);
        Assert.All(warehouses.Places ?? [], p => Assert.StartsWith("SA-", p.Code, StringComparison.Ordinal));

        // Orders at many stages, all delivering inside Saudi Arabia, with no Iraq-only international freight.
        var all = (await Admin.GetFromJsonAsync<ShipmentListResult>("/" + ShipmentsApiRoutes.Shipments + "?pageSize=200"))!;
        Assert.True(all.TotalCount >= SaudiDemoFactory.Orders, $"only {all.TotalCount} orders");
        Assert.True(all.Items.Select(s => s.Status).Distinct().Count() >= 7, "orders should reach many stages");
        Assert.DoesNotContain(all.Items, s => s.ServiceType == ServiceType.International);
        Assert.All(all.Items, s => Assert.StartsWith("SA-", s.DestinationCode, StringComparison.Ordinal));

        // Riyals and Saudi mobile numbers on the cash-on-delivery orders.
        var cod = all.Items.Where(s => s.CodAmount > 0).ToList();
        Assert.NotEmpty(cod);
        Assert.All(cod, s => Assert.Equal("SAR", s.CodCurrency));
        Assert.Contains(cod, s => s.ConsigneePhone is { } p && p.StartsWith("+9665", StringComparison.Ordinal));

        // Couriers, cash and seller balances all exist and connect.
        var fleet = (await Admin.GetFromJsonAsync<List<CourierLookupItem>>("/" + ShipmentsApiRoutes.FleetLookup))!;
        Assert.NotEmpty(fleet);
        var finance = (await Admin.GetFromJsonAsync<FinanceSummary>("/" + ShipmentsApiRoutes.FinanceSummary))!;
        Assert.NotEmpty(finance.SellerBalances);
    }

    [Fact]
    public async Task A_saudi_shop_books_to_saudi_regions_in_riyals()
    {
        // Link a seller account to one of the Saudi demo shops.
        var merchants = (await Admin.GetFromJsonAsync<PagedResult<MerchantListItem>>("/api/v1/merchants?pageSize=1"))!;
        var shop = Assert.Single(merchants.Items);
        var seller = factory.CreateUserClient("saudi-seller@logistico.test", Tenants.Default, StaffRoles.Seller, merchantId: shop.Id);

        // The order form's destination list is the company's own regions — every one Saudi, none Iraqi.
        var places = (await seller.GetFromJsonAsync<NetworkHubs>("/" + ShipmentsApiRoutes.SellerPlaces))!;
        Assert.Equal("SA", places.Country);
        Assert.NotEmpty(places.Places!);
        Assert.All(places.Places!, p => Assert.StartsWith("SA-", p.Code, StringComparison.Ordinal));

        // Booking to one of those regions succeeds (no "needs governorate" rejection) and settles cash on delivery in riyals.
        var order = ApiTestHelpers.DomesticOrder(Guid.Empty, "عميل سعودي");
        order.DestinationCode = places.Places![0].Code;
        order.ConsigneePhone = "0501234567";
        order.CodAmount = 450;
        var created = await seller.PostAsJsonAsync("/" + ShipmentsApiRoutes.SellerOrders, order);
        Assert.True(created.StatusCode == HttpStatusCode.Created, await created.Content.ReadAsStringAsync());
        var booked = (await created.Content.ReadFromJsonAsync<CreateShipmentResponse>())!;

        var all = (await Admin.GetFromJsonAsync<ShipmentListResult>("/" + ShipmentsApiRoutes.Shipments + "?pageSize=200"))!;
        var mine = all.Items.Single(s => s.TrackingNumber == booked.TrackingNumber);
        Assert.Equal("SAR", mine.CodCurrency);
        Assert.StartsWith("SA-", mine.DestinationCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(ShipmentsApiRoutes.Warehouses)]
    [InlineData(ShipmentsApiRoutes.NetworkHubs)]
    [InlineData(ShipmentsApiRoutes.Transfers)]
    [InlineData(ShipmentsApiRoutes.Returns)]
    [InlineData(ShipmentsApiRoutes.CashHandovers)]
    [InlineData(ShipmentsApiRoutes.FinanceSummary)]
    [InlineData(ShipmentsApiRoutes.AutomationOverview)]
    [InlineData("api/v1/merchants")]
    public async Task Every_core_screen_reads_the_saudi_demo(string route)
    {
        var response = await Admin.GetAsync("/" + route);
        Assert.True(response.IsSuccessStatusCode, $"{route}: {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
    }
}
