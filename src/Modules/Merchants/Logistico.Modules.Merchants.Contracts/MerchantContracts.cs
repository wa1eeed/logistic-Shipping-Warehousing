using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Resources;
using Logistico.SharedKernel;
using Logistico.SharedKernel.Geo;

namespace Logistico.Modules.Merchants.Contracts;

public static class MerchantsApiRoutes
{
    public const string Merchants = "api/v1/merchants";
    public const string Lookup = Merchants + "/lookup";

    public static string ById(Guid id) => $"{Merchants}/{id}";
}

/// <summary>A seller's declared daily shipment volume, used to place them in a pricing tier.</summary>
public enum MerchantVolumeBand
{
    Under10,
    From10To50,
    From50To200,
    Over200,
}

public sealed record MerchantListItem(
    Guid Id,
    string Name,
    string? InstagramHandle,
    string ContactName,
    string Phone,
    string Governorate,
    bool IsActive,
    DateTimeOffset CreatedAt,
    MerchantVolumeBand DailyShipments = MerchantVolumeBand.Under10,
    bool SelfRegistered = false);

public sealed record MerchantDetails(
    Guid Id,
    string Name,
    string? InstagramHandle,
    string ContactName,
    string Phone,
    string Governorate,
    string? Address,
    string? Notes,
    bool IsActive,
    DateTimeOffset CreatedAt,
    uint Version,
    MerchantVolumeBand DailyShipments = MerchantVolumeBand.Under10,
    bool SelfRegistered = false);

/// <summary>Minimal seller data other modules may read (e.g. Shipments when booking an order).</summary>
public sealed record MerchantLookupItem(Guid Id, string Name, string? InstagramHandle, string Governorate, string Phone);

public sealed record CreateMerchantResponse(Guid Id);

/// <summary>Create or update a seller (e.g. an Instagram shop).</summary>
public sealed class MerchantRequest : IValidatableObject
{
    [Required(ErrorMessageResourceType = typeof(ValidationText), ErrorMessageResourceName = nameof(ValidationText.Required))]
    [StringLength(150, ErrorMessageResourceType = typeof(ValidationText), ErrorMessageResourceName = nameof(ValidationText.MaxLength))]
    public string Name { get; set; } = "";

    [RegularExpression(@"^@?[A-Za-z0-9._]{1,30}$", ErrorMessageResourceType = typeof(ValidationText), ErrorMessageResourceName = nameof(ValidationText.InstagramHandle))]
    public string? InstagramHandle { get; set; }

    [Required(ErrorMessageResourceType = typeof(ValidationText), ErrorMessageResourceName = nameof(ValidationText.Required))]
    [StringLength(120, ErrorMessageResourceType = typeof(ValidationText), ErrorMessageResourceName = nameof(ValidationText.MaxLength))]
    public string ContactName { get; set; } = "";

    [Required(ErrorMessageResourceType = typeof(ValidationText), ErrorMessageResourceName = nameof(ValidationText.Required))]
    public string Phone { get; set; } = "";

    [Required(ErrorMessageResourceType = typeof(ValidationText), ErrorMessageResourceName = nameof(ValidationText.Required))]
    public string Governorate { get; set; } = "IQ-BG";

    [StringLength(300, ErrorMessageResourceType = typeof(ValidationText), ErrorMessageResourceName = nameof(ValidationText.MaxLength))]
    public string? Address { get; set; }

    [StringLength(1000, ErrorMessageResourceType = typeof(ValidationText), ErrorMessageResourceName = nameof(ValidationText.MaxLength))]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>The seller's declared daily shipment volume (their pricing tier).</summary>
    public MerchantVolumeBand DailyShipments { get; set; } = MerchantVolumeBand.Under10;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!PhoneNumbers.IsValid(Phone))
        {
            yield return new(ValidationText.Phone, [nameof(Phone)]);
        }

        if (!PlaceCodes.IsValid(Governorate))
        {
            yield return new(ValidationText.Governorate, [nameof(Governorate)]);
        }
    }
}

/// <summary>Read access to sellers for other modules (in-process contract; implemented by the Merchants module).</summary>
public interface IMerchantDirectory
{
    Task<MerchantLookupItem?> FindActiveAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<MerchantLookupItem>> ListActiveAsync(CancellationToken cancellationToken);
}

public static class MerchantTexts
{
    private static readonly ResourceManager Resources = new("Logistico.Modules.Merchants.Contracts.Resources.MerchantTexts", typeof(MerchantTexts).Assembly);

    public static string Problem(string code) => Resources.GetString("Problem_" + code, CultureInfo.CurrentUICulture) ?? code;
}
