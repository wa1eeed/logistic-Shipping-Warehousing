using Logistico.SharedKernel.Setup;

namespace Logistico.Web.Data;

/// <summary>A company's profile and its getting-started choices (steps it skipped), one row per company.</summary>
public class CompanyProfileRecord
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string Country { get; set; } = "IQ";
    public string? Governorate { get; set; }
    public string? Address { get; set; }

    /// <summary>The platform's brand colour (#RRGGBB); null keeps the default emerald.</summary>
    public string? BrandColor { get; set; }

    /// <summary>Getting-started steps the company said are not for it.</summary>
    public List<string> SkippedSteps { get; set; } = [];

    public DateTimeOffset UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public uint Version { get; set; }

    /// <summary>The profile has what customers and staff need to recognise and reach the company.</summary>
    public bool IsSet => Name.Length > 0 && Phone is not null;

    public CompanyProfile Read() => new() { Name = Name, Phone = Phone, Country = string.IsNullOrEmpty(Country) ? "IQ" : Country, Governorate = Governorate, Address = Address, BrandColor = BrandColor };

    public void Write(CompanyProfile profile, string? by, DateTimeOffset now)
    {
        Name = profile.Name.Trim();
        Phone = Clean(profile.Phone);
        Country = string.IsNullOrWhiteSpace(profile.Country) ? "IQ" : profile.Country.Trim().ToUpperInvariant();
        Governorate = Clean(profile.Governorate)?.ToUpperInvariant();
        Address = Clean(profile.Address);
        BrandColor = CleanHex(profile.BrandColor);
        UpdatedAt = now;
        UpdatedBy = by;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>A valid #RRGGBB colour (lower-cased), or null — never arbitrary text that could reach a stylesheet.</summary>
    private static string? CleanHex(string? value)
    {
        var v = value?.Trim();
        return v is { Length: 7 } && System.Text.RegularExpressions.Regex.IsMatch(v, "^#[0-9a-fA-F]{6}$") ? v.ToLowerInvariant() : null;
    }
}
