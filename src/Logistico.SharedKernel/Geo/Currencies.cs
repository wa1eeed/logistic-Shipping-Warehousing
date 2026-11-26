namespace Logistico.SharedKernel.Geo;

/// <summary>The currency a company settles in, chosen from its operating country: Iraqi dinar or Saudi riyal.</summary>
public static class Currencies
{
    public const string Iraqi = "IQD";
    public const string Saudi = "SAR";

    /// <summary>The currency code for an operating country (IQD by default, SAR for Saudi Arabia).</summary>
    public static string For(string? country) => Countries.IsSaudi(country) ? Saudi : Iraqi;

    /// <summary>
    /// The currency's display symbol: the Iraqi dinar (د.ع) and the Saudi riyal sign (U+20C1, Unicode 17.0) in Arabic,
    /// the ISO code otherwise. The riyal sign needs a font that carries U+20C1 to render instead of a tofu box.
    /// </summary>
    public static string Symbol(string? currency, bool arabic) => currency switch
    {
        Iraqi => arabic ? "د.ع" : "IQD",
        Saudi => arabic ? "⃁" : "SAR",
        "USD" => "$",
        null or "" => "",
        _ => currency,
    };

    /// <summary>Whole-unit currencies shown without decimals (IQD and SAR in this app's demo and pricing).</summary>
    public static bool IsWhole(string? currency) => currency is Iraqi or Saudi;
}
