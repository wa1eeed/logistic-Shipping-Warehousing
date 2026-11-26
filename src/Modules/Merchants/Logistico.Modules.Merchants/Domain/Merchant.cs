using Logistico.Modules.Merchants.Contracts;
using Logistico.SharedKernel.Geo;

namespace Logistico.Modules.Merchants.Domain;

/// <summary>A seller whose parcels we deliver, typically an Instagram shop paid by cash on delivery.</summary>
internal sealed class Merchant
{
    private Merchant()
    {
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = "";

    /// <summary>Lower-case handle without the leading @.</summary>
    public string? InstagramHandle { get; private set; }
    public string ContactName { get; private set; } = "";

    /// <summary>E.164, e.g. +9647701234567.</summary>
    public string Phone { get; private set; } = "";
    public string Governorate { get; private set; } = "";
    public string? Address { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>The seller's declared daily shipment volume (their pricing tier).</summary>
    public MerchantVolumeBand DailyShipments { get; private set; }

    /// <summary>True when the seller signed themselves up from the public site (not added by staff).</summary>
    public bool SelfRegistered { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public uint Version { get; private set; }

    public static Merchant Create(Guid tenantId, MerchantRequest request, DateTimeOffset now)
    {
        var merchant = new Merchant { Id = Guid.CreateVersion7(now), TenantId = tenantId, CreatedAt = now };
        merchant.Apply(request, now);
        return merchant;
    }

    /// <summary>A seller who signed up from the public site; active immediately and flagged for staff.</summary>
    public static Merchant CreateSelfRegistered(Guid tenantId, MerchantRequest request, DateTimeOffset now)
    {
        var merchant = Create(tenantId, request, now);
        merchant.SelfRegistered = true;
        return merchant;
    }

    public void Update(MerchantRequest request, DateTimeOffset now) => Apply(request, now);

    public static string? NormalizeHandle(string? handle) =>
        string.IsNullOrWhiteSpace(handle) ? null : handle.Trim().TrimStart('@').ToLowerInvariant();

    private void Apply(MerchantRequest request, DateTimeOffset now)
    {
        Name = request.Name.Trim();
        InstagramHandle = NormalizeHandle(request.InstagramHandle);
        ContactName = request.ContactName.Trim();
        Phone = PhoneNumbers.Normalize(request.Phone) ?? request.Phone.Trim();
        Governorate = request.Governorate.ToUpperInvariant();
        Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        IsActive = request.IsActive;
        DailyShipments = request.DailyShipments;
        UpdatedAt = now;
    }
}
