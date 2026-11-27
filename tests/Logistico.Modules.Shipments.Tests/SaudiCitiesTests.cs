using Logistico.SharedKernel.Saudi;

namespace Logistico.Modules.Shipments.Tests;

public class SaudiCitiesTests
{
    [Fact]
    public void Every_city_is_well_formed_under_a_real_region_and_every_region_has_one()
    {
        var regions = SaudiRegions.All.Select(r => r.Code).ToHashSet();

        Assert.NotEmpty(SaudiCities.All);
        Assert.All(SaudiCities.All, c =>
        {
            Assert.Contains(c.RegionCode, regions);
            Assert.True(c.Code.Length is > 0 and <= 20, $"{c.Code} must be 1–20 chars");
            Assert.False(string.IsNullOrWhiteSpace(c.NameAr));
            Assert.False(string.IsNullOrWhiteSpace(c.NameEn));
        });

        // Codes are unique (saved prices key on them), and every region has at least one city.
        Assert.Equal(SaudiCities.All.Count, SaudiCities.All.Select(c => c.Code).Distinct().Count());
        Assert.All(regions, r => Assert.Contains(SaudiCities.All, c => c.RegionCode == r));
    }
}
