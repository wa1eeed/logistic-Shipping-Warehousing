using System.Globalization;

namespace Logistico.SharedKernel.Saudi;

/// <summary>A Saudi administrative region (منطقة إدارية); <see cref="Latitude"/>/<see cref="Longitude"/> are its capital city.</summary>
public sealed record SaudiRegion(string Code, string NameEn, string NameAr, double Latitude, double Longitude)
{
    public string Name => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? NameAr : NameEn;
}

/// <summary>Saudi Arabia's thirteen regions, coded with ISO 3166-2:SA (there is no SA-13; 'Asir is SA-14).</summary>
public static class SaudiRegions
{
    public static readonly IReadOnlyList<SaudiRegion> All =
    [
        new("SA-01", "Riyadh", "الرياض", 24.7136, 46.6753),
        new("SA-02", "Makkah", "مكة المكرمة", 21.3891, 39.8579),
        new("SA-03", "Madinah", "المدينة المنورة", 24.4686, 39.6142),
        new("SA-04", "Eastern Province", "المنطقة الشرقية", 26.4207, 50.0888),
        new("SA-05", "Al-Qassim", "القصيم", 26.3260, 43.9750),
        new("SA-06", "Ha'il", "حائل", 27.5219, 41.6907),
        new("SA-07", "Tabuk", "تبوك", 28.3835, 36.5662),
        new("SA-08", "Northern Borders", "الحدود الشمالية", 30.9843, 41.0404),
        new("SA-09", "Jazan", "جازان", 16.8892, 42.5611),
        new("SA-10", "Najran", "نجران", 17.4933, 44.1277),
        new("SA-11", "Al-Bahah", "الباحة", 20.0129, 41.4677),
        new("SA-12", "Al-Jawf", "الجوف", 29.9697, 40.2000),
        new("SA-14", "'Asir", "عسير", 18.2465, 42.5117),
    ];

    private static readonly Dictionary<string, SaudiRegion> ByCode = All.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase);

    public static bool IsKnown(string? code) => code is not null && ByCode.ContainsKey(code);

    public static SaudiRegion? Find(string? code) => code is not null && ByCode.TryGetValue(code, out var r) ? r : null;

    /// <summary>Region name in the current UI language, or the code itself for places that are not Saudi regions.</summary>
    public static string NameOrCode(string? code) => Find(code)?.Name ?? code ?? "";
}
