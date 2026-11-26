using Logistico.Modules.Shipments.Contracts.Rules;
using Logistico.SharedKernel.Saudi;

namespace Logistico.Modules.Shipments.Tests;

/// <summary>Saudi Arabia is a built-in operating country: thirteen regions and a standard four-hub network.</summary>
public class SaudiNetworkTests
{
    [Fact]
    public void Has_the_thirteen_regions_with_arabic_names()
    {
        Assert.Equal(13, SaudiRegions.All.Count);
        Assert.Equal(13, SaudiRegions.All.Select(r => r.Code).Distinct().Count());
        Assert.All(SaudiRegions.All, r => Assert.False(string.IsNullOrWhiteSpace(r.NameAr)));
        Assert.True(SaudiRegions.IsKnown("sa-01"));
        Assert.Equal("المنطقة الشرقية", SaudiRegions.Find("SA-04")!.NameAr);
    }

    [Fact]
    public void The_standard_network_covers_every_region_exactly_once()
    {
        var covered = SaudiHubs.Standard.SelectMany(h => h.Covers).ToList();

        Assert.Equal(covered.Count, covered.Distinct().Count());
        Assert.All(SaudiRegions.All, r => Assert.Contains(r.Code, covered));
    }

    [Theory]
    [InlineData("SA-01", SaudiHubs.Riyadh)]   // Riyadh region → Riyadh hub
    [InlineData("SA-05", SaudiHubs.Riyadh)]   // Qassim → Riyadh hub
    [InlineData("SA-03", SaudiHubs.Jeddah)]   // Madinah → Jeddah hub
    [InlineData("SA-04", SaudiHubs.Dammam)]   // Eastern Province → Dammam hub
    [InlineData("SA-09", SaudiHubs.Abha)]     // Jazan → Abha hub
    public void Each_region_routes_to_its_hub(string region, string hub) =>
        Assert.Equal(hub, SaudiHubs.Default.ResolveDestinationHub(region).Code);

    [Fact]
    public void Riyadh_is_the_primary_hub()
    {
        Assert.Equal(SaudiHubs.Riyadh, SaudiHubs.Standard[0].Code);
        Assert.Equal("RUH", IraqHubs.TagOf(SaudiHubs.Riyadh));
    }
}
