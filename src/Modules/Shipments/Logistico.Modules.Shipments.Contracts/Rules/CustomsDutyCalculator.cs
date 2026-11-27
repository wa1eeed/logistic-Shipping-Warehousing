using System.Globalization;
using Logistico.SharedKernel.Geo;

namespace Logistico.Modules.Shipments.Contracts.Rules;

/// <summary>
/// Customs valuation, duty and compliance rules by operating country.
/// Iraq: ASYCUDA, duty in dinars at the official CBI rate, stamp and port fees
/// (docs/05-customs-clearance-and-iraq-compliance.md). Saudi Arabia: ZATCA/FASAH,
/// duty and 15% value-added tax in riyals at the SAMA peg.
/// </summary>
public static class CustomsDutyCalculator
{
    /// <summary>Central Bank of Iraq (CBI) official USD/IQD customs valuation rate.</summary>
    public const decimal OfficialUsdToIqdRate = 1_310m;

    /// <summary>SAMA USD/SAR peg used to value Saudi imports in riyals.</summary>
    public const decimal OfficialUsdToSarRate = 3.75m;

    /// <summary>Saudi value-added tax on imports (ZATCA), charged on the customs value plus the assessed duty.</summary>
    public const decimal SaudiVatRatePercent = 15m;

    public sealed record CustomsOfficeInfo(string Code, string NameEn, string NameAr)
    {
        public string Name => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? NameAr : NameEn;
    }

    /// <summary>Iraqi General Commission for Customs clearance offices (ASYCUDA).</summary>
    public static readonly IReadOnlyList<CustomsOfficeInfo> Offices =
    [
        new("IQUQR", "Umm Qasr North / South Port", "ميناء أم قصر الشمالي / الجنوبي"),
        new("IQBGW", "Baghdad International Airport", "مطار بغداد الدولي"),
        new("IQBSR", "Basra / Safwan Border", "منفذ البصرة / سفوان"),
        new("IQEBL", "Erbil / Ibrahim Khalil", "أربيل / إبراهيم الخليل"),
        new("IQISU", "Sulaymaniyah / Bashmakh", "السليمانية / باشماخ"),
        new("IQNJF", "Najaf International Airport", "مطار النجف الدولي"),
    ];

    /// <summary>Saudi Zakat, Tax and Customs Authority (ZATCA) ports of entry, cleared through FASAH.</summary>
    public static readonly IReadOnlyList<CustomsOfficeInfo> SaudiOffices =
    [
        new("SAJED", "Jeddah Islamic Port", "ميناء جدة الإسلامي"),
        new("SADMM", "King Abdulaziz Port, Dammam", "ميناء الملك عبدالعزيز بالدمّام"),
        new("SARUH", "Riyadh Dry Port", "ميناء الرياض الجاف"),
        new("SAKKIA", "King Khalid International Airport, Riyadh", "مطار الملك خالد الدولي بالرياض"),
        new("SAKAIA", "King Abdulaziz International Airport, Jeddah", "مطار الملك عبدالعزيز الدولي بجدة"),
        new("SABATHA", "Al Batha Land Port", "منفذ البطحاء البري"),
        new("SAHDTH", "Al Haditha Land Port", "منفذ الحديثة البري"),
        new("SAKFC", "King Fahd Causeway", "جسر الملك فهد"),
    ];

    /// <summary>Iraqi ASYCUDA declaration regimes (import for home use, warehousing, temporary admission, transit).</summary>
    public static readonly string[] Regimes = ["IM4", "IM5", "IM7", "TR8"];

    /// <summary>Saudi FASAH declaration types (import for home use, temporary admission, re-export, transit).</summary>
    public static readonly string[] SaudiRegimes = ["IMP", "TMP", "REX", "TRN"];

    /// <summary>The clearance offices for an operating country.</summary>
    public static IReadOnlyList<CustomsOfficeInfo> OfficesFor(string? country) =>
        Countries.IsSaudi(country) ? SaudiOffices : Offices;

    /// <summary>The declaration regimes for an operating country.</summary>
    public static string[] RegimesFor(string? country) =>
        Countries.IsSaudi(country) ? SaudiRegimes : Regimes;

