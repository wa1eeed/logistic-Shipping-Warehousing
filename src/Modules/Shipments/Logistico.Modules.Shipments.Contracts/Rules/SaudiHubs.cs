namespace Logistico.Modules.Shipments.Contracts.Rules;

/// <summary>
/// The standard Saudi network a company operating in Saudi Arabia starts with: four sorting hubs that together cover all
/// thirteen regions. Companies add, move and retire warehouses afterwards; routing always goes through a <see cref="HubNetwork"/>.
/// </summary>
public static class SaudiHubs
{
    public const string Riyadh = "HUB-RUH";
    public const string Jeddah = "HUB-JED";
    public const string Dammam = "HUB-DMM";
    public const string Abha = "HUB-ABH";

    /// <summary>The four standard hubs, Riyadh first (the primary hub).</summary>
    public static IReadOnlyList<HubInfo> Standard { get; } =
    [
        new(Riyadh, "RUH", "SA-01", "مركز فرز الرياض المركزي", "Riyadh Central Hub")
        {
            Latitude = 24.7136, Longitude = 46.6753, Covers = ["SA-01", "SA-05", "SA-06", "SA-08", "SA-12"],
        },
        new(Jeddah, "JED", "SA-02", "مركز فرز جدة والغرب", "Jeddah Western Hub")
        {
            Latitude = 21.4858, Longitude = 39.1925, Covers = ["SA-02", "SA-03", "SA-07", "SA-11"],
        },
        new(Dammam, "DMM", "SA-04", "مركز فرز الدمام والشرقية", "Dammam Eastern Hub")
        {
            Latitude = 26.4207, Longitude = 50.0888, Covers = ["SA-04"],
        },
        new(Abha, "ABH", "SA-14", "مركز فرز أبها والجنوب", "Abha Southern Hub")
        {
            Latitude = 18.2465, Longitude = 42.5117, Covers = ["SA-14", "SA-09", "SA-10"],
        },
    ];

    /// <summary>The standard Saudi network (used to start a Saudi company and in tests).</summary>
    public static HubNetwork Default { get; } = new(Standard, iraqCatalog: false, knownPlaces: SaudiRegionCodes);

    private static IEnumerable<string> SaudiRegionCodes => Standard.SelectMany(h => h.Covers).Distinct();
}
