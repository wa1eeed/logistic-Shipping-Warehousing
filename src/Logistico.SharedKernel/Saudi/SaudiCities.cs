using System.Globalization;

namespace Logistico.SharedKernel.Saudi;

/// <summary>A major Saudi city under one of the <see cref="SaudiRegions"/>.</summary>
public sealed record SaudiCity(string Code, string RegionCode, string NameEn, string NameAr)
{
    public string Name => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? NameAr : NameEn;
}

/// <summary>
/// The main cities of each Saudi region — a starting list a company can drop into its price list with one click,
/// then price or prune. Codes are stable (region code + a short tag) so saved prices survive edits.
/// </summary>
public static class SaudiCities
{
    public static readonly IReadOnlyList<SaudiCity> All =
    [
        new("SA-01-RUH", "SA-01", "Riyadh", "الرياض"),
        new("SA-01-KHJ", "SA-01", "Al-Kharj", "الخرج"),
        new("SA-01-DWD", "SA-01", "Al-Dawadmi", "الدوادمي"),
        new("SA-01-MJM", "SA-01", "Al-Majma'ah", "المجمعة"),

        new("SA-02-MAK", "SA-02", "Makkah", "مكة المكرمة"),
        new("SA-02-JED", "SA-02", "Jeddah", "جدة"),
        new("SA-02-TAF", "SA-02", "Taif", "الطائف"),
        new("SA-02-QUN", "SA-02", "Al-Qunfudhah", "القنفذة"),

        new("SA-03-MED", "SA-03", "Madinah", "المدينة المنورة"),
        new("SA-03-YNB", "SA-03", "Yanbu", "ينبع"),
        new("SA-03-ULA", "SA-03", "AlUla", "العلا"),

        new("SA-04-DMM", "SA-04", "Dammam", "الدمام"),
        new("SA-04-KHB", "SA-04", "Khobar", "الخبر"),
        new("SA-04-DHA", "SA-04", "Dhahran", "الظهران"),
        new("SA-04-HFF", "SA-04", "Al-Hofuf", "الهفوف"),
        new("SA-04-JUB", "SA-04", "Jubail", "الجبيل"),
        new("SA-04-QTF", "SA-04", "Qatif", "القطيف"),

        new("SA-05-BUR", "SA-05", "Buraydah", "بريدة"),
        new("SA-05-UNZ", "SA-05", "Unayzah", "عنيزة"),
        new("SA-05-RAS", "SA-05", "Ar-Rass", "الرس"),

        new("SA-06-HAS", "SA-06", "Ha'il", "حائل"),

        new("SA-07-TUU", "SA-07", "Tabuk", "تبوك"),
        new("SA-07-UMJ", "SA-07", "Umluj", "أملج"),

        new("SA-08-ARR", "SA-08", "Arar", "عرعر"),
        new("SA-08-RAF", "SA-08", "Rafha", "رفحاء"),

        new("SA-09-GIZ", "SA-09", "Jazan", "جازان"),
        new("SA-09-SAB", "SA-09", "Sabya", "صبيا"),

        new("SA-10-EAM", "SA-10", "Najran", "نجران"),

        new("SA-11-ABT", "SA-11", "Al-Bahah", "الباحة"),

        new("SA-12-AJF", "SA-12", "Sakaka", "سكاكا"),
        new("SA-12-QUR", "SA-12", "Al-Qurayyat", "القريات"),

        new("SA-14-AHB", "SA-14", "Abha", "أبها"),
        new("SA-14-KMS", "SA-14", "Khamis Mushait", "خميس مشيط"),
        new("SA-14-BSH", "SA-14", "Bisha", "بيشة"),
    ];

    public static IEnumerable<SaudiCity> ForRegion(string? regionCode) =>
        All.Where(c => string.Equals(c.RegionCode, regionCode, StringComparison.OrdinalIgnoreCase));
}