    /// <summary>The default clearance office when the shipment's destination is not itself a customs office.</summary>
    public static string DefaultOfficeCode(string? country) =>
        Countries.IsSaudi(country) ? "SAJED" : "IQUQR";

    /// <summary>The official USD valuation rate for an operating country (CBI for Iraq, SAMA peg for Saudi Arabia).</summary>
    public static decimal RateFor(string? country) =>
        Countries.IsSaudi(country) ? OfficialUsdToSarRate : OfficialUsdToIqdRate;

    public static string OfficeNameOrCode(string? code) =>
        Offices.Concat(SaudiOffices).FirstOrDefault(o => string.Equals(o.Code, code, StringComparison.OrdinalIgnoreCase))?.Name ?? code ?? "";

    /// <summary>
    /// The customs value in US dollars of what the shipper declared: dollars as they are, dinars and riyals at the
    /// official rate. Other currencies, and no declared value, give 0: the clerk enters the dollar value from the invoice.
    /// </summary>
    public static decimal CustomsValueUsd(decimal? declaredValue, string? currency) => (declaredValue, currency?.ToUpperInvariant()) switch
    {
        ( > 0 and var value, "USD") => value,
        ( > 0 and var value, "IQD") => Math.Round(value / OfficialUsdToIqdRate, 2, MidpointRounding.AwayFromZero),
        ( > 0 and var value, "SAR") => Math.Round(value / OfficialUsdToSarRate, 2, MidpointRounding.AwayFromZero),
        _ => 0m,
    };

    /// <summary>The assessed customs duty in the operating country's settlement currency (IQD or SAR).</summary>
    public static decimal CalculateDuty(decimal customsValueUsd, decimal dutyRatePercent, string? country)
    {
        if (customsValueUsd <= 0 || dutyRatePercent <= 0)
        {
            return 0m;
        }

        return Math.Round(customsValueUsd * RateFor(country) * (dutyRatePercent / 100m), 0, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// The tax and fees in the operating country's settlement currency: Iraq charges stamp and port fees
    /// (minimum 25,000 IQD); Saudi Arabia charges 15% VAT on the customs value plus the assessed duty.
    /// </summary>
    public static decimal CalculateTaxAndFees(decimal customsValueUsd, decimal dutyRatePercent, string? country)
    {
        if (customsValueUsd <= 0)
        {
            return 0m;
        }

        if (Countries.IsSaudi(country))
        {
            var valueSar = customsValueUsd * OfficialUsdToSarRate;
            var dutySar = dutyRatePercent > 0 ? valueSar * (dutyRatePercent / 100m) : 0m;
            return Math.Round((valueSar + dutySar) * (SaudiVatRatePercent / 100m), 0, MidpointRounding.AwayFromZero);
        }

        return Math.Round(Math.Max(25_000m, customsValueUsd * OfficialUsdToIqdRate * 0.005m), 0, MidpointRounding.AwayFromZero);
    }

    /// <summary>Iraqi duty at the official CBI rate (kept for callers that are Iraq-only).</summary>
    public static decimal CalculateDutyIqd(decimal customsValueUsd, decimal dutyRatePercent) =>
        CalculateDuty(customsValueUsd, dutyRatePercent, Countries.Iraq);

    /// <summary>Iraqi stamp and port fees (kept for callers that are Iraq-only).</summary>
    public static decimal CalculateStampAndPortFeesIqd(decimal customsValueUsd) =>
        CalculateTaxAndFees(customsValueUsd, 0m, Countries.Iraq);

    /// <summary>
    /// A declaration can only be released when a declaration number is assigned and both Certificate of Origin (CoO)
    /// and Commercial Invoice are e-verified (Iraq: ASYCUDA / General Commission for Customs; Saudi: ZATCA / FASAH).
    /// </summary>
    public static bool CanRelease(string? declarationNumber, bool cooEVerified, bool invoiceEVerified) =>
        !string.IsNullOrWhiteSpace(declarationNumber) && cooEVerified && invoiceEVerified;
}
