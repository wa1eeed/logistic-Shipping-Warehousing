using Logistico.SharedKernel.Geo;
using Logistico.SharedKernel.Iraq;
using Logistico.SharedKernel.Saudi;

namespace Logistico.Web.Client.Ui.Map;

/// <summary>
/// The network map for the signed-in company's country: its region outlines, projection and region names. Iraq and
/// Saudi Arabia ship with built-in maps; any other country has none (the map is hidden). Chosen from
/// <see cref="HubCatalog.Country"/>, the same way routing picks the network.
/// </summary>
internal sealed record MapSource(
    double Width,
    double Height,
    IReadOnlyList<GovernorateShape> Shapes,
    Func<double, double, (double X, double Y)> Project,
    Func<double, double, (double Latitude, double Longitude)> Unproject,
    Func<string, string> NameOf)
{
    public static readonly MapSource Iraq = new(
        IraqMapShapes.Width, IraqMapShapes.Height, IraqMapShapes.Governorates,
        IraqMapShapes.Project, IraqMapShapes.Unproject, code => IraqGovernorates.Find(code)?.Name ?? code);

    public static readonly MapSource Saudi = new(
        SaudiMapShapes.Width, SaudiMapShapes.Height, SaudiMapShapes.Regions,
        SaudiMapShapes.Project, SaudiMapShapes.Unproject, code => SaudiRegions.Find(code)?.Name ?? code);

    /// <summary>The map for the signed-in company's country, defaulting to Iraq (also used before the network loads).</summary>
    public static MapSource Current => Countries.IsSaudi(HubCatalog.Country) ? Saudi : Iraq;

    /// <summary>Whether a built-in map exists for the company's country (Iraq or Saudi Arabia).</summary>
    public static bool Available => HubCatalog.UsesIraqCatalog || Countries.IsSaudi(HubCatalog.Country);
}
